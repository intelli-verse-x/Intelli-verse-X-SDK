#include "IVXXRHelper.h"
#include "IVXManager.h"
#include "IXRTrackingSystem.h"
#include "Engine/Engine.h"

EIVXXRPlatform UIVXXRHelper::DetectXRPlatform()
{
    if (!GEngine || !GEngine->XRSystem.IsValid())
    {
        return EIVXXRPlatform::None;
    }

    FString SystemName = GEngine->XRSystem->GetSystemName().ToString();
    UE_LOG(LogIVX, Log, TEXT("[IVXXRHelper] XR System: %s"), *SystemName);

    if (SystemName.Contains(TEXT("OculusHMD")) || SystemName.Contains(TEXT("Meta")))
        return EIVXXRPlatform::MetaQuest;
    if (SystemName.Contains(TEXT("SteamVR")))
        return EIVXXRPlatform::SteamVR;
    if (SystemName.Contains(TEXT("AppleVision")))
        return EIVXXRPlatform::AppleVisionPro;
    if (SystemName.Contains(TEXT("OpenXR")))
        return EIVXXRPlatform::GenericOpenXR;

    return EIVXXRPlatform::None;
}

bool UIVXXRHelper::IsXRActive()
{
    return GEngine && GEngine->XRSystem.IsValid() && GEngine->XRSystem->IsHeadTrackingAllowed();
}

bool UIVXXRHelper::IsHandTrackingAvailable()
{
    EIVXXRPlatform platform = DetectXRPlatform();
    return platform == EIVXXRPlatform::MetaQuest || platform == EIVXXRPlatform::AppleVisionPro;
}

bool UIVXXRHelper::IsPassthroughAvailable()
{
    EIVXXRPlatform platform = DetectXRPlatform();
    return platform == EIVXXRPlatform::MetaQuest || platform == EIVXXRPlatform::AppleVisionPro;
}
