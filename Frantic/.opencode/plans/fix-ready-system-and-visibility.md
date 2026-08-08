# Fix Plan: Ready System & Player/Projectile Visibility

## Issue 1: Ready System Not Working

### Root Cause
`InteractionNetwork.cs:33` passes `OwnerClientId` to `GameManager.RequestReadyServerRpc()`. This is unreliable because `OwnerClientId` on the InteractionNetwork component may not represent the actual RPC sender.

### Fix: 3-file chain

#### 1. `Assets/Scripts/Networking/Interaction/InteractionNetwork.cs` (line 33)
**Before:**
```csharp
GameManager.Instance?.RequestReadyServerRpc(OwnerClientId);
```
**After:**
```csharp
var playerNetwork = GetComponent<PlayerNetwork>();
playerNetwork?.InteractServerRpc();
```
The interaction component delegates to the player, which has the correct RPC sender context.

#### 2. `Assets/Scripts/Networking/Player/PlayerNetwork.cs` (lines 40-47)
**Before:**
```csharp
[ServerRpc]
public void InteractServerRpc(ServerRpcParams serverRpcParams = default)
{
    if (!IsServer) return;
    var callerId = serverRpcParams.Receive.SenderClientId;
    Debug.Log($"[Player] {callerId} interacting");
}
```
**After:**
```csharp
[ServerRpc]
public void InteractServerRpc(ServerRpcParams serverRpcParams = default)
{
    if (!IsServer) return;
    var callerId = serverRpcParams.Receive.SenderClientId;
    Debug.Log($"[Player] {callerId} interacting with dungeon entrance");
    GameManager.Instance?.RequestReadyServerRpc(callerId);
}
```
The player's RPC already receives `serverRpcParams` with the correct sender. It forwards that sender ID to GameManager.

#### 3. `Assets/Scripts/Networking/State/GameManager.cs` (lines 37-51)
**Before:**
```csharp
[ServerRpc]
public void RequestReadyServerRpc(ulong callerId, ServerRpcParams serverRpcParams = default)
{
    if (!IsServer) return;
    Debug.Log($"[GameManager] Player {callerId} readying up");
    if (_currentState != GameState.Hub) return;
    var readyManager = GetComponent<ReadyManager>();
    if (readyManager != null)
    {
        readyManager.SetReady(callerId, true);
    }
}
```
**After:**
```csharp
[ServerRpc]
public void RequestReadyServerRpc(ulong callerId, ServerRpcParams serverRpcParams = default)
{
    if (!IsServer) return;
    var actualCaller = serverRpcParams.Receive.SenderClientId;
    Debug.Log($"[GameManager] Player {actualCaller} readying up (RPC sender)");
    if (_currentState != GameState.Hub) return;
    var readyManager = GetComponent<ReadyManager>();
    if (readyManager != null)
    {
        readyManager.SetReady(actualCaller, true);
    }
}
```
Uses `serverRpcParams.Receive.SenderClientId` (the true RPC caller) instead of the `callerId` parameter (which was `OwnerClientId` from the old code).

---

## Issue 2: Player Prefab Not Visible to Clients

### Root Cause
`SetupGame.cs:248-251` adds enemy, room, and projectile to `DefaultNetworkPrefabsList` but **skips the player prefab**. The player is set as `NetworkConfig.PlayerPrefab` but must also be in `DefaultNetworkPrefabsList` for clients to deserialize spawned player objects.

### Fix: `Assets/Scripts/Editor/SetupGame.cs` (lines 248-261)

**Before:**
```csharp
var prefabsToAdd = new List<GameObject>();
if (enemyPrefab != null) prefabsToAdd.Add(enemyPrefab);
if (roomPrefab != null) prefabsToAdd.Add(roomPrefab);
if (projectilePrefab != null) prefabsToAdd.Add(projectilePrefab);
```

**After:**
```csharp
var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
var prefabsToAdd = new List<GameObject>();
if (playerPrefab != null) prefabsToAdd.Add(playerPrefab);
if (enemyPrefab != null) prefabsToAdd.Add(enemyPrefab);
if (roomPrefab != null) prefabsToAdd.Add(roomPrefab);
if (projectilePrefab != null) prefabsToAdd.Add(projectilePrefab);
```

**After fixing SetupGame, re-run:** `Frantic -> Setup All` then `Frantic -> Configure Hub Scene`

