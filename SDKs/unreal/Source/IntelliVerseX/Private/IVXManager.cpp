#include "IVXManager.h"
#include "Kismet/GameplayStatics.h"
#include "Misc/Guid.h"
#include "JsonObjectConverter.h"

DEFINE_LOG_CATEGORY(LogIVX);

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

FString UIVXManager::ValidateNakamaConfig(UIVXConfig* Config)
{
    if (!Config)
    {
        return TEXT("Config is null.");
    }
    if (Config->NakamaHost.IsEmpty())
    {
        return TEXT("Nakama Host is empty. Set it in IVXConfig (e.g. 127.0.0.1).");
    }
    if (Config->NakamaPort <= 0 || Config->NakamaPort > 65535)
    {
        return FString::Printf(TEXT("Nakama Port %d is invalid. Use 7350 for default HTTP."), Config->NakamaPort);
    }
    if (Config->NakamaServerKey.IsEmpty())
    {
        return TEXT("Nakama Server Key is empty. Set it in IVXConfig (e.g. defaultkey).");
    }
    if (Config->NakamaHost.Contains(TEXT("http://")) || Config->NakamaHost.Contains(TEXT("https://")))
    {
        return TEXT("Nakama Host must not include http:// or https://. Use host only (e.g. 127.0.0.1).");
    }
    return FString();
}

void UIVXManager::InitializeSDK(UIVXConfig* Config)
{
    if (!Config)
    {
        LogError(TEXT("InitializeSDK called with null config"));
        OnError.Broadcast(TEXT("Config is null"));
        return;
    }

    FString ValidationError = ValidateNakamaConfig(Config);
    if (!ValidationError.IsEmpty())
    {
        LogError(FString::Printf(TEXT("IVX Config invalid: %s"), *ValidationError));
        OnError.Broadcast(ValidationError);
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

    FOnAuthUpdate SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnAuthSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->AuthenticateDevice(ResolvedId, FString(), true, {}, SuccessCallback, ErrorCallback);
}

void UIVXManager::AuthenticateWithEmail(const FString& Email, const FString& Password, bool bCreate)
{
    if (!bIsInitialized || !NakamaClient)
    {
        OnError.Broadcast(TEXT("SDK not initialized"));
        return;
    }

    FOnAuthUpdate SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnAuthSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->AuthenticateEmail(Email, Password, FString(), bCreate, {}, SuccessCallback, ErrorCallback);
}

void UIVXManager::AuthenticateWithGoogle(const FString& Token)
{
    if (!bIsInitialized || !NakamaClient)
    {
        OnError.Broadcast(TEXT("SDK not initialized"));
        return;
    }

    FOnAuthUpdate SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnAuthSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->AuthenticateGoogle(Token, FString(), true, {}, SuccessCallback, ErrorCallback);
}

void UIVXManager::AuthenticateWithApple(const FString& Token)
{
    if (!bIsInitialized || !NakamaClient)
    {
        OnError.Broadcast(TEXT("SDK not initialized"));
        return;
    }

    FOnAuthUpdate SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnAuthSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->AuthenticateApple(Token, FString(), true, {}, SuccessCallback, ErrorCallback);
}

void UIVXManager::AuthenticateWithCustomId(const FString& CustomId)
{
    if (!bIsInitialized || !NakamaClient)
    {
        OnError.Broadcast(TEXT("SDK not initialized"));
        return;
    }

    FOnAuthUpdate SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnAuthSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

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

    FOnUserAccountInfo SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnGetAccountSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->GetAccount(CurrentSession, SuccessCallback, ErrorCallback);
}

void UIVXManager::OnGetAccountSuccess(const FNakamaAccount& AccountData)
{
    const FNakamaUser& User = AccountData.User;
    TSharedPtr<FJsonObject> Json = MakeShareable(new FJsonObject());
    Json->SetStringField(TEXT("user_id"), User.Id);
    Json->SetStringField(TEXT("username"), User.Username);
    Json->SetStringField(TEXT("display_name"), User.DisplayName);
    Json->SetStringField(TEXT("avatar_url"), User.AvatarUrl);
    Json->SetStringField(TEXT("lang_tag"), User.Language);
    Json->SetStringField(TEXT("metadata"), User.MetaData);
    Json->SetStringField(TEXT("wallet"), AccountData.Wallet);
    Json->SetStringField(TEXT("email"), AccountData.Email);
    Json->SetStringField(TEXT("create_time"), User.CreatedAt.ToString());
    Json->SetStringField(TEXT("update_time"), User.updatedAt.ToString());

    FString ProfileJson;
    TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&ProfileJson);
    FJsonSerializer::Serialize(Json.ToSharedRef(), Writer);

    LogDebug(FString::Printf(TEXT("Profile loaded for: %s"), *User.Username));
    OnProfileLoaded.Broadcast(ProfileJson);
}

