# Creating Custom Modules

Learn how to extend the SDK with custom modules.

## Module Structure

```
YourModule/
├── YourModule.asmdef
├── Runtime/
│   ├── YourModuleManager.cs
│   └── YourModuleConfig.cs
└── Editor/
    └── YourModuleEditor.cs
```

## Creating a Module Manager

```csharp
using IntelliVerseX.Core;
using UnityEngine;

namespace YourNamespace
{
    public class YourModuleManager : IVXSingleton<YourModuleManager>
    {
        protected override void OnInitialize()
        {
            Debug.Log("Your module initialized!");
        }
    }
}
```

## Registering with SDK

Add your module to the SDK initialization sequence.

## See Also

- [Core Module](../modules/core.md)
