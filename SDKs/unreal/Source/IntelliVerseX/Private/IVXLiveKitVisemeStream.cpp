// IVXLiveKitVisemeStream — Unreal implementation of the LiveKit
// `viseme.v1` data-channel receiver. See header for the integration
// pattern and wire contract.

#include "IVXLiveKitVisemeStream.h"
#include "Dom/JsonObject.h"
#include "Dom/JsonValue.h"
#include "Serialization/JsonReader.h"
#include "Serialization/JsonSerializer.h"
#include "Animation/AnimSingleNodeInstance.h"

namespace
{
    static EIVXBlendshapeProfile ProfileFromInt(int32 Raw)
    {
        switch (Raw)
        {
            case 1: return EIVXBlendshapeProfile::Arkit52;
            case 2: return EIVXBlendshapeProfile::Ovr60;
            case 3: return EIVXBlendshapeProfile::Vrm69;
            default: return EIVXBlendshapeProfile::None;
        }
    }

    static EIVXVisemeSource SourceFromInt(int32 Raw)
    {
        switch (Raw)
        {
            case 1: return EIVXVisemeSource::Agent;
            case 2: return EIVXVisemeSource::UserFace;
            case 3: return EIVXVisemeSource::UserTts;
            case 4: return EIVXVisemeSource::Fallback;
            default: return EIVXVisemeSource::Unspecified;
        }
    }

    static int32 GetIntField(const TSharedPtr<FJsonObject>& Obj, const TCHAR* Key, int32 Fallback = 0)
    {
        int32 Value = Fallback;
        if (Obj.IsValid())
        {
            Obj->TryGetNumberField(Key, Value);
        }
        return Value;
    }

    static int64 GetInt64Field(const TSharedPtr<FJsonObject>& Obj, const TCHAR* Key, int64 Fallback = 0)
    {
        int64 Value = Fallback;
        if (Obj.IsValid())
        {
            double AsDouble = static_cast<double>(Fallback);
            if (Obj->TryGetNumberField(Key, AsDouble))
            {
                Value = static_cast<int64>(AsDouble);
            }
        }
        return Value;
    }

    static FString GetStringField(const TSharedPtr<FJsonObject>& Obj, const TCHAR* Key)
    {
        FString Out;
        if (Obj.IsValid()) Obj->TryGetStringField(Key, Out);
        return Out;
    }
}

UIVXLiveKitVisemeStream::UIVXLiveKitVisemeStream() {}

void UIVXLiveKitVisemeStream::Dispatch(const TArray<uint8>& Payload, bool bIsJson)
{
    if (Payload.Num() == 0) return;

    if (!bIsJson)
    {
        // Phase-4 ships JSON-on-wire; binary proto reserved for v1.1.
        return;
    }

    FString Text;
    Text.AppendChars(reinterpret_cast<const ANSICHAR*>(Payload.GetData()), Payload.Num());
    HandleJsonEnvelope(Text);
}

void UIVXLiveKitVisemeStream::ResetStream(const FString& /*Reason*/)
{
    bIsActive = false;
    CurrentLineId = 0;
    LastIntensityPct = 0;
    LastFrameSeq = 0;
    DroppedFrames = 0;
    ZeroOutMorphTargets();
}

void UIVXLiveKitVisemeStream::SetArkit52NameMap(const TArray<FName>& OrderedNames)
{
    BlendshapeNameMap.Reset();
    const int32 Count = FMath::Min(OrderedNames.Num(), 52);
    for (int32 i = 0; i < Count; ++i)
    {
        BlendshapeNameMap.Add(i, OrderedNames[i]);
    }
}

FString UIVXLiveKitVisemeStream::DiagnosticsJson() const
{
    return FString::Printf(
        TEXT("{\"topic\":\"viseme.v1\",\"isActive\":%s,\"currentLineId\":%lld,\"lastFrameSeq\":%lld,\"lastIntensityPct\":%d,\"droppedFrames\":%d}"),
        bIsActive ? TEXT("true") : TEXT("false"),
        CurrentLineId, LastFrameSeq, LastIntensityPct, DroppedFrames);
}

