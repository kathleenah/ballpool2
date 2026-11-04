using UnityEngine;

public class GoldBall : Ball
{
    public override void OnCollected()
    {
        FindAnyObjectByType<AudioManager>().PlayCollect();
        FindAnyObjectByType<GameManager>().AddScore(poin);
        FindAnyObjectByType<GameManager>().BallExitedPool();
    }
}
