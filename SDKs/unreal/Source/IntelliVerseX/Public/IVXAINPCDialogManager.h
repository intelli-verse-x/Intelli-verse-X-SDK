// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXAINPCDialogManager.generated.h"

DECLARE_DYNAMIC_DELEGATE_OneParam(FIVXNPCSessionJsonDelegate, const FString&, SessionJson);

/**
 * NPC dialog manager (Unity IVXAINPCDialogManager). Stub surface.
 */
UCLASS(BlueprintType)
class INTELLIVERSEX_API UIVXAINPCDialogManager : public UObject
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|NPC", meta = (WorldContextObject = "WorldContextObject"))
    static UIVXAINPCDialogManager* GetInstance(UObject* WorldContextObject);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI")
    static void Shutdown();

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|NPC")
    void Initialize(UObject* Config);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|NPC")
    void SetAuthToken(const FString& Token);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|NPC")
    void StartDialog(const FString& NpcId, const FString& PlayerId, const FString& PlayerContext,
                     const FIVXNPCSessionJsonDelegate& OnStarted);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|NPC")
    void SendMessage(const FString& SessionId, const FString& Message, const FIVXNPCSessionJsonDelegate& OnResponse);

    UFUNCTION(BlueprintCallable, Category = "IntelliVerseX|AI|NPC")
    void EndDialog(const FString& SessionId);

private:
    static TWeakObjectPtr<UIVXAINPCDialogManager> Singleton;
};
