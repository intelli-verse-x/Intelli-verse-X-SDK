#include "IVXManager.h"
#include "Kismet/GameplayStatics.h"
#include "Misc/Guid.h"
#include "JsonObjectConverter.h"

DEFINE_LOG_CATEGORY_STATIC(LogIVX, Log, All);

static const FString IVX_SESSION_TOKEN_KEY = TEXT("IVX_SessionToken");
static const FString IVX_REFRESH_TOKEN_KEY = TEXT("IVX_RefreshToken");
static const FString IVX_DEVICE_ID_KEY = TEXT("IVX_DeviceId");

void UIVXManager::Initialize(FSubsystemCollectionBase& Collection)
{
    Super::Initialize(Collection);
    UE_LOG(LogIVX, Log, TEXT("IntelliVerseX SDK subsystem created"));
}

void UIVXManager::Deinitialize()
{
    DisconnectSocket();
    ClearSession();
    Super::Deinitialize();
}

void UIVXManager::InitializeSDK(UIVXConfig* Config)
{
    if (!Config)
    {
        LogError(TEXT("InitializeSDK called with null config"));
        OnError.Broadcast(TEXT("Config is null"));
        return;
    }

    SDKConfig = Config;

    NakamaClient = UNakamaClient::CreateDefaultClient(
        Config->NakamaServerKey,
        Config->NakamaHost,
        Config->NakamaPort,
        Config->bUseSSL
    );

    if (!NakamaClient)
    {
        LogError(TEXT("Failed to create Nakama client"));
        OnError.Broadcast(TEXT("Failed to create Nakama client"));
        return;
    }

    bIsInitialized = true;
    LogDebug(FString::Printf(TEXT("IntelliVerseX SDK initialized — Host: %s:%d"), *Config->NakamaHost, Config->NakamaPort));
    OnInitialized.Broadcast();
}

void UIVXManager::AuthenticateWithDevice(const FString& DeviceId)
{
    if (!bIsInitialized || !NakamaClient)
    {
        OnError.Broadcast(TEXT("SDK not initialized"));
        return;
    }

    FString ResolvedId = DeviceId.IsEmpty() ? GetPersistentDeviceId() : DeviceId;

    auto SuccessCallback = FOnAuthUpdate::CreateLambda([this](UNakamaSession* Session)
    {
        OnAuthSuccess(Session);
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnAuthError(Error);
    });

    NakamaClient->AuthenticateDevice(ResolvedId, true, FString(), {}, SuccessCallback, ErrorCallback);
}

void UIVXManager::AuthenticateWithEmail(const FString& Email, const FString& Password, bool bCreate)
{
    if (!bIsInitialized || !NakamaClient)
    {
        OnError.Broadcast(TEXT("SDK not initialized"));
        return;
    }

    auto SuccessCallback = FOnAuthUpdate::CreateLambda([this](UNakamaSession* Session)
    {
        OnAuthSuccess(Session);
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnAuthError(Error);
    });

    NakamaClient->AuthenticateEmail(Email, Password, FString(), bCreate, {}, SuccessCallback, ErrorCallback);
}

void UIVXManager::AuthenticateWithGoogle(const FString& Token)
{
    if (!bIsInitialized || !NakamaClient)
    {
        OnError.Broadcast(TEXT("SDK not initialized"));
        return;
    }

    auto SuccessCallback = FOnAuthUpdate::CreateLambda([this](UNakamaSession* Session)
    {
        OnAuthSuccess(Session);
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnAuthError(Error);
    });

    NakamaClient->AuthenticateGoogle(Token, FString(), true, {}, SuccessCallback, ErrorCallback);
}

void UIVXManager::AuthenticateWithApple(const FString& Token)
{
    if (!bIsInitialized || !NakamaClient)
    {
        OnError.Broadcast(TEXT("SDK not initialized"));
        return;
    }

    auto SuccessCallback = FOnAuthUpdate::CreateLambda([this](UNakamaSession* Session)
    {
        OnAuthSuccess(Session);
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnAuthError(Error);
    });

    NakamaClient->AuthenticateApple(Token, FString(), true, {}, SuccessCallback, ErrorCallback);
}

void UIVXManager::AuthenticateWithCustomId(const FString& CustomId)
{
    if (!bIsInitialized || !NakamaClient)
    {
        OnError.Broadcast(TEXT("SDK not initialized"));
        return;
    }

    auto SuccessCallback = FOnAuthUpdate::CreateLambda([this](UNakamaSession* Session)
    {
        OnAuthSuccess(Session);
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnAuthError(Error);
    });

    NakamaClient->AuthenticateCustom(CustomId, FString(), true, {}, SuccessCallback, ErrorCallback);
}

