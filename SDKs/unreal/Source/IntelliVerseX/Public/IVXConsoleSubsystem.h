#pragma once
#include "Subsystems/GameInstanceSubsystem.h"
#include "OnlineSubsystem.h"
#include "IVXConsoleSubsystem.generated.h"

UCLASS()
class INTELLIVERSEX_API UIVXConsoleSubsystem : public UGameInstanceSubsystem
{
    GENERATED_BODY()
public:
    virtual void Initialize(FSubsystemCollectionBase& Collection) override;
    virtual void Deinitialize() override;

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Console")
    FString GetPlatformUserId() const;

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Console")
    bool IsConsoleAvailable() const;

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Console")
    void SignInWithPlatform();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Console")
    bool UnlockAchievement(const FString& AchievementId);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Console")
    void SetPresence(const FString& Status);

    DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnConsoleSignInComplete, bool, bSuccess);
    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Console")
    FOnConsoleSignInComplete OnConsoleSignInComplete;

private:
    IOnlineSubsystem* OnlineSub = nullptr;
};
