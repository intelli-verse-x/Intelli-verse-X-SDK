// IVXVoiceTokenClient — Blueprint-callable helper around the
// `mp_voice_token` Nakama RPC. Mirrors:
//
//   * Unity:  Assets/Intelli-verse-X-SDK/MultiplayerKernel/Voice/IVXVoiceTokenClient.cs
//   * JS:     SDKs/javascript/packages/multiplayer/src/voice/token-client.ts
//
// Returns an FIVXVoiceSessionToken that you feed straight into
// UIVXLiveKitVoiceProvider::Connect(...).
//
// Usage (C++):
//
//   FIVXMintVoiceTokenRequest Req;
//   Req.Client     = NakamaClient;   // UNakamaClient*
//   Req.Session    = NakamaSession;  // UNakamaSession*
//   Req.MatchId    = MatchId;
//   Req.bSpatial   = false;
//   UIVXVoiceTokenClient::MintAsync(Req,
//       FIVXMintVoiceTokenSuccess::CreateLambda([this](const FIVXVoiceSessionToken& T) { Voice->Connect(T); }),
//       FIVXMintVoiceTokenFailure::CreateLambda([](const FString& Code, const FString& Msg) { UE_LOG(LogTemp, Warning, TEXT("voice token failed: %s %s"), *Code, *Msg); }));
//
// Usage (Blueprint): Drop the "IVX Mint Voice Token" node, fill in the
// Nakama Client/Session and Match Id, then connect the OnSuccess pin
// straight into "IVX LiveKit Connect".

#pragma once

#include "CoreMinimal.h"
#include "UObject/Object.h"
#include "Delegates/DelegateCombinations.h"
#include "IVXVoiceLiveKit.h" // FIVXVoiceSessionToken / EIVXVoiceProvider
#include "IVXVoiceTokenClient.generated.h"

class UNakamaClient;
class UNakamaSession;

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXMintVoiceTokenRequest
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadWrite, Category = "IVX|Voice")
    UNakamaClient*  Client = nullptr;

    UPROPERTY(BlueprintReadWrite, Category = "IVX|Voice")
    UNakamaSession* Session = nullptr;

    UPROPERTY(BlueprintReadWrite, Category = "IVX|Voice")
    FString MatchId;

    UPROPERTY(BlueprintReadWrite, Category = "IVX|Voice")
    bool bCanPublish = true;

    UPROPERTY(BlueprintReadWrite, Category = "IVX|Voice")
    bool bCanSubscribe = true;

    /** Set true only for spatial Party / multi-human modes. Phase-3 1:1 stays false. */
    UPROPERTY(BlueprintReadWrite, Category = "IVX|Voice")
    bool bSpatial = false;

    /** Optional region override; kernel picks closest if empty. */
    UPROPERTY(BlueprintReadWrite, Category = "IVX|Voice")
    FString Region;
};

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXMintVoiceTokenSuccess, const FIVXVoiceSessionToken&, Token);
DECLARE_DYNAMIC_DELEGATE_TwoParams(FIVXMintVoiceTokenFailure, const FString&, Code, const FString&, Message);

/**
 * Static-only helper. Don't instantiate.
 */
UCLASS()
class INTELLIVERSEX_API UIVXVoiceTokenClient : public UObject
{
    GENERATED_BODY()

public:
    /**
     * Mint a LiveKit session token via the kernel's `mp_voice_token` RPC.
     * Fires OnSuccess with the typed token on success; OnFailure with
     * a normalised error code on failure. Codes mirror the JS adapter:
     *   - "bad_args"            (missing client/session/matchId)
     *   - "session_expired"     (Nakama session expired)
     *   - "rpc_failed"          (transport / 5xx / kernel rejected)
     *   - "voice_unconfigured"  (kernel returned empty payload)
     *   - "decode_failed"       (JSON parse error)
     *   - "invalid_token"       (kernel response missing token/url)
     */
    UFUNCTION(BlueprintCallable, Category = "IVX|Voice", meta = (DisplayName = "IVX Mint Voice Token"))
    static void MintAsync(
        const FIVXMintVoiceTokenRequest& Request,
        const FIVXMintVoiceTokenSuccess& OnSuccess,
        const FIVXMintVoiceTokenFailure& OnFailure);

private:
    static FString BuildPayload(const FIVXMintVoiceTokenRequest& Request);
    static bool ParseTokenJson(const FString& Json, FIVXVoiceSessionToken& Out, FString& OutErrorMessage);
};
