#include "GameBootstrap.h"

#include "GameConfig.h"
#include "Engine/Engine.h"
#include "IVXManager.h"

AGameBootstrap::AGameBootstrap()
{
	PrimaryActorTick.bCanEverTick = false;
}

void AGameBootstrap::BeginPlay()
{
	Super::BeginPlay();

	GameId = FString(FGameConfig::GAME_ID);
	ServerHost = FString(FGameConfig::SERVER_HOST);
	ServerPort = FGameConfig::SERVER_PORT;
	ServerKey = FString(FGameConfig::SERVER_KEY);

	UIVXManager* const Mgr = UIVXManager::Get(GetWorld());
	if (!Mgr)
	{
		UE_LOG(LogTemp, Error, TEXT("[IVX] UIVXManager not available — add IntelliVerseX plugin"));
		return;
	}

	Mgr->Configure(GameId, ServerHost, ServerPort, ServerKey);
	Mgr->AuthenticatePlayer();
	Mgr->LoadHiroSystems();

	TMap<FString, FString> Meta;
	Meta.Add(TEXT("game_id"), GameId);
	Mgr->TrackEvent(TEXT("session_start"), Meta);
}
