#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXSatori.generated.h"

UCLASS(BlueprintType, Blueprintable)
class INTELLIVERSEX_API UIVXSatori : public UObject
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintCallable, Category="IntelliVerseX|Satori")
	void GetLiveEvents();
};
