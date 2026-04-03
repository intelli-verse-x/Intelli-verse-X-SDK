#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "StoreWidget.generated.h"

UCLASS()
class INTELLIVERSEXSTARTER_API UStoreWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	virtual void NativeConstruct() override;
};
