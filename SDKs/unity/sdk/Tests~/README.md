# IntelliVerseX SDK Tests

This folder contains unit tests and integration tests for the IntelliVerseX SDK.

## Test Structure

```
Tests~/
├── Editor/                    # Edit mode tests
│   ├── IntelliVerseX.Editor.Tests.asmdef
│   └── (test files)
├── Runtime/                   # Play mode tests
│   ├── IntelliVerseX.Tests.asmdef
│   └── (test files)
└── README.md
```

## Running Tests

### Via Unity Test Runner

1. Open **Window > General > Test Runner**
2. Select **EditMode** or **PlayMode** tab
3. Click **Run All** or select specific tests

### Via Command Line

```bash
# Edit Mode Tests
Unity.exe -runTests -testPlatform editmode -projectPath . -testResults results.xml

# Play Mode Tests
Unity.exe -runTests -testPlatform playmode -projectPath . -testResults results.xml
```

## Test Categories

### Unit Tests (EditMode)
- Core utilities
- Data models
- Serialization
- Configuration validation

### Integration Tests (PlayMode)
- Manager initialization
- Backend connectivity (mocked)
- UI components
- Event system

## Writing Tests

### Example EditMode Test

```csharp
using NUnit.Framework;
using IntelliVerseX.Core;

namespace IntelliVerseX.Tests.Editor
{
    public class CoreUtilsTests
    {
        [Test]
        public void ValidateUserId_WithValidId_ReturnsTrue()
        {
            // Arrange
            string validId = "user_123456";
            
            // Act
            bool result = IVXCoreUtils.ValidateUserId(validId);
            
            // Assert
            Assert.IsTrue(result);
        }
    }
}
```

### Example PlayMode Test

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using IntelliVerseX.Core;

namespace IntelliVerseX.Tests.Runtime
{
    public class ManagerInitializationTests
    {
        [UnityTest]
        public IEnumerator IVXManager_Initialize_CompletesSuccessfully()
        {
            // Arrange & Act
            var manager = new GameObject().AddComponent<IVXManager>();
            
            yield return null; // Wait one frame
            
            // Assert
            Assert.IsNotNull(manager);
            Assert.IsTrue(manager.IsInitialized);
        }
    }
}
```

## Test Coverage Goals

| Module | Target Coverage |
|--------|-----------------|
| Core | 80% |
| Identity | 70% |
| Backend | 60% |
| Localization | 80% |
| UI | 50% |