---

## Issue 3: Projectile Not Visible / Not Moving

### Root Cause A: No NetworkTransform on projectile prefab
Without `NetworkTransform`, position changes on the server won't sync to clients.

### Fix A: `Assets/Scripts/Editor/SetupGame.cs` (CreateProjectilePrefab method, ~line 199)

**Before:**
```csharp
var projRoot = new GameObject("Projectile");
projRoot.AddComponent<NetworkObject>();
projRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
projRoot.AddComponent<CircleCollider2D>().radius = 0.15f;
```

**After:**
```csharp
var projRoot = new GameObject("Projectile");
projRoot.AddComponent<NetworkObject>();
var networkTransform = projRoot.AddComponent<NetworkTransform>();
networkTransform.SyncPositionX = true;
networkTransform.SyncPositionY = true;
networkTransform.UseQuaternionSynchronization = false;
projRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
projRoot.AddComponent<CircleCollider2D>().radius = 0.15f;
```

### Root Cause B: Projectile direction never set
`ProjectileNetwork.Initialize(direction)` is never called after spawn, so `_direction` stays `Vector3.zero` and the projectile doesn't move.

### Fix B: `Assets/Scripts/Player/ProjectileNetwork.cs` (lines 19-23)

**Before:**
```csharp
public void Initialize(Vector3 direction)
{
    _direction = direction;
    Destroy(gameObject, _lifetime);
}
```

**After:**
```csharp
private GameObject _owner;

public void Initialize(Vector3 direction, GameObject owner = null)
{
    _direction = direction.normalized;
    _owner = owner;
    Destroy(gameObject, _lifetime);
}
```

### Fix C: `Assets/Scripts/Networking/Player/PlayerCombatNetwork.cs` (lines 83-103)

**Before:**
```csharp
[ServerRpc]
private void SpawnProjectileServerRpc(Vector3 position, Vector3 direction)
{
    if (_projectilePrefab != null)
    {
        var projectile = GameObject.Instantiate(_projectilePrefab, position, Quaternion.identity);
        var networkObject = projectile.GetComponent<NetworkObject>();
        if (networkObject == null)
            networkObject = projectile.AddComponent<NetworkObject>();
        networkObject.Spawn();
    }
}
```

**After:**
```csharp
[ServerRpc]
private void SpawnProjectileServerRpc(Vector3 position, Vector3 direction)
{
    if (_projectilePrefab != null)
    {
        var projectile = GameObject.Instantiate(_projectilePrefab, position, Quaternion.identity);
        var networkObject = projectile.GetComponent<NetworkObject>();
        if (networkObject == null)
            networkObject = projectile.AddComponent<NetworkObject>();
        networkObject.Spawn(true);
        var projectileNetwork = projectile.GetComponent<ProjectileNetwork>();
        projectileNetwork?.Initialize(direction, gameObject);
    }
}
```

Key changes:
- `Spawn()` -> `Spawn(true)` for networked spawn
- Call `Initialize(direction, gameObject)` after spawn

---

## Summary of All Changes

| File | Change |
|------|--------|
| `InteractionNetwork.cs` | Call `player.InteractServerRpc()` instead of direct GameManager call |
| `PlayerNetwork.cs` | `InteractServerRpc` forwards `senderClientId` to `GameManager.RequestReadyServerRpc` |
| `GameManager.cs` | Use `serverRpcParams.Receive.SenderClientId` instead of `callerId` param |
| `SetupGame.cs` (RegisterAllPrefabs) | Add player prefab to `DefaultNetworkPrefabsList` |
| `SetupGame.cs` (CreateProjectilePrefab) | Add `NetworkTransform` with X/Y sync |
| `ProjectileNetwork.cs` | `Initialize` takes direction + owner; direction is normalized |
| `PlayerCombatNetwork.cs` | `SpawnProjectileServerRpc` calls `Spawn(true)` + `Initialize(direction, gameObject)` |

## Post-Fix Steps
1. Re-run `Frantic -> Setup All` (recreates projectile prefab with NetworkTransform, registers player in prefabs list)
2. Re-run `Frantic -> Configure Hub Scene` (ensures player prefab is registered)
3. Test as Host: player should be visible, projectiles should fly
4. Test as Client (2nd window): player should be visible, projectiles should sync
