# Testing Guide

Best practices for testing your SDK integration.

## Unit Testing

### Setup

Add the SDK test assembly reference to your test assembly.

### Example Test

```csharp
using NUnit.Framework;
using IntelliVerseX.Core;

public class SDKTests
{
    [Test]
    public void SDK_Initializes_Successfully()
    {
        // Arrange & Act
        IVXCore.Initialize();
        
        // Assert
        Assert.IsTrue(IVXCore.IsInitialized);
    }
}
```

## PlayMode Testing

Use Unity Test Framework for integration tests.

## See Also

- [Troubleshooting FAQ](../troubleshooting/faq.md)
