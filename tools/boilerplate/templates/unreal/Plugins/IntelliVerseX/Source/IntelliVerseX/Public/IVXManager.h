#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXManager.generated.h"

UCLASS(BlueprintType, Blueprintable)
class INTELLIVERSEX_API UIVXManager : public UObject
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintCallable, Category="IntelliVerseX")
	void Initialize();
};
