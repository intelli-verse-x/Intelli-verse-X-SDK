#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "GameBootstrap.generated.h"

class UIVXManager;

/**
 * Place in level or spawn from GameMode: configures IVX, authenticates, loads Hiro.
 */
UCLASS()
class INTELLIVERSEXSTARTER_API AGameBootstrap : public AActor
{
	GENERATED_BODY()

public:
	AGameBootstrap();

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IVX")
	FString GameId = TEXT("{{game_id}}");

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IVX")
	FString ServerHost = TEXT("{{server_host}}");

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IVX")
	int32 ServerPort = {{server_port}};

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "IVX")
	FString ServerKey = TEXT("{{server_key}}");

protected:
	virtual void BeginPlay() override;
};
