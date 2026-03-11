#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "IVXConfig.h"
#include "NakamaClient.h"
#include "NakamaSession.h"
#include "NakamaRealtimeClient.h"
#include "IVXManager.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnIVXInitialized);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnIVXAuthenticated);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXError, const FString&, ErrorMessage);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXProfileLoaded, const FString&, ProfileJson);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXWalletLoaded, const FString&, WalletJson);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXLeaderboardFetched, const FString&, LeaderboardJson);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnIVXStorageRead, const FString&, ValueJson);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnIVXRpcResult, const FString&, RpcId, const FString&, ResponseJson);

UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXManager : public UGameInstanceSubsystem
{
    GENERATED_BODY()

public:
    virtual void Initialize(FSubsystemCollectionBase& Collection) override;
    virtual void Deinitialize() override;

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX")
    void InitializeSDK(UIVXConfig* Config);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Auth")
    void AuthenticateWithDevice(const FString& DeviceId);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Auth")
    void AuthenticateWithEmail(const FString& Email, const FString& Password, bool bCreate = false);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Auth")
    void AuthenticateWithGoogle(const FString& Token);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Auth")
    void AuthenticateWithApple(const FString& Token);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Auth")
    void AuthenticateWithCustomId(const FString& CustomId);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Auth")
    void RestoreSession();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Auth")
    void ClearSession();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Socket")
    void DisconnectSocket();

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX")
    bool IsInitialized() const { return bIsInitialized; }

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX")
    bool HasValidSession() const;

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|Auth")
    FString GetUserId() const;

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|Auth")
    FString GetUsername() const;

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Profile")
    void FetchProfile();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Profile")
    void UpdateProfile(const FString& DisplayName, const FString& AvatarUrl, const FString& LangTag);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Wallet")
    void FetchWallet();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Wallet")
    void GrantCurrency(const FString& CurrencyId, int64 Amount);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Leaderboard")
    void SubmitLeaderboardScore(const FString& LeaderboardId, int64 Score);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Leaderboard")
    void FetchLeaderboard(const FString& LeaderboardId, int32 Limit = 20);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Storage")
    void WriteStorageObject(const FString& Collection, const FString& Key, const FString& ValueJson);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Storage")
    void ReadStorageObject(const FString& Collection, const FString& Key);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|RPC")
    void CallRpc(const FString& RpcId, const FString& PayloadJson);

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Events")
    FOnIVXInitialized OnInitialized;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Events")
    FOnIVXAuthenticated OnAuthenticated;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Events")
    FOnIVXError OnError;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Events")
    FOnIVXProfileLoaded OnProfileLoaded;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Events")
    FOnIVXWalletLoaded OnWalletLoaded;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Events")
    FOnIVXLeaderboardFetched OnLeaderboardFetched;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Events")
    FOnIVXStorageRead OnStorageRead;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|Events")
    FOnIVXRpcResult OnRpcResult;

private:
    UPROPERTY()
    UIVXConfig* SDKConfig = nullptr;

    UPROPERTY()
    UNakamaClient* NakamaClient = nullptr;

    UPROPERTY()
    UNakamaSession* CurrentSession = nullptr;

    UPROPERTY()
    UNakamaRealtimeClient* RtClient = nullptr;

    bool bIsInitialized = false;

    void SaveSessionToLocal(UNakamaSession* Session);
    UNakamaSession* LoadSessionFromLocal();
    void OnAuthSuccess(UNakamaSession* Session);
    void OnAuthError(const FNakamaError& Error);

    FString GetPersistentDeviceId() const;
    void SyncPlayerMetadata();

    void LogDebug(const FString& Message) const;
    void LogError(const FString& Message) const;
};