void UIVXManager::RestoreSession()
{
    if (!bIsInitialized || !NakamaClient)
    {
        OnError.Broadcast(TEXT("SDK not initialized — call InitializeSDK first"));
        return;
    }

    UNakamaSession* SavedSession = LoadSessionFromLocal();
    if (SavedSession && !SavedSession->IsExpired())
    {
        CurrentSession = SavedSession;
        LogDebug(FString::Printf(TEXT("Session restored for user: %s"), *CurrentSession->GetUserId()));
        OnAuthenticated.Broadcast();
        SyncPlayerMetadata();
    }
    else
    {
        LogDebug(TEXT("No valid session to restore, authenticating with device"));
        AuthenticateWithDevice(FString());
    }
}

void UIVXManager::ClearSession()
{
    CurrentSession = nullptr;
    GConfig->SetString(TEXT("IntelliVerseX"), *IVX_SESSION_TOKEN_KEY, TEXT(""), GGameIni);
    GConfig->SetString(TEXT("IntelliVerseX"), *IVX_REFRESH_TOKEN_KEY, TEXT(""), GGameIni);
    LogDebug(TEXT("Session cleared"));
}

void UIVXManager::DisconnectSocket()
{
    if (RtClient)
    {
        RtClient->Disconnect();
        RtClient = nullptr;
        LogDebug(TEXT("Realtime socket disconnected"));
    }
}

bool UIVXManager::HasValidSession() const
{
    return CurrentSession != nullptr && !CurrentSession->IsExpired();
}

FString UIVXManager::GetUserId() const
{
    return CurrentSession ? CurrentSession->GetUserId() : FString();
}

FString UIVXManager::GetUsername() const
{
    return CurrentSession ? CurrentSession->GetUsername() : FString();
}

void UIVXManager::FetchProfile()
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    auto SuccessCallback = FOnUserAccount::CreateLambda([this](const FNakamaAccount& Account)
    {
        TSharedPtr<FJsonObject> Json = MakeShareable(new FJsonObject());
        Json->SetStringField(TEXT("user_id"), Account.User.Id);
        Json->SetStringField(TEXT("username"), Account.User.Username);
        Json->SetStringField(TEXT("display_name"), Account.User.DisplayName);
        Json->SetStringField(TEXT("avatar_url"), Account.User.AvatarUrl);
        Json->SetStringField(TEXT("lang_tag"), Account.User.LangTag);
        Json->SetStringField(TEXT("metadata"), Account.User.Metadata);
        Json->SetStringField(TEXT("wallet"), Account.Wallet);
        Json->SetStringField(TEXT("email"), Account.Email);
        Json->SetStringField(TEXT("create_time"), Account.User.CreatedAt.ToString());
        Json->SetStringField(TEXT("update_time"), Account.User.UpdatedAt.ToString());

        FString ProfileJson;
        TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&ProfileJson);
        FJsonSerializer::Serialize(Json.ToSharedRef(), Writer);

        LogDebug(FString::Printf(TEXT("Profile loaded for: %s"), *Account.User.Username));
        OnProfileLoaded.Broadcast(ProfileJson);
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnError.Broadcast(Error.Message);
    });

    NakamaClient->GetAccount(CurrentSession, SuccessCallback, ErrorCallback);
}

void UIVXManager::UpdateProfile(const FString& DisplayName, const FString& AvatarUrl, const FString& LangTag)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    auto SuccessCallback = FOnComplete::CreateLambda([this]()
    {
        LogDebug(TEXT("Profile updated"));
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnError.Broadcast(Error.Message);
    });

    NakamaClient->UpdateAccount(CurrentSession, FString(), DisplayName, AvatarUrl, LangTag, FString(), SuccessCallback, ErrorCallback);
}

void UIVXManager::FetchWallet()
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    auto SuccessCallback = FOnRpc::CreateLambda([this](const FNakamaRPC& Rpc)
    {
        LogDebug(FString::Printf(TEXT("Wallet fetched: %s"), *Rpc.Payload));
        OnWalletLoaded.Broadcast(Rpc.Payload);
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        LogError(FString::Printf(TEXT("FetchWallet failed: %s"), *Error.Message));
        OnError.Broadcast(Error.Message);
    });

    NakamaClient->RPC(CurrentSession, TEXT("hiro_economy_list"), TEXT("{}"), SuccessCallback, ErrorCallback);
}

