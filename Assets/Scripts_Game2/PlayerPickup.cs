using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerPickup : MonoBehaviour
{
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    private PlayerInventory inventory;
    private readonly List<CirclePickup> nearby = new List<CirclePickup>();

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out CirclePickup pickup) && !nearby.Contains(pickup))
            nearby.Add(pickup);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out CirclePickup pickup))
            nearby.Remove(pickup);
    }

    private void Update()
    {
        nearby.RemoveAll(p => p == null);

        if (nearby.Count == 0 || !Input.GetKeyDown(pickupKey)) return;

        CirclePickup target = nearby[0];

        if (inventory.TryAdd())
        {
            nearby.Remove(target);
            Destroy(target.gameObject);
        }
    }

    private void OnGUI()
    {
        if (nearby.Count == 0) return;

        string msg = inventory.IsFull
            ? "You are already carrying a circle. Deliver it first!"
            : $"Press {pickupKey} to pick up";

        GUI.Label(new Rect(Screen.width / 2f - 160f, Screen.height - 60f, 400f, 30f), msg);
    }
}