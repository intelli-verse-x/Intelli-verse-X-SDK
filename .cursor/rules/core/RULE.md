# Core SDK Rules

## Applies To
All files under `Assets/Intelli-verse-X-SDK/` and `Assets/_IntelliVerseXSDK/`.

## Rules

1. **Namespace**: All code must be in `IntelliVerseX.*` namespace
2. **Prefix**: Public types use `IVX` prefix, interfaces use `IIVX`
3. **Singleton**: Managers use singleton pattern with `DontDestroyOnLoad`
4. **Events**: Use C# `Action<T>` events for decoupling
5. **Async**: Use `Task` for async operations, suffix with `Async`
6. **Logging**: Use `Debug.Log($"[{nameof(ClassName)}] message")`
7. **Null Safety**: Always check references, use `?.` operator
8. **No GC in Update**: No LINQ, lambdas, string concat, boxing in hot paths
9. **XML Docs**: All public APIs must have XML documentation comments
10. **Regions**: Use `#region` blocks (Constants, Serialized Fields, Private Fields, Properties, Unity Lifecycle, Public Methods, Private Methods)
