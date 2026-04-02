// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXDiscordDebug.generated.h"

/** Minimum severity for Discord SDK log output. */
UENUM(BlueprintType)
enum class EIVXDiscordLogLevel : uint8
{
    None UMETA(DisplayName = "None"),
    Error UMETA(DisplayName = "Error"),
    Warn UMETA(DisplayName = "Warn"),
    Info UMETA(DisplayName = "Info"),
    Debug UMETA(DisplayName = "Debug")
};

USTRUCT(BlueprintType)
struct INTELLIVERSEX_API FIVXDiscordLogEntry
{
    GENERATED_BODY()

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Debug")
    EIVXDiscordLogLevel Level = EIVXDiscordLogLevel::None;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Debug")
    FString Message;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Debug")
    int64 Timestamp = 0;

    UPROPERTY(BlueprintReadOnly, Category = "IntelliVerseX|Discord|Debug")
    FString Source;
};

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXDiscordLogEntryDelegate, const FIVXDiscordLogEntry&, Entry);

/**
 * Discord Social SDK — debug logging and history. Stub surface; native bridge calls EmitLog.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXDiscordDebug : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Debug",
              meta = (WorldContextObject = "WorldContextObject"))
    static UIVXDiscordDebug* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Debug")
    void SetLogLevel(EIVXDiscordLogLevel Level);

    UFUNCTION(BlueprintPure, Category = "IntelliVerseX|Discord|Debug")
    EIVXDiscordLogLevel GetLogLevel() const { return LogLevel; }

    /** Returns a handle for RemoveLogCallback. */
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Debug")
    int32 AddLogCallback(const FIVXDiscordLogEntryDelegate& Callback);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Debug")
    void RemoveLogCallback(int32 CallbackHandle);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Debug")
    TArray<FIVXDiscordLogEntry> GetLogHistory(int32 Limit = 100) const;

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Debug")
    void ClearLogHistory();

    /** Invoked by the native Discord bridge when a log line is produced. */
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|Discord|Debug")
    void EmitLog(EIVXDiscordLogLevel Level, const FString& Message, const FString& Source = TEXT("discord"));

private:
    static TWeakObjectPtr<UIVXDiscordDebug> Singleton;

    static constexpr int32 MaxHistory = 500;

    UPROPERTY()
    TArray<FIVXDiscordLogEntry> LogHistory;

    EIVXDiscordLogLevel LogLevel = EIVXDiscordLogLevel::Warn;

    int32 NextCallbackId = 0;

    TMap<int32, FIVXDiscordLogEntryDelegate> Callbacks;
};
