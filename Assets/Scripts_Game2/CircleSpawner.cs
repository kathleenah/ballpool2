using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CircleSpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private int spawnCount = 5;        // how many circles to spawn
    [SerializeField] private Vector2 spawnArea = new Vector2(16f, 9f); // width x height, centered on this object
    [SerializeField] private bool spawnOnStart = true;

    [Header("Avoid")]
    [Tooltip("Drag the DropZone object here so circles never spawn inside it.")]
    [SerializeField] private Collider2D noSpawnZone;

    [Tooltip("Drag your wall / obstacle Tilemap(s) here (the Tilemap object that draws the walls). " +
             "Circles never spawn on or next to a tile of these tilemaps.")]
    [SerializeField] private Tilemap[] blockedTilemaps;

    [Tooltip("Minimum free space around a circle. The circle prefab's own collider size is also taken into account.")]
    [SerializeField] private float checkRadius = 0.5f;

    [Header("Win")]
    [Tooltip("The GameManager of this scene. Its TriggerWin() is called when all circles are delivered. Found automatically if empty.")]
    [SerializeField] private GameManager gameManager;

    /// <summary>Circles that still need to be delivered.</summary>
    public int Remaining { get; private set; }

    private int totalSpawned;
    private bool won;
    private int rejectedByZone;
    private int rejectedByTile;
    private int rejectedBySolid;
    private GUIStyle topRightStyle;
    private GUIStyle centerStyle;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = FindAnyObjectByType<GameManager>();
    }

    private IEnumerator Start()
    {
        string prefabName = circlePrefab != null ? circlePrefab.name : "NONE";
        Debug.Log("CircleSpawner started. Prefab: " + prefabName + ", Count: " + spawnCount + ", SpawnOnStart: " + spawnOnStart);

        if (blockedTilemaps == null || blockedTilemaps.Length == 0)
        {
            Debug.LogWarning("CircleSpawner: Blocked Tilemaps is empty. Circles will only avoid solid colliders. " +
                             "Drag your wall Tilemap into Blocked Tilemaps for the best result.");
        }

        if (spawnOnStart)
        {
            // Tilemap colliders are only built after the first frame / physics step,
            // so wait a moment before checking for obstacles.
            yield return new WaitForFixedUpdate();
            yield return null;
            Physics2D.SyncTransforms();

            Spawn(spawnCount);
        }
    }

    // Right-click the CircleSpawner component title in the Inspector > "Spawn Now" (works while playing)
    [ContextMenu("Spawn Now")]
    private void SpawnNow()
    {
        Spawn(spawnCount);
    }

    /// <summary>Spawns 'count' circles at random free positions inside the spawn area.</summary>
    public void Spawn(int count)
    {
        if (circlePrefab == null)
        {
            Debug.LogWarning("CircleSpawner: no circle prefab assigned.");
            return;
        }

        rejectedByZone = 0;
        rejectedByTile = 0;
        rejectedBySolid = 0;
        float radius = GetCircleRadius();
        int spawned = 0;

        for (int i = 0; i < count; i++)
        {
            if (!TryGetSpawnPosition(radius, out Vector2 position))
            {
                Debug.LogWarning("CircleSpawner: could not find a free spot. Spawned " + spawned + " of " + count +
                                 ". Try a bigger Spawn Area, a smaller Check Radius, or fewer blocked tilemaps.");
                break;
            }

            Instantiate(circlePrefab, position, Quaternion.identity);
            spawned++;
        }

        Remaining += spawned;
        totalSpawned += spawned;

        Debug.Log("CircleSpawner spawned " + spawned + " circles (check radius " + radius.ToString("0.00") + "). Rejected spots: " +
                  rejectedByTile + " on blocked tiles, " + rejectedBySolid + " touching solid colliders, " +
                  rejectedByZone + " inside the drop zone.");
    }

    // The radius used for all checks: Check Radius, or the circle prefab's real size if that is bigger.
    private float GetCircleRadius()
    {
        float radius = checkRadius;

        if (circlePrefab != null && circlePrefab.TryGetComponent(out CircleCollider2D col))
        {
            Vector3 s = circlePrefab.transform.lossyScale;
            float prefabRadius = col.radius * Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
            radius = Mathf.Max(radius, prefabRadius);
        }

        return radius;
    }

    // Picks a random position that is outside the drop zone, not on a blocked tile and not touching any solid collider.
    private bool TryGetSpawnPosition(float radius, out Vector2 position)
    {
        const float zoneMargin = 0.7f;

        for (int attempt = 0; attempt < 100; attempt++)
        {
            position = (Vector2)transform.position + new Vector2(
                Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f),
                Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f));

            // 1) Not inside the drop zone (plus a small margin)
            if (noSpawnZone != null)
            {
                Bounds b = noSpawnZone.bounds;
                bool insideZone =
                    position.x > b.min.x - zoneMargin && position.x < b.max.x + zoneMargin &&
                    position.y > b.min.y - zoneMargin && position.y < b.max.y + zoneMargin;

                if (insideZone)
                {
                    rejectedByZone++;
                    continue;
                }
            }

            // 2) Not on a tile of the blocked tilemaps (works without layers or physics)
            if (IsOnBlockedTile(position, radius))
            {
                rejectedByTile++;
                continue;
            }

            // 3) Not touching any solid collider on ANY layer (tilemap colliders, walls, the player...)
            if (IsTouchingSolid(position, radius))
            {
                rejectedBySolid++;
                continue;
            }

            return true;
        }

        position = Vector2.zero;
        return false;
    }

    // True if any tile of the blocked tilemaps lies within 'radius' of the position.
    private bool IsOnBlockedTile(Vector2 position, float radius)
    {
        if (blockedTilemaps == null) return false;

        foreach (Tilemap map in blockedTilemaps)
        {
            if (map == null) continue;

            Vector3Int min = map.WorldToCell(new Vector3(position.x - radius, position.y - radius, 0f));
            Vector3Int max = map.WorldToCell(new Vector3(position.x + radius, position.y + radius, 0f));

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    // Tiles painted in a tilemap sit at cell z = 0
                    if (map.HasTile(new Vector3Int(x, y, 0))) return true;
                }
            }
        }

        return false;
    }

    // True if the circle would touch a non-trigger collider (or a tilemap / composite collider) on any layer.
    private bool IsTouchingSolid(Vector2 position, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.isTrigger) return true;
            if (hit is TilemapCollider2D || hit is CompositeCollider2D) return true;
        }

        return false;
    }

    /// <summary>Called by the DropZone when circles are delivered.</summary>
    public void NotifyDelivered(int amount)
    {
        Remaining = Mathf.Max(0, Remaining - amount);

        // All circles delivered: trigger the win (only once)
        if (Remaining == 0 && totalSpawned > 0 && !won)
        {
            won = true;

            if (gameManager != null)
                gameManager.TriggerWin();
            else
                Debug.LogWarning("CircleSpawner: all circles delivered, but no GameManager was found in this scene.");
        }
    }

    private void OnGUI()
    {
        if (topRightStyle == null)
        {
            topRightStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.UpperRight };
            centerStyle = new GUIStyle(GUI.skin.label) { fontSize = 36, alignment = TextAnchor.MiddleCenter };
        }

        // Top-right: how many circles are left to deliver
        GUI.Label(new Rect(Screen.width - 260f, 10f, 250f, 40f), $"Remaining: {Remaining}", topRightStyle);

        // Win message
        if (totalSpawned > 0 && Remaining == 0 && gameManager == null)
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "All circles delivered!", centerStyle);
    }

    // Shows the spawn area in the Scene view when this object is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}