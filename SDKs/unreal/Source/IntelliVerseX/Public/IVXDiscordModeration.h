// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXDiscordModeration.generated.h"

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXModerationBoolDelegate, bool, bOk);
DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXModerationStringDelegate, const FString&, Value);

/**
 * Discord moderation & reporting (Unity IVXDiscordModeration). Stub surface.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXDiscordModeration : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Moderation", meta = (WorldContextObject = "WorldContextObject"))
    static UIVXDiscordModeration* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Moderation")
    void EnableAutoModeration(bool bEnable);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Moderation")
    void ProcessModerationMetadata(int64 MessageId, const TMap<FString, FString>& Metadata);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Moderation")
    void StartVoiceModerationCapture(int64 LobbyId);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Moderation")
    void StopVoiceModerationCapture();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Moderation")
    void ReportUser(int64 UserId, const FString& Reason, const FIVXModerationBoolDelegate& OnComplete);

private:
    static TWeakObjectPtr<UIVXDiscordModeration> Singleton;
};
