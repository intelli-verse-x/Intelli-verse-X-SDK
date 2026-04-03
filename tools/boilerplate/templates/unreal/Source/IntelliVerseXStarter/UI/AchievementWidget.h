#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "AchievementWidget.generated.h"

UCLASS()
class INTELLIVERSEXSTARTER_API UAchievementWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	virtual void NativeConstruct() override;
};
