using UnityEngine;

public class GoldBall : Ball
{
    public override void OnCollected()
    {
        FindAnyObjectByType<GameManager>().AddScore(poin);
        FindAnyObjectByType<GameManager>().BallExitedPool();
    }
}