void UIVXManager::GrantCurrency(const FString& CurrencyId, int64 Amount)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    FString Payload = FString::Printf(TEXT("{\"currencies\":{\"%s\":%lld}}"), *CurrencyId, Amount);

    auto SuccessCallback = FOnRpc::CreateLambda([this](const FNakamaRPC& Rpc)
    {
        LogDebug(FString::Printf(TEXT("Currency granted: %s"), *Rpc.Payload));
        OnWalletLoaded.Broadcast(Rpc.Payload);
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        LogError(FString::Printf(TEXT("GrantCurrency failed: %s"), *Error.Message));
        OnError.Broadcast(Error.Message);
    });

    NakamaClient->RPC(CurrentSession, TEXT("hiro_economy_grant"), Payload, SuccessCallback, ErrorCallback);
}

void UIVXManager::SubmitLeaderboardScore(const FString& LeaderboardId, int64 Score)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    auto SuccessCallback = FOnWriteLeaderboardRecord::CreateLambda([this](const FNakamaLeaderboardRecord& Record)
    {
        LogDebug(FString::Printf(TEXT("Score submitted: %lld"), Record.Score));
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnError.Broadcast(Error.Message);
    });

    NakamaClient->WriteLeaderboardRecord(CurrentSession, LeaderboardId, Score, 0, FString(), FString(), SuccessCallback, ErrorCallback);
}

void UIVXManager::FetchLeaderboard(const FString& LeaderboardId, int32 Limit)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    auto SuccessCallback = FOnListLeaderboardRecords::CreateLambda([this](const FNakamaLeaderboardRecordList& List)
    {
        TSharedPtr<FJsonObject> Root = MakeShareable(new FJsonObject());
        TArray<TSharedPtr<FJsonValue>> RecordsArray;

        for (const auto& Record : List.Records)
        {
            TSharedPtr<FJsonObject> Entry = MakeShareable(new FJsonObject());
            Entry->SetStringField(TEXT("owner_id"), Record.OwnerId);
            Entry->SetStringField(TEXT("username"), Record.Username);
            Entry->SetNumberField(TEXT("score"), Record.Score);
            Entry->SetNumberField(TEXT("rank"), Record.Rank);
            RecordsArray.Add(MakeShareable(new FJsonValueObject(Entry)));
        }

        Root->SetArrayField(TEXT("records"), RecordsArray);

        FString ResultJson;
        TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&ResultJson);
        FJsonSerializer::Serialize(Root.ToSharedRef(), Writer);

        LogDebug(FString::Printf(TEXT("Leaderboard fetched: %d records"), List.Records.Num()));
        OnLeaderboardFetched.Broadcast(ResultJson);
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnError.Broadcast(Error.Message);
    });

    NakamaClient->ListLeaderboardRecords(CurrentSession, LeaderboardId, {}, Limit, FString(), ENakamaLeaderboardListBy::BY_SCORE, SuccessCallback, ErrorCallback);
}

void UIVXManager::WriteStorageObject(const FString& Collection, const FString& Key, const FString& ValueJson)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    FNakamaStoreObjectWrite WriteObj;
    WriteObj.Collection = Collection;
    WriteObj.Key = Key;
    WriteObj.Value = ValueJson;
    WriteObj.PermissionRead = 1;
    WriteObj.PermissionWrite = 1;

    auto SuccessCallback = FOnStorageObjectAcks::CreateLambda([this](const FNakamaStoreObjectAcks& Acks)
    {
        LogDebug(TEXT("Storage write complete"));
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnError.Broadcast(Error.Message);
    });

    NakamaClient->WriteStorageObjects(CurrentSession, { WriteObj }, SuccessCallback, ErrorCallback);
}

void UIVXManager::ReadStorageObject(const FString& Collection, const FString& Key)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    FNakamaReadStorageObjectId ReadId;
    ReadId.Collection = Collection;
    ReadId.Key = Key;
    ReadId.UserId = GetUserId();

    auto SuccessCallback = FOnStorageObjectsRead::CreateLambda([this](const FNakamaStorageObjectList& Objects)
    {
        FString ValueJson = TEXT("{}");
        if (Objects.Objects.Num() > 0)
        {
            ValueJson = Objects.Objects[0].Value;
        }

        LogDebug(FString::Printf(TEXT("Storage read: %d objects"), Objects.Objects.Num()));
        OnStorageRead.Broadcast(ValueJson);
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        OnError.Broadcast(Error.Message);
    });

    NakamaClient->ReadStorageObjects(CurrentSession, { ReadId }, SuccessCallback, ErrorCallback);
}

