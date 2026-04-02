// Copyright (c) 2026 Intelli-verse-X — MIT License

#pragma once

#include "CoreMinimal.h"
#include "IVXDiscordSettings.generated.h"

/**
 * Discord Social Settings — notification preferences, privacy, DND mode.
 * Stub: API shape matches Unity IVXDiscordSettings.
 */
UCLASS()
class INTELLIVERSEX_API UIVXDiscordSettings : public UObject
{
    GENERATED_BODY()

public:
    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Discord|Settings")
    bool bNotificationsEnabled = true;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Discord|Settings")
    bool bFriendRequestsEnabled = true;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Discord|Settings")
    bool bDoNotDisturb = false;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Discord|Settings")
    bool bShowOnlineStatus = true;

    UPROPERTY(BlueprintReadWrite, Category = "IntelliVerseX|Discord|Settings")
    bool bAllowDirectMessages = true;

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Settings")
    void EnableDoNotDisturb() { bDoNotDisturb = true; }

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Settings")
    void DisableDoNotDisturb() { bDoNotDisturb = false; }

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Settings")
    void ResetToDefaults();
};