void UIVXManager::UpdateProfile(const FString& DisplayName, const FString& AvatarUrl, const FString& LangTag)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    FOnUpdateAccount SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnUpdateAccountSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->UpdateAccount(CurrentSession, FString(), DisplayName, AvatarUrl, LangTag, FString(), FString(), SuccessCallback, ErrorCallback);
}

void UIVXManager::OnUpdateAccountSuccess()
{
    LogDebug(TEXT("Profile updated"));
}

void UIVXManager::FetchWallet()
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    PendingRpcPurpose = ERpcPurpose::Wallet;
    FOnRPC SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnRpcSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

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

    PendingRpcPurpose = ERpcPurpose::Grant;
    FOnRPC SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnRpcSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->RPC(CurrentSession, TEXT("hiro_economy_grant"), Payload, SuccessCallback, ErrorCallback);
}

void UIVXManager::OnRpcSuccess(const FNakamaRPC& rpc)
{
    switch (PendingRpcPurpose)
    {
    case ERpcPurpose::Wallet:
    case ERpcPurpose::Grant:
        LogDebug(FString::Printf(TEXT("Wallet RPC response: %s"), *rpc.Payload));
        OnWalletLoaded.Broadcast(rpc.Payload);
        break;
    case ERpcPurpose::Generic:
        LogDebug(FString::Printf(TEXT("RPC %s response: %s"), *PendingRpcId, *rpc.Payload));
        OnRpcResult.Broadcast(PendingRpcId, rpc.Payload);
        break;
    case ERpcPurpose::Sync:
        LogDebug(TEXT("Player metadata synced"));
        break;
    default:
        break;
    }
    PendingRpcPurpose = ERpcPurpose::None;
    PendingRpcId.Empty();
}

void UIVXManager::SubmitLeaderboardScore(const FString& LeaderboardId, int64 Score)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    FOnWriteLeaderboardRecord SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnWriteLeaderboardSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->WriteLeaderboardRecord(CurrentSession, LeaderboardId, Score, 0, FString(), SuccessCallback, ErrorCallback);
}

void UIVXManager::OnWriteLeaderboardSuccess(const FNakamaLeaderboardRecord& Record)
{
    LogDebug(FString::Printf(TEXT("Score submitted: %lld"), Record.Score));
}

void UIVXManager::FetchLeaderboard(const FString& LeaderboardId, int32 Limit)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    FOnListLeaderboardRecords SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnListLeaderboardSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->ListLeaderboardRecords(CurrentSession, LeaderboardId, {}, Limit, FString(), ENakamaLeaderboardListBy::BY_SCORE, SuccessCallback, ErrorCallback);
}

void UIVXManager::OnListLeaderboardSuccess(const FNakamaLeaderboardRecordList& RecordsList)
{
    TSharedPtr<FJsonObject> Root = MakeShareable(new FJsonObject());
    TArray<TSharedPtr<FJsonValue>> RecordsArray;

    for (const auto& Record : RecordsList.Records)
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

    LogDebug(FString::Printf(TEXT("Leaderboard fetched: %d records"), RecordsList.Records.Num()));
    OnLeaderboardFetched.Broadcast(ResultJson);
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
    WriteObj.PermissionRead = ENakamaStoragePermissionRead::OWNER_READ;
    WriteObj.PermissionWrite = ENakamaStoragePermissionWrite::OWNER_WRITE;

    FOnStorageObjectAcks SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnStorageWriteSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->WriteStorageObjects(CurrentSession, { WriteObj }, SuccessCallback, ErrorCallback);
}

