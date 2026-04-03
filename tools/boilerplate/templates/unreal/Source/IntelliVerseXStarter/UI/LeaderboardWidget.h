#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "LeaderboardWidget.generated.h"

UCLASS()
class INTELLIVERSEXSTARTER_API ULeaderboardWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	virtual void NativeConstruct() override;
};
