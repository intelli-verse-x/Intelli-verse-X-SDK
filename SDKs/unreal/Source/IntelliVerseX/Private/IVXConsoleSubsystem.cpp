#include "IVXConsoleSubsystem.h"
#include "OnlineSubsystemUtils.h"
#include "Interfaces/OnlineIdentityInterface.h"
#include "Interfaces/OnlineAchievementsInterface.h"
#include "Interfaces/OnlinePresenceInterface.h"

DEFINE_LOG_CATEGORY_STATIC(LogIVXConsole, Log, All);

void UIVXConsoleSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
    Super::Initialize(Collection);
    OnlineSub = IOnlineSubsystem::Get();
    if (OnlineSub)
    {
        UE_LOG(LogIVXConsole, Log, TEXT("[IVX] Console subsystem initialised — platform: %s"),
               *FString(OnlineSub->GetSubsystemName().ToString()));
    }
    else
    {
        UE_LOG(LogIVXConsole, Warning, TEXT("[IVX] No online subsystem available"));
    }
}

void UIVXConsoleSubsystem::Deinitialize()
{
    OnlineSub = nullptr;
    Super::Deinitialize();
}

FString UIVXConsoleSubsystem::GetPlatformUserId() const
{
    if (!OnlineSub) return FString();
    IOnlineIdentityPtr Identity = OnlineSub->GetIdentityInterface();
    if (!Identity.IsValid()) return FString();
    FUniqueNetIdPtr UserId = Identity->GetUniquePlayerId(0);
    return UserId.IsValid() ? UserId->ToString() : FString();
}

bool UIVXConsoleSubsystem::IsConsoleAvailable() const
{
    return OnlineSub != nullptr;
}

void UIVXConsoleSubsystem::SignInWithPlatform()
{
    if (!OnlineSub)
    {
        OnConsoleSignInComplete.Broadcast(false);
        return;
    }

    IOnlineIdentityPtr Identity = OnlineSub->GetIdentityInterface();
    if (!Identity.IsValid())
    {
        OnConsoleSignInComplete.Broadcast(false);
        return;
    }

    Identity->OnLoginCompleteDelegates->AddWeakLambda(this,
        [this](int32 LocalUserNum, bool bWasSuccessful, const FUniqueNetId& /*UserId*/, const FString& Error)
        {
            if (!Error.IsEmpty())
            {
                UE_LOG(LogIVXConsole, Warning, TEXT("[IVX] Platform sign-in error: %s"), *Error);
            }
            OnConsoleSignInComplete.Broadcast(bWasSuccessful);
        });

    Identity->AutoLogin(0);
}

bool UIVXConsoleSubsystem::UnlockAchievement(const FString& AchievementId)
{
    if (!OnlineSub) return false;
    IOnlineAchievementsPtr Achievements = OnlineSub->GetAchievementsInterface();
    if (!Achievements.IsValid()) return false;

    FUniqueNetIdPtr UserId = OnlineSub->GetIdentityInterface()->GetUniquePlayerId(0);
    if (!UserId.IsValid()) return false;

    FOnlineAchievementsWriteRef WriteObj = MakeShareable(new FOnlineAchievementsWrite());
    WriteObj->SetFloatStat(*AchievementId, 100.0f);

    Achievements->WriteAchievements(
        *UserId, WriteObj,
        FOnAchievementsWrittenDelegate::CreateWeakLambda(this,
            [AchievementId](const FUniqueNetId& /*PlayerId*/, bool bWasSuccessful)
            {
                UE_LOG(LogIVXConsole, Log, TEXT("[IVX] Achievement '%s' write %s"),
                       *AchievementId, bWasSuccessful ? TEXT("succeeded") : TEXT("failed"));
            }));

    return true;
}

void UIVXConsoleSubsystem::SetPresence(const FString& Status)
{
    if (!OnlineSub) return;
    IOnlinePresencePtr Presence = OnlineSub->GetPresenceInterface();
    if (!Presence.IsValid()) return;

    FUniqueNetIdPtr UserId = OnlineSub->GetIdentityInterface()->GetUniquePlayerId(0);
    if (!UserId.IsValid()) return;

    FOnlineUserPresenceStatus PresenceStatus;
    PresenceStatus.StatusStr = Status;
    PresenceStatus.State = EOnlinePresenceState::Online;

    Presence->SetPresence(*UserId, PresenceStatus,
        IOnlinePresence::FOnPresenceTaskCompleteDelegate::CreateWeakLambda(this,
            [](const FUniqueNetId& /*UserId*/, const bool bWasSuccessful)
            {
                UE_LOG(LogIVXConsole, Log, TEXT("[IVX] Presence update %s"),
                       bWasSuccessful ? TEXT("succeeded") : TEXT("failed"));
            }));
}
