#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXHiro.generated.h"

UCLASS(BlueprintType, Blueprintable)
class INTELLIVERSEX_API UIVXHiro : public UObject
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintCallable, Category="IntelliVerseX|Hiro")
	void SyncEconomy();
};
