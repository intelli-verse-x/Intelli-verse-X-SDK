// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXDiscordDebug.h"

TWeakObjectPtr<UIVXDiscordDebug> UIVXDiscordDebug::Singleton = nullptr;

UIVXDiscordDebug* UIVXDiscordDebug::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXDiscordDebug>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXDiscordDebug::SetLogLevel(EIVXDiscordLogLevel Level)
{
    LogLevel = Level;
}

int32 UIVXDiscordDebug::AddLogCallback(const FIVXDiscordLogEntryDelegate& Callback)
{
    const int32 Id = ++NextCallbackId;
    Callbacks.Add(Id, Callback);
    return Id;
}

void UIVXDiscordDebug::RemoveLogCallback(int32 CallbackHandle)
{
    Callbacks.Remove(CallbackHandle);
}

TArray<FIVXDiscordLogEntry> UIVXDiscordDebug::GetLogHistory(int32 Limit) const
{
    if (Limit <= 0 || LogHistory.Num() == 0)
    {
        return TArray<FIVXDiscordLogEntry>();
    }
    const int32 Start = FMath::Max(0, LogHistory.Num() - Limit);
    TArray<FIVXDiscordLogEntry> Out;
    Out.Reserve(LogHistory.Num() - Start);
    for (int32 i = Start; i < LogHistory.Num(); ++i)
    {
        Out.Add(LogHistory[i]);
    }
    return Out;
}

void UIVXDiscordDebug::ClearLogHistory()
{
    LogHistory.Reset();
}

void UIVXDiscordDebug::EmitLog(EIVXDiscordLogLevel Level, const FString& Message, const FString& Source)
{
    if (static_cast<uint8>(Level) > static_cast<uint8>(LogLevel))
    {
        return;
    }

    FIVXDiscordLogEntry Entry;
    Entry.Level = Level;
    Entry.Message = Message;
    Entry.Timestamp = FDateTime::UtcNow().ToUnixTimestamp() * 1000;
    Entry.Source = Source.IsEmpty() ? TEXT("discord") : Source;

    LogHistory.Add(Entry);
    if (LogHistory.Num() > MaxHistory)
    {
        LogHistory.RemoveAt(0, LogHistory.Num() - MaxHistory, EAllowShrinking::No);
    }

    for (auto& Pair : Callbacks)
    {
        Pair.Value.ExecuteIfBound(Entry);
    }
}
