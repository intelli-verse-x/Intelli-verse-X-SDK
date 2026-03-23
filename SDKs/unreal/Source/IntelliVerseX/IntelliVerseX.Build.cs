// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

using UnrealBuildTool;

public class IntelliVerseX : ModuleRules
{
    public IntelliVerseX(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;

        PublicDependencyModuleNames.AddRange(new string[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "HTTP",
            "Json",
            "JsonUtilities",
            "NakamaUnreal"
        });
    }
}
