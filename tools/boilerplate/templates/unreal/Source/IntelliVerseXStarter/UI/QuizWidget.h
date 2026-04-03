#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "QuizWidget.generated.h"

UCLASS()
class INTELLIVERSEXSTARTER_API UQuizWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	virtual void NativeConstruct() override;
};
