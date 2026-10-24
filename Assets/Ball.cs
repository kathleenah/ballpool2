using UnityEngine;

public abstract class Ball : MonoBehaviour
{
    private bool hasExited = false;
    public int poin;

    public abstract void OnCollected();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("OutArea") && !hasExited)
        {
            hasExited = true;
            OnCollected();
        }
    }
}