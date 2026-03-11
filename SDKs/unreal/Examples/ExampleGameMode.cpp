#include "ExampleGameMode.h"
#include "Engine/GameInstance.h"

DEFINE_LOG_CATEGORY_STATIC(LogIVXExample, Log, All);

AExampleGameMode::AExampleGameMode()
{
    SDKConfig = nullptr;
}

void AExampleGameMode::InitGame(const FString& MapName, const FString& Options, FString& ErrorMessage)
{
    Super::InitGame(MapName, Options, ErrorMessage);

    UGameInstance* GI = GetGameInstance();
    if (!GI)
    {
        UE_LOG(LogIVXExample, Error, TEXT("No GameInstance available"));
        return;
    }

    UIVXManager* IVX = GI->GetSubsystem<UIVXManager>();
    if (!IVX)
    {
        UE_LOG(LogIVXExample, Error, TEXT("UIVXManager subsystem not found"));
        return;
    }

    IVX->OnInitialized.AddDynamic(this, &AExampleGameMode::HandleSDKInitialized);
    IVX->OnAuthenticated.AddDynamic(this, &AExampleGameMode::HandleAuthenticated);
    IVX->OnProfileLoaded.AddDynamic(this, &AExampleGameMode::HandleProfileLoaded);
    IVX->OnWalletLoaded.AddDynamic(this, &AExampleGameMode::HandleWalletLoaded);
    IVX->OnError.AddDynamic(this, &AExampleGameMode::HandleError);

    if (!SDKConfig)
    {
        SDKConfig = NewObject<UIVXConfig>(this);
        SDKConfig->NakamaHost = TEXT("127.0.0.1");
        SDKConfig->NakamaPort = 7350;
        SDKConfig->NakamaServerKey = TEXT("defaultkey");
        SDKConfig->bEnableDebugLogs = true;
    }

    UE_LOG(LogIVXExample, Log, TEXT("Initializing IntelliVerseX SDK..."));
    IVX->InitializeSDK(SDKConfig);
}

void AExampleGameMode::HandleSDKInitialized()
{
    UE_LOG(LogIVXExample, Log, TEXT("SDK initialized — authenticating with device..."));

    UIVXManager* IVX = GetGameInstance()->GetSubsystem<UIVXManager>();
    IVX->AuthenticateWithDevice(FString());
}

void AExampleGameMode::HandleAuthenticated()
{
    UIVXManager* IVX = GetGameInstance()->GetSubsystem<UIVXManager>();

    UE_LOG(LogIVXExample, Log, TEXT("Authenticated as %s (uid: %s)"), *IVX->GetUsername(), *IVX->GetUserId());
    UE_LOG(LogIVXExample, Log, TEXT("Fetching profile and wallet..."));

    IVX->FetchProfile();
    IVX->FetchWallet();
}

void AExampleGameMode::HandleProfileLoaded(const FString& ProfileJson)
{
    UE_LOG(LogIVXExample, Log, TEXT("Profile received: %s"), *ProfileJson);
}

void AExampleGameMode::HandleWalletLoaded(const FString& WalletJson)
{
    UE_LOG(LogIVXExample, Log, TEXT("Wallet received: %s"), *WalletJson);
}

void AExampleGameMode::HandleError(const FString& ErrorMessage)
{
    UE_LOG(LogIVXExample, Error, TEXT("IVX Error: %s"), *ErrorMessage);
}