void UIVXManager::OnStorageWriteSuccess(const FNakamaStoreObjectAcks& StorageObjectsAcks)
{
    LogDebug(TEXT("Storage write complete"));
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

    FOnStorageObjectsRead SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnStorageReadSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->ReadStorageObjects(CurrentSession, { ReadId }, SuccessCallback, ErrorCallback);
}

void UIVXManager::OnStorageReadSuccess(const FNakamaStorageObjectList& StorageObjects)
{
    FString ValueJson = TEXT("{}");
    if (StorageObjects.Objects.Num() > 0)
    {
        ValueJson = StorageObjects.Objects[0].Value;
    }

    LogDebug(FString::Printf(TEXT("Storage read: %d objects"), StorageObjects.Objects.Num()));
    OnStorageRead.Broadcast(ValueJson);
}

void UIVXManager::CallRpc(const FString& RpcId, const FString& PayloadJson)
{
    if (!HasValidSession())
    {
        OnError.Broadcast(TEXT("No valid session"));
        return;
    }

    PendingRpcId = RpcId;
    PendingRpcPurpose = ERpcPurpose::Generic;
    FOnRPC SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnRpcSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnAuthError);

    NakamaClient->RPC(CurrentSession, RpcId, PayloadJson, SuccessCallback, ErrorCallback);
}

void UIVXManager::OnAuthSuccess(UNakamaSession* LoginData)
{
    CurrentSession = LoginData;
    SaveSessionToLocal(LoginData);
    LogDebug(FString::Printf(TEXT("Authenticated — UserId: %s, Username: %s"), *LoginData->GetUserId(), *LoginData->GetUsername()));
    SyncPlayerMetadata();
    OnAuthenticated.Broadcast();
}

void UIVXManager::OnAuthError(const FNakamaError& ErrorData)
{
    FString Msg = ErrorData.Message;
    const bool bRpcNotFound = Msg.Contains(TEXT("RPC function not found"));

    if (bRpcNotFound)
    {
        switch (PendingRpcPurpose)
        {
        case ERpcPurpose::Wallet:
            UE_LOG(LogIVX, Warning, TEXT("Wallet RPC not on server, returning empty wallet: %s"), *Msg);
            OnWalletLoaded.Broadcast(TEXT("{}"));
            break;
        case ERpcPurpose::Grant:
            UE_LOG(LogIVX, Warning, TEXT("Grant RPC not on server, skipping: %s"), *Msg);
            break;
        case ERpcPurpose::Generic:
            UE_LOG(LogIVX, Warning, TEXT("RPC %s not on server: %s"), *PendingRpcId, *Msg);
            OnRpcResult.Broadcast(PendingRpcId, TEXT("{}"));
            break;
        default:
            LogError(FString::Printf(TEXT("Auth/RPC error: %s"), *Msg));
            OnError.Broadcast(Msg);
            break;
        }
        PendingRpcPurpose = ERpcPurpose::None;
        PendingRpcId.Empty();
        return;
    }

    if (Msg.Contains(TEXT("Failed to process request")) || Msg.Contains(TEXT("Request failed")))
    {
        Msg += TEXT(" [Check: 1) Nakama server running  2) Host/port correct in IVXConfig  3) Server key matches  4) Firewall/network]");
    }
    LogError(FString::Printf(TEXT("Auth failed: %s"), *Msg));
    OnError.Broadcast(Msg);
    PendingRpcPurpose = ERpcPurpose::None;
    PendingRpcId.Empty();
}

void UIVXManager::OnSyncMetadataError(const FNakamaError& ErrorData)
{
    PendingRpcPurpose = ERpcPurpose::None;
    PendingRpcId.Empty();
    UE_LOG(LogIVX, Warning, TEXT("Metadata sync skipped (ivx_sync_metadata RPC not on server): %s"), *ErrorData.Message);
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

    PendingRpcPurpose = ERpcPurpose::Sync;
    FOnRPC SuccessCallback;
    SuccessCallback.AddDynamic(this, &UIVXManager::OnRpcSuccess);
    FOnError ErrorCallback;
    ErrorCallback.AddDynamic(this, &UIVXManager::OnSyncMetadataError);

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
