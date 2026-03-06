#pragma once

#include "Modules/ModuleManager.h"

class FIntelliVerseXModule : public IModuleInterface
{
public:
    virtual void StartupModule() override;
    virtual void ShutdownModule() override;
};
