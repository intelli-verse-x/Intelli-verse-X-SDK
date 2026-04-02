// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "IVXSatori.h"
#include "IVXManager.h"

TWeakObjectPtr<UIVXSatori> UIVXSatori::Singleton = nullptr;

UIVXSatori* UIVXSatori::GetInstance(UObject* WorldContextObject)
{
    if (!Singleton.IsValid())
    {
        Singleton = NewObject<UIVXSatori>(GetTransientPackage());
        Singleton->AddToRoot();
    }
    return Singleton.Get();
}

void UIVXSatori::Initialize(const FIVXSatoriConfig& InConfig)
{
    if (InConfig.SatoriUrl.IsEmpty() || InConfig.ApiKey.IsEmpty())
    {
        UE_LOG(LogIVX, Error, TEXT("[IVXSatori] Initialize requires SatoriUrl and ApiKey."));
        bInitialized = false;
        return;
    }
    Config = InConfig;
    bInitialized = true;
}

void UIVXSatori::Authenticate(const FString& InIdentityId, const TMap<FString, FString>& DefaultProperties,
                              const TMap<FString, FString>& CustomProperties)
{
    if (!bInitialized)
    {
        UE_LOG(LogIVX, Error, TEXT("[IVXSatori] Authenticate called before Initialize."));
        return;
    }
    UE_LOG(LogIVX, Warning, TEXT("[IVXSatori] Authenticate: stub – not yet implemented"));
    IdentityId = InIdentityId;
}

void UIVXSatori::UpdateIdentity(const TMap<FString, FString>& DefaultProperties,
                                const TMap<FString, FString>& CustomProperties)
{
    if (!bInitialized)
    {
        UE_LOG(LogIVX, Error, TEXT("[IVXSatori] UpdateIdentity called before Initialize."));
        return;
    }
    UE_LOG(LogIVX, Warning, TEXT("[IVXSatori] UpdateIdentity: stub – not yet implemented"));
}

void UIVXSatori::CaptureEvents(const TArray<FIVXSatoriEvent>& Events)
{
    if (!bInitialized)
    {
        UE_LOG(LogIVX, Error, TEXT("[IVXSatori] CaptureEvents called before Initialize."));
        return;
    }
    UE_LOG(LogIVX, Warning, TEXT("[IVXSatori] CaptureEvents: stub – not yet implemented"));
}

TArray<FIVXSatoriFlag> UIVXSatori::GetAllFlags()
{
    if (!bInitialized)
    {
        UE_LOG(LogIVX, Error, TEXT("[IVXSatori] GetAllFlags called before Initialize."));
        return TArray<FIVXSatoriFlag>();
    }
    UE_LOG(LogIVX, Warning, TEXT("[IVXSatori] GetAllFlags: stub – not yet implemented"));
    return TArray<FIVXSatoriFlag>();
}

bool UIVXSatori::GetFlag(const FString& Name, FIVXSatoriFlag& OutFlag)
{
    if (!bInitialized)
    {
        UE_LOG(LogIVX, Error, TEXT("[IVXSatori] GetFlag called before Initialize."));
        return false;
    }
    UE_LOG(LogIVX, Warning, TEXT("[IVXSatori] GetFlag: stub – not yet implemented"));
    return false;
}

FString UIVXSatori::GetExperimentVariant(const FString& ExperimentName)
{
    if (!bInitialized)
    {
        UE_LOG(LogIVX, Error, TEXT("[IVXSatori] GetExperimentVariant called before Initialize."));
        return FString();
    }
    UE_LOG(LogIVX, Warning, TEXT("[IVXSatori] GetExperimentVariant: stub – not yet implemented"));
    return FString();
}

TArray<FIVXSatoriExperiment> UIVXSatori::GetAllExperiments()
{
    if (!bInitialized)
    {
        UE_LOG(LogIVX, Error, TEXT("[IVXSatori] GetAllExperiments called before Initialize."));
        return TArray<FIVXSatoriExperiment>();
    }
    UE_LOG(LogIVX, Warning, TEXT("[IVXSatori] GetAllExperiments: stub – not yet implemented"));
    return TArray<FIVXSatoriExperiment>();
}

TArray<FIVXSatoriLiveEvent> UIVXSatori::GetLiveEvents()
{
    if (!bInitialized)
    {
        UE_LOG(LogIVX, Error, TEXT("[IVXSatori] GetLiveEvents called before Initialize."));
        return TArray<FIVXSatoriLiveEvent>();
    }
    UE_LOG(LogIVX, Warning, TEXT("[IVXSatori] GetLiveEvents: stub – not yet implemented"));
    return TArray<FIVXSatoriLiveEvent>();
}

void UIVXSatori::Logout()
{
    if (!bInitialized)
    {
        UE_LOG(LogIVX, Error, TEXT("[IVXSatori] Logout called before Initialize."));
        return;
    }
    UE_LOG(LogIVX, Warning, TEXT("[IVXSatori] Logout: stub – not yet implemented"));
    IdentityId.Reset();
}
