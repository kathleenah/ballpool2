using UnityEngine;

public class NormalBall : Ball
{
    public override void OnCollected()
    {
        FindAnyObjectByType<AudioManager>().PlayCollect();
        FindAnyObjectByType<GameManager>().AddScore(poin);
        FindAnyObjectByType<GameManager>().BallExitedPool();
    }
}
