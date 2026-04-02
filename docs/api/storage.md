# Storage API Reference

IntelliVerseX storage spans **local encrypted storage** (`IVXSecureStorage`) and **Nakama cloud storage** (user-scoped objects identified by collection + key). This page documents the **cloud storage** surface as async operations aligned with Nakama’s storage engine, plus how other SDKs expose the same behavior.

## Namespaces and assemblies

| Surface | Namespace / package |
|--------|---------------------|
| Unity local | `IntelliVerseX.Storage` — `IVXSecureStorage` |
| Unity + Nakama | Use `Nakama.IClient` with an authenticated `ISession` (same semantics as below) |
| JavaScript / Web3 | `@intelliversex/sdk` — `readStorage`, `writeStorage` |
| Java / Android | `com.intelliversex.sdk.core.IVXManager` — `readStorage`, `writeStorage` |
| Flutter | `ivx_manager.dart` — `readStorage`, `writeStorage` |

## Nakama storage concepts

| Concept | Description |
|--------|-------------|
| **Collection** | Logical bucket for objects (e.g. `game_saves`, `settings`, `profile`). Use consistent naming per game. |
| **Key** | Unique object id within a collection for a user. |
| **User id** | Owner of the object. Reads for “self” use the session user; public reads specify another user id. |
| **Value** | Opaque payload, typically JSON. |
| **Permission read / write** | Who may read or write. Common patterns: owner-only read/write; **public read** for leaderboards or shared profiles (set when writing). |

Official Nakama overview: [Storage collections](https://heroiclabs.com/docs/nakama/concepts/storage/).

---

## Cloud API: `ReadAsync`, `WriteAsync`, `DeleteAsync`, `ListAsync`, `ReadPublicAsync`

These names describe the **recommended async contract** for cloud objects. In C# with the Nakama Unity client, they map to `ReadStorageObjectsAsync`, `WriteStorageObjectsAsync`, `DeleteStorageObjectsAsync`, `ListStorageObjectsAsync`, and a public read is a read with `UserId` set to the target user and appropriate read permission on the object.

### `ReadAsync`

| Parameter | Type | Description |
|-----------|------|-------------|
| `collection` | `string` | Storage collection name. |
| `key` | `string` | Object key within the collection. |
| `userId` | `string` | Optional; defaults to current session user. |

| Returns | Description |
|---------|-------------|
| `Task<T>` / `Task<string>` | Deserialized payload or raw JSON. Empty or missing object: handle as `null` / `{}`. |

**Example (Unity / Nakama):**

```csharp
var readId = new StorageObjectId {
    Collection = "game_saves",
    Key = "slot_1",
    UserId = session.UserId
};
var result = await client.ReadStorageObjectsAsync(session, new[] { readId });
// Parse result.Objects[0].Value
```

**Cross-platform:** Java `readStorage(collection, key)` returns JSON string; JS `readStorage(collection, key)` returns parsed JSON.

### `WriteAsync`

| Parameter | Type | Description |
|-----------|------|-------------|
| `collection` | `string` | Collection name. |
| `key` | `string` | Object key. |
| `value` | `object` or JSON | Serializable game state. |
| `permissionRead` | enum | e.g. owner-only vs public read. |
| `permissionWrite` | enum | Typically owner write. |

| Returns | Description |
|---------|-------------|
| `Task` | Completes when Nakama acknowledges the write. |

**Example (JavaScript):**

```typescript
await ivx.writeStorage('game_saves', 'slot1', {
  level: 12,
  checkpoint: 'forest',
  savedAt: Date.now(),
});
```

**Example (Java):**

```java
mgr.writeStorage("game_data", "prefs", "{\"volume\":0.8}");
```

### `DeleteAsync`

| Parameter | Type | Description |
|-----------|------|-------------|
| `collection` | `string` | Collection name. |
| `key` | `string` | Key to remove. |

| Returns | Description |
|---------|-------------|
| `Task` | Completes after delete RPC succeeds. |

Use deletes for GDPR erasure, save-slot management, or resetting cached server state. Handle “not found” as idempotent success where appropriate.

### `ListAsync`

| Parameter | Type | Description |
|-----------|------|-------------|
| `collection` | `string` | Collection to enumerate. |
| `limit` | `int` | Max objects (respect server caps). |
| `cursor` | `string` | Pagination cursor from previous page. |

| Returns | Description |
|---------|-------------|
| `Task<(IList<...> objects, string cursor)>` | Object ids, versions, and optional values. |

Use listing for save-slot pickers, admin tools, or migration.

### `ReadPublicAsync`

| Parameter | Type | Description |
|-----------|------|-------------|
| `collection` | `string` | Must match object written with public-read permission. |
| `key` | `string` | Object key. |
| `userId` | `string` | **Required** — profile or content owner to read from. |

| Returns | Description |
|---------|-------------|
| `Task<T>` | Public payload or empty if denied / missing. |

Objects must be stored with a read permission that allows the current session to read another user’s data (e.g. public read for showcase profiles).

---

## Error handling patterns

| Situation | Suggested handling |
|-----------|-------------------|
| No session | Do not call cloud APIs; authenticate first. |
| Network / timeout | Retry with backoff; keep a local copy via `IVXSecureStorage`. |
| Permission denied | Verify `permissionRead` / `permissionWrite` on write; user id on read. |
| Version conflict | Nakama supports version checks on write; re-read, merge, write again. |
| Empty read | Treat as new save; initialize defaults. |

**Unity pattern:**

```csharp
try {
    await client.WriteStorageObjectsAsync(session, writes);
} catch (ApiResponseException ex) {
    Debug.LogWarning($"Storage write failed: {ex.Message}");
    IVXSecureStorage.SetString("pending_cloud_sync", json);
}
```

---

## Local storage (`IVXSecureStorage`)

For offline-first UX, persist critical data locally and sync when online:

```csharp
using IntelliVerseX.Storage;

IVXSecureStorage.SetObject("last_save", localData);
// Later: merge with cloud using ReadAsync / WriteAsync
```

---

## See also

- [Storage module](../modules/storage.md)
- [Nakama storage concepts](https://heroiclabs.com/docs/nakama/concepts/storage/)
- Java reference: `IVXManager.readStorage` / `writeStorage` in `SDKs/java/.../IVXManager.java`
