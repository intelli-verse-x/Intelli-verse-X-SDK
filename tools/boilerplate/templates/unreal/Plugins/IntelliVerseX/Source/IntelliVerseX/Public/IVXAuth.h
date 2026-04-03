#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "IVXAuth.generated.h"

UCLASS(BlueprintType, Blueprintable)
class INTELLIVERSEX_API UIVXAuth : public UObject
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintCallable, Category="IntelliVerseX|Auth")
	void Authenticate();
};
