#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "IVXManager.h"
#include "IVXConfig.h"
#include "ExampleGameMode.generated.h"

/**
 * Example game mode demonstrating IntelliVerseX SDK initialization,
 * device authentication, and profile fetching in Unreal Engine.
 *
 * Usage:
 *   1. Create a UIVXConfig data asset in the editor with your server settings.
 *   2. Assign it to the SDKConfig property on this game mode (in BP or editor).
 *   3. Set this as your map's game mode.
 */
UCLASS()
class AExampleGameMode : public AGameModeBase
{
    GENERATED_BODY()

public:
    AExampleGameMode();

    virtual void InitGame(const FString& MapName, const FString& Options, FString& ErrorMessage) override;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX")
    UIVXConfig* SDKConfig;

private:
    UFUNCTION()
    void HandleSDKInitialized();

    UFUNCTION()
    void HandleAuthenticated();

    UFUNCTION()
    void HandleProfileLoaded(const FString& ProfileJson);

    UFUNCTION()
    void HandleWalletLoaded(const FString& WalletJson);

    UFUNCTION()
    void HandleError(const FString& ErrorMessage);
};
