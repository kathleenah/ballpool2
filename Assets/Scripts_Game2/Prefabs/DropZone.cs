using System;
using UnityEngine;

// Put this on an object with a Collider2D set to Is Trigger.
// When the player stands in the zone while carrying a circle, a prompt appears.
// Pressing the key confirms the delivery and the circle disappears.
public class DropZone : MonoBehaviour
{
    [SerializeField] private CircleSpawner spawner;
    [SerializeField] private KeyCode deliverKey = KeyCode.E;

    private PlayerInventory playerInside;

    /// <summary>True while the player is in the zone carrying a circle (ready to deliver).</summary>
    public bool IsReady => playerInside != null && playerInside.Count > 0;

    /// <summary>Raised right after a circle is delivered (used by DropZoneAnimation).</summary>
    public event Action Delivered;

    private void Awake()
    {
        // If the Spawner slot is empty, find the spawner in the scene automatically.
        if (spawner == null)
        {
#if UNITY_2023_1_OR_NEWER
            spawner = FindFirstObjectByType<CircleSpawner>();
#else
            spawner = FindObjectOfType<CircleSpawner>();
#endif
        }

        if (spawner == null)
            Debug.LogWarning("DropZone: no CircleSpawner found. The Remaining counter will not update.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory != null) playerInside = inventory;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory != null && inventory == playerInside) playerInside = null;
    }

    private void Update()
    {
        if (!IsReady) return;
        if (!Input.GetKeyDown(deliverKey)) return;

        int delivered = playerInside.RemoveAll();
        if (spawner != null) spawner.NotifyDelivered(delivered);

        Delivered?.Invoke();
    }

    private void OnGUI()
    {
        if (!IsReady) return;

        GUI.Label(new Rect(Screen.width / 2f - 160f, Screen.height - 100f, 400f, 30f),
            $"Press {deliverKey} to deliver the circle");
    }
}