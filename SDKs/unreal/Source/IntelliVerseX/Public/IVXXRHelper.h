#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXXRHelper.generated.h"

UENUM(BlueprintType)
enum class EIVXXRPlatform : uint8
{
    None          UMETA(DisplayName = "None"),
    MetaQuest     UMETA(DisplayName = "Meta Quest"),
    SteamVR       UMETA(DisplayName = "SteamVR"),
    AppleVisionPro UMETA(DisplayName = "Apple Vision Pro"),
    PSVR2         UMETA(DisplayName = "PSVR2"),
    GenericOpenXR UMETA(DisplayName = "Generic OpenXR"),
    ARKit         UMETA(DisplayName = "ARKit"),
    ARCore        UMETA(DisplayName = "ARCore")
};

UCLASS(BlueprintType, Blueprintable)
class INTELLIVERSEX_API UIVXXRHelper : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|XR")
    static EIVXXRPlatform DetectXRPlatform();

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|XR")
    static bool IsXRActive();

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|XR")
    static bool IsHandTrackingAvailable();

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|XR")
    static bool IsPassthroughAvailable();

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|XR")
    static bool IsEyeTrackingAvailable();

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|XR")
    static FString GetXRSystemName();
};
