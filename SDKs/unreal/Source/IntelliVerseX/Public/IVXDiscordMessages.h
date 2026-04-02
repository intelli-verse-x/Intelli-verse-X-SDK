// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXDiscordSocial.h"
#include "IVXDiscordMessages.generated.h"

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXDMMessageIdDelegate, int64, MessageId);
DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXDMStringDelegate, const FString&, Value);

/**
 * Discord direct messages API (Unity IVXDiscordMessages). Stub integration surface.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXDiscordMessages : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|DM", meta = (WorldContextObject = "WorldContextObject"))
    static UIVXDiscordMessages* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|Discord|DM")
    bool IsShowingChat() const { return bShowingChat; }

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|DM")
    void SendDM(int64 RecipientId, const FString& Message, const FIVXDMMessageIdDelegate& OnSuccess,
                const FIVXDMStringDelegate& OnError);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|DM")
    void EditDM(int64 RecipientId, int64 MessageId, const FString& NewContent, const FIVXDiscordSuccessDelegate& OnSuccess,
                const FIVXDMStringDelegate& OnError);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|DM")
    void GetDMHistory(int64 RecipientId, int32 Limit, const FIVXDMStringDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|DM")
    void GetDMSummaries(const FIVXDMStringDelegate& OnComplete);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|DM")
    void SetShowingChat(bool bShowing);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|DM")
    void OpenMessageInDiscord(int64 MessageId);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|DM")
    void OpenDMSettingsInDiscord();

private:
    static TWeakObjectPtr<UIVXDiscordMessages> Singleton;

    bool bShowingChat = false;
};