void UIVXManager::CallRpc(const FString& RpcId, const FString& PayloadJson)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    auto SuccessCallback = FOnRpc::CreateLambda([this, RpcId](const FNakamaRPC& Rpc)
    {
        LogDebug(FString::Printf(TEXT("RPC %s response: %s"), *RpcId, *Rpc.Payload));
        OnRpcResult.Broadcast(RpcId, Rpc.Payload);
    });

    auto ErrorCallback = FOnError::CreateLambda([this, RpcId](const FNakamaError& Error)
    {
        LogError(FString::Printf(TEXT("RPC %s failed: %s"), *RpcId, *Error.Message));
        OnError.Broadcast(Error.Message);
    });

    NakamaClient->RPC(CurrentSession, RpcId, PayloadJson, SuccessCallback, ErrorCallback);
}

void UIVXManager::OnAuthSuccess(UNakamaSession* Session)
{
    CurrentSession = Session;
    SaveSessionToLocal(Session);
    LogDebug(FString::Printf(TEXT("Authenticated — UserId: %s, Username: %s"), *Session->GetUserId(), *Session->GetUsername()));
    SyncPlayerMetadata();
    OnAuthenticated.Broadcast();
}

void UIVXManager::OnAuthError(const FNakamaError& Error)
{
    LogError(FString::Printf(TEXT("Auth failed: %s"), *Error.Message));
    OnError.Broadcast(Error.Message);
}

void UIVXManager::SaveSessionToLocal(UNakamaSession* Session)
{
    if (Session)
    {
        GConfig->SetString(TEXT("IntelliVerseX"), *IVX_SESSION_TOKEN_KEY, *Session->GetAuthToken(), GGameIni);
        GConfig->SetString(TEXT("IntelliVerseX"), *IVX_REFRESH_TOKEN_KEY, *Session->GetRefreshToken(), GGameIni);
        GConfig->Flush(false, GGameIni);
    }
}

UNakamaSession* UIVXManager::LoadSessionFromLocal()
{
    FString Token, RefreshToken;
    GConfig->GetString(TEXT("IntelliVerseX"), *IVX_SESSION_TOKEN_KEY, Token, GGameIni);
    GConfig->GetString(TEXT("IntelliVerseX"), *IVX_REFRESH_TOKEN_KEY, RefreshToken, GGameIni);

    if (Token.IsEmpty())
    {
        return nullptr;
    }

    return UNakamaSession::RestoreSession(Token, RefreshToken);
}

FString UIVXManager::GetPersistentDeviceId() const
{
    FString DeviceId;
    GConfig->GetString(TEXT("IntelliVerseX"), *IVX_DEVICE_ID_KEY, DeviceId, GGameIni);

    if (DeviceId.IsEmpty())
    {
        DeviceId = FGuid::NewGuid().ToString();
        GConfig->SetString(TEXT("IntelliVerseX"), *IVX_DEVICE_ID_KEY, *DeviceId, GGameIni);
        GConfig->Flush(false, GGameIni);
    }

    return DeviceId;
}

void UIVXManager::SyncPlayerMetadata()
{
    if (!HasValidSession())
    {
        return;
    }

    TSharedPtr<FJsonObject> Meta = MakeShareable(new FJsonObject());
    Meta->SetStringField(TEXT("sdk_version"), TEXT("5.1.0"));
    Meta->SetStringField(TEXT("platform"), UGameplayStatics::GetPlatformName());
    Meta->SetStringField(TEXT("engine"), TEXT("unreal"));

    FString MetaString;
    TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&MetaString);
    FJsonSerializer::Serialize(Meta.ToSharedRef(), Writer);

    FString Payload = FString::Printf(TEXT("{\"metadata\":%s}"), *MetaString);

    auto SuccessCallback = FOnRpc::CreateLambda([this](const FNakamaRPC& Rpc)
    {
        LogDebug(TEXT("Player metadata synced"));
    });

    auto ErrorCallback = FOnError::CreateLambda([this](const FNakamaError& Error)
    {
        LogDebug(FString::Printf(TEXT("Metadata sync failed (non-fatal): %s"), *Error.Message));
    });

    NakamaClient->RPC(CurrentSession, TEXT("ivx_sync_metadata"), Payload, SuccessCallback, ErrorCallback);
}

void UIVXManager::LogDebug(const FString& Message) const
{
    if (SDKConfig && SDKConfig->bEnableDebugLogs)
    {
        UE_LOG(LogIVX, Log, TEXT("%s"), *Message);
    }
}

void UIVXManager::LogError(const FString& Message) const
{
    UE_LOG(LogIVX, Error, TEXT("%s"), *Message);
}
