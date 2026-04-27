// IVXLiveKitVisemeStream — Unreal Engine 5 client receiver for the
// Phase-4 LiveKit `viseme.v1` data-channel protocol.
//
// Mirrors the C# (Unity) `IVXLiveKitVisemeStream` and the TypeScript
// `IVXLiveKitVisemeReceiver`. Wire it on the Unreal client by:
//   1. Spawning one `UIVXLiveKitVisemeStream` per remote avatar.
//   2. Setting `BlendshapeNameMap` so ARKit-52 indices map to the
//      morph-target names on your USkeletalMeshComponent.
//   3. Forwarding every LiveKit `OnDataReceived(payload, topic)` you
//      get on the topic `viseme.v1` to `Dispatch(Payload, /*bIsJson*/true)`.
//
// The receiver tracks per-line state, drops out-of-order frames, and
// optionally drives a SkeletalMeshComponent's morph targets directly
// when `TargetMesh` is set. All event delegates are BlueprintAssignable
// so no C++ is required for a vanilla integration.
//
// Wire contract: schemas/avatar/viseme_v1.proto.

#pragma once

#include "CoreMinimal.h"
#include "UObject/Object.h"
#include "Components/SkeletalMeshComponent.h"
#include "Delegates/DelegateCombinations.h"
#include "IVXLiveKitVisemeStream.generated.h"

UENUM(BlueprintType)
enum class EIVXBlendshapeProfile : uint8
{
    None    = 0 UMETA(DisplayName = "None"),
    Arkit52 = 1 UMETA(DisplayName = "ARKit-52"),
    Ovr60   = 2 UMETA(DisplayName = "Oculus 60"),
    Vrm69   = 3 UMETA(DisplayName = "VRM-69"),
};

UENUM(BlueprintType)
enum class EIVXVisemeSource : uint8
{
    Unspecified = 0,
    Agent       = 1,
    UserFace    = 2,
    UserTts     = 3,
    Fallback    = 4,
};

USTRUCT(BlueprintType)
struct FIVXVisemeStreamHeader
{
    GENERATED_BODY()
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") FString UserId;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") FString TrackId;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") EIVXVisemeSource Source = EIVXVisemeSource::Agent;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int64 LineId = 0;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int32 ExpectedFrames = 0;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int32 SampleRateHz = 24000;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int32 FrameHz = 60;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") EIVXBlendshapeProfile Profile = EIVXBlendshapeProfile::Arkit52;
};

USTRUCT(BlueprintType)
struct FIVXVisemeFrame
{
    GENERATED_BODY()
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") FString UserId;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") TArray<uint8> Blendshapes;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") EIVXBlendshapeProfile Profile = EIVXBlendshapeProfile::Arkit52;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int64 AudioSeq = 0;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int64 AudioTsMs = 0;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int32 IntensityPct = 100;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int64 FrameSeq = 0;
};

USTRUCT(BlueprintType)
struct FIVXVisemeStreamFooter
{
    GENERATED_BODY()
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") FString UserId;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int64 LineId = 0;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int32 FramesSent = 0;
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Viseme") int64 FinalAudioSeq = 0;
};

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXVisemeHeaderEvent, const FIVXVisemeStreamHeader&, Header);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXVisemeFrameEvent,  const FIVXVisemeFrame&,        Frame);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FIVXVisemeFooterEvent, const FIVXVisemeStreamFooter&, Footer);

/**
 * UObject receiver for the LiveKit `viseme.v1` data channel. Drop one
 * per remote avatar and wire its `Dispatch()` method to your LiveKit
 * client's data-received callback. Optional auto-drive: assign a
 * `TargetMesh` and a `BlendshapeNameMap` and the receiver will write
 * morph weights directly on every frame.
 */
UCLASS(BlueprintType, Blueprintable, ClassGroup = "IVX|Avatar")
class INTELLIVERSEX_API UIVXLiveKitVisemeStream : public UObject
{
    GENERATED_BODY()

public:
    UIVXLiveKitVisemeStream();

    /** True between header receipt and footer receipt for the active line. */
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Avatar") bool bIsActive = false;

    /** Active line id (matches the value in the most-recent header). */
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Avatar") int64 CurrentLineId = 0;

    /** Last per-frame intensity (0..100) — useful for envelope shaping. */
    UPROPERTY(BlueprintReadOnly, Category = "IVX|Avatar") int32 LastIntensityPct = 0;

    /**
     * Optional auto-drive target. When set, every frame writes morph
     * weights directly onto the mesh's morph-target curves using
     * `BlendshapeNameMap` (ARKit index → morph-target name).
     */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IVX|Avatar")
    TObjectPtr<USkeletalMeshComponent> TargetMesh = nullptr;

    /**
     * Map from ARKit-52 index (0..51) to morph-target curve name on
     * `TargetMesh`. Indices missing from the map are skipped.
     */
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IVX|Avatar")
    TMap<int32, FName> BlendshapeNameMap;

    UPROPERTY(BlueprintAssignable, Category = "IVX|Avatar") FIVXVisemeHeaderEvent OnHeader;
    UPROPERTY(BlueprintAssignable, Category = "IVX|Avatar") FIVXVisemeFrameEvent  OnFrame;
    UPROPERTY(BlueprintAssignable, Category = "IVX|Avatar") FIVXVisemeFooterEvent OnFooter;

    /**
     * Decode a raw LiveKit data-channel payload (proto bytes or JSON)
     * and fire the matching event. Wire this to LiveKit's Native
     * `OnDataReceived(payload, participant, kind, topic)` callback,
     * filtering by `topic == "viseme.v1"`.
     *
     * @param Payload   Bytes from LiveKit data packet.
     * @param bIsJson   Phase-4 ships JSON-on-wire; binary proto is reserved.
     */
    UFUNCTION(BlueprintCallable, Category = "IVX|Avatar")
    void Dispatch(const TArray<uint8>& Payload, bool bIsJson = true);

    /** Force-reset the receiver (e.g. on track-change or session end). */
    UFUNCTION(BlueprintCallable, Category = "IVX|Avatar")
    void ResetStream(const FString& Reason);

    /** Map-helper for Blueprints: assign every ARKit-52 index in one go. */
    UFUNCTION(BlueprintCallable, Category = "IVX|Avatar")
    void SetArkit52NameMap(const TArray<FName>& OrderedNames);

    /** Diagnostics — used by QA dashboards / logs. */
    UFUNCTION(BlueprintCallable, Category = "IVX|Avatar")
    FString DiagnosticsJson() const;

private:
    int64 LastFrameSeq = 0;
    int32 DroppedFrames = 0;

    void HandleJsonEnvelope(const FString& Text);
    void ApplyToMesh(const FIVXVisemeFrame& Frame);
    void ZeroOutMorphTargets();
};
