using UnityEngine;

public class BadBall : Ball
{
    public override void OnCollected()
    {
        FindAnyObjectByType<GameManager>().AddScore(poin);
        FindAnyObjectByType<GameManager>().BallExitedPool();
    }
}
