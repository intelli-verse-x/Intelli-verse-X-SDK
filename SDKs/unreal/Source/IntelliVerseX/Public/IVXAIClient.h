// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "Http.h"
#include "IVXAIClient.generated.h"

// --- Struct types ---

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXHostProfile
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|AI")
    FString PersonaId;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|AI")
    FString DisplayName;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|AI")
    FString VoiceId;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|AI")
    FString Language;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IntelliVerseX|AI")
    TMap<FString, FString> ExtraParams;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXAISessionResponse
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString SessionId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString Status;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString WebSocketUrl;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXAIMessage
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString Role;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString Content;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString Timestamp;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXAIPersona
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString PersonaId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString Name;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString Description;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString VoiceId;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString AvatarUrl;
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXAIEntitlement
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    bool bEntitled = false;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString Tier;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    int32 RemainingCredits = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|AI")
    FString ExpiresAt;
};

// --- Delegates ---

DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXAISessionDelegate, bool, bSuccess, const FIVXAISessionResponse&, Response);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXEntitlementDelegate, bool, bSuccess, const FIVXAIEntitlement&, Entitlement);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXPersonasDelegate, bool, bSuccess, const TArray<FIVXAIPersona>&, Personas);

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnIVXAIMessageReceived, const FString&, SessionId, const FIVXAIMessage&, Message);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnIVXAIError, const FString&, SessionId, const FString&, ErrorMessage);

/**
 * REST-based AI client for IntelliVerseX voice/text sessions and host AI.
 * Blueprint-exposed singleton accessed via GetIVXAIClient().
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXAIClient : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI", meta = (DisplayName = "Get IVX AI Client", WorldContext = "WorldContextObject"))
    static UIVXAIClient* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI")
    void Initialize(const FString& ApiBaseUrl, const FString& ApiKey);

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|AI")
    bool IsInitialized() const { return bIsInitialized; }

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Voice")
    void StartVoiceSession(const FString& PersonaId, const FString& UserId, const FIVXAISessionDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Voice")
    void EndVoiceSession(const FString& SessionId);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Voice")
    void SendText(const FString& SessionId, const FString& Text);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Host")
    void StartHostSession(const FString& MatchId, const FIVXHostProfile& Profile, const FIVXAISessionDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Host")
    void SendHostEvent(const FString& SessionId, const FString& EventType, const FString& Data);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Entitlement")
    void CheckEntitlement(const FString& UserId, const FIVXEntitlementDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|Personas")
    void GetPersonas(const FIVXPersonasDelegate& OnComplete);

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|AI|Events")
    FOnIVXAIMessageReceived OnMessageReceived;

    UPROPERTY(BlueprintAssignable, Category = "IntelliVerseX|AI|Events")
    FOnIVXAIError OnAIError;

private:
    static TWeakObjectPtr<UIVXAIClient> Singleton;

    bool bIsInitialized = false;
    FString BaseUrl;
    FString ApiKey;

    TSharedRef<IHttpRequest> CreateRequest(const FString& Endpoint, const FString& Verb) const;
    TSharedPtr<FJsonObject> ParseResponse(FHttpResponsePtr Response, bool bSucceeded) const;
    void LogDebug(const FString& Message) const;
    void LogError(const FString& Message) const;
};
