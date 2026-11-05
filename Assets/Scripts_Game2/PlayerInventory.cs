using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int maxCapacity = 1;

    public int Count { get; private set; }
    public bool IsFull => Count >= maxCapacity;

    public bool TryAdd(int amount = 1)
    {
        if (Count + amount > maxCapacity) return false;
        Count += amount;
        return true;
    }

    public int RemoveAll()
    {
        int removed = Count;
        Count = 0;
        return removed;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 220, 25), $"Carrying: {Count}/{maxCapacity}");
    }
}