void UIVXLiveKitVisemeStream::HandleJsonEnvelope(const FString& Text)
{
    TSharedPtr<FJsonObject> Root;
    const auto Reader = TJsonReaderFactory<TCHAR>::Create(Text);
    if (!FJsonSerializer::Deserialize(Reader, Root) || !Root.IsValid()) return;

    FString Kind;
    if (!Root->TryGetStringField(TEXT("kind"), Kind)) return;

    if (Kind == TEXT("header"))
    {
        const TSharedPtr<FJsonObject> Body = Root->GetObjectField(TEXT("header"));
        if (!Body.IsValid()) return;
        FIVXVisemeStreamHeader H;
        H.UserId         = GetStringField(Body, TEXT("user_id"));
        H.TrackId        = GetStringField(Body, TEXT("track_id"));
        H.Source         = SourceFromInt(GetIntField(Body, TEXT("source"), 1));
        H.LineId         = GetInt64Field(Body, TEXT("line_id"));
        H.ExpectedFrames = GetIntField(Body, TEXT("expected_frames"));
        H.SampleRateHz   = GetIntField(Body, TEXT("sample_rate_hz"), 24000);
        H.FrameHz        = GetIntField(Body, TEXT("frame_hz"), 60);
        H.Profile        = ProfileFromInt(GetIntField(Body, TEXT("profile"), 1));

        bIsActive = true;
        CurrentLineId = H.LineId;
        LastFrameSeq = 0;
        DroppedFrames = 0;
        OnHeader.Broadcast(H);
        return;
    }

    if (Kind == TEXT("frame"))
    {
        const TSharedPtr<FJsonObject> Body = Root->GetObjectField(TEXT("frame"));
        if (!Body.IsValid()) return;
        FIVXVisemeFrame F;
        F.UserId       = GetStringField(Body, TEXT("user_id"));
        F.Profile      = ProfileFromInt(GetIntField(Body, TEXT("profile"), 1));
        F.AudioSeq     = GetInt64Field(Body, TEXT("audio_seq"));
        F.AudioTsMs    = GetInt64Field(Body, TEXT("audio_ts_ms"));
        F.IntensityPct = GetIntField(Body, TEXT("intensity_pct"), 100);
        F.FrameSeq     = GetInt64Field(Body, TEXT("frame_seq"));

        const TArray<TSharedPtr<FJsonValue>>* WeightsArr = nullptr;
        if (Body->TryGetArrayField(TEXT("blendshapes"), WeightsArr) && WeightsArr)
        {
            F.Blendshapes.Reserve(WeightsArr->Num());
            for (const TSharedPtr<FJsonValue>& V : *WeightsArr)
            {
                F.Blendshapes.Add(static_cast<uint8>(FMath::Clamp(static_cast<int32>(V->AsNumber()), 0, 255)));
            }
        }

        if (F.FrameSeq < LastFrameSeq)
        {
            DroppedFrames++;
            return;
        }
        LastFrameSeq = F.FrameSeq;
        LastIntensityPct = F.IntensityPct;
        ApplyToMesh(F);
        OnFrame.Broadcast(F);
        return;
    }

    if (Kind == TEXT("footer"))
    {
        const TSharedPtr<FJsonObject> Body = Root->GetObjectField(TEXT("footer"));
        if (!Body.IsValid()) return;
        FIVXVisemeStreamFooter Foot;
        Foot.UserId        = GetStringField(Body, TEXT("user_id"));
        Foot.LineId        = GetInt64Field(Body, TEXT("line_id"));
        Foot.FramesSent    = GetIntField(Body, TEXT("frames_sent"));
        Foot.FinalAudioSeq = GetInt64Field(Body, TEXT("final_audio_seq"));

        OnFooter.Broadcast(Foot);
        bIsActive = false;
        ZeroOutMorphTargets();
        return;
    }
}

void UIVXLiveKitVisemeStream::ApplyToMesh(const FIVXVisemeFrame& Frame)
{
    if (!TargetMesh || Frame.Blendshapes.Num() == 0 || BlendshapeNameMap.Num() == 0) return;

    for (int32 ArkitIdx = 0; ArkitIdx < Frame.Blendshapes.Num(); ++ArkitIdx)
    {
        const FName* Name = BlendshapeNameMap.Find(ArkitIdx);
        if (!Name || Name->IsNone()) continue;
        const float Weight = static_cast<float>(Frame.Blendshapes[ArkitIdx]) / 255.0f; // 0..1 morph value.
        TargetMesh->SetMorphTarget(*Name, Weight);
    }
}

void UIVXLiveKitVisemeStream::ZeroOutMorphTargets()
{
    if (!TargetMesh || BlendshapeNameMap.Num() == 0) return;
    for (const TPair<int32, FName>& Kvp : BlendshapeNameMap)
    {
        if (!Kvp.Value.IsNone())
        {
            TargetMesh->SetMorphTarget(Kvp.Value, 0.0f);
        }
    }
}
