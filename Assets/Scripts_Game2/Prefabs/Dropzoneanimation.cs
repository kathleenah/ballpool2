using UnityEngine;

[RequireComponent(typeof(DropZone))]
[RequireComponent(typeof(SpriteRenderer))]
public class DropZoneAnimation : MonoBehaviour
{
    [Header("Your sprite frames")]
    [Tooltip("Loops while the zone is waiting. One sprite is fine for a still image.")]
    [SerializeField] private Sprite[] idleFrames;

    [Tooltip("Loops while the player is inside carrying a circle. Optional: Idle Frames are used if empty.")]
    [SerializeField] private Sprite[] readyFrames;

    [Tooltip("Plays once when a circle is delivered, then returns to idle. Optional.")]
    [SerializeField] private Sprite[] deliverFrames;

    [SerializeField] private float framesPerSecond = 12f;

    [Header("Optional code effects (off by default)")]
    [SerializeField] private bool pulse = false;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.06f;     
    [SerializeField] private float readyPulseMultiplier = 2.5f; 

    [SerializeField] private bool tintWhenReady = false;
    [SerializeField] private Color readyColor = new Color(1f, 0.9f, 0.3f, 1f);
    [SerializeField] private float colorBlendSpeed = 8f;

    [SerializeField] private bool popOnDeliver = false;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float popScale = 1.25f;
    [SerializeField] private float popDuration = 0.35f;

    private DropZone dropZone;
    private SpriteRenderer sprite;
    private Sprite baseSprite;
    private Vector3 baseScale;
    private Color baseColor;

    private Sprite[] currentFrames;
    private float frameTimer;
    private float deliverTimer = -1f; 
    private float popTimer = -1f;     
    private float readyBlend;        
    private void Awake()
    {
        dropZone = GetComponent<DropZone>();
        sprite = GetComponent<SpriteRenderer>();
        baseSprite = sprite.sprite;
        baseScale = transform.localScale;
        baseColor = sprite.color;
    }

    private void OnEnable()
    {
        dropZone.Delivered += PlayDeliver;
    }

    private void OnDisable()
    {
        dropZone.Delivered -= PlayDeliver;
        transform.localScale = baseScale;
        sprite.color = baseColor;
        sprite.sprite = baseSprite;
    }

    private void PlayDeliver()
    {
        if (Has(deliverFrames)) deliverTimer = 0f;
        if (popOnDeliver) popTimer = 0f;
    }

    private static bool Has(Sprite[] frames)
    {
        return frames != null && frames.Length > 0;
    }

    private void Update()
    {
        bool ready = dropZone.IsReady;
        readyBlend = Mathf.MoveTowards(readyBlend, ready ? 1f : 0f, colorBlendSpeed * Time.deltaTime);

        UpdateFrames(ready);
        UpdateEffects();
    }

    private void UpdateFrames(bool ready)
    {
        if (deliverTimer >= 0f)
        {
            deliverTimer += Time.deltaTime * framesPerSecond;
            int index = (int)deliverTimer;

            if (index < deliverFrames.Length)
            {
                sprite.sprite = deliverFrames[index];
                return;
            }

            deliverTimer = -1f; 
            currentFrames = null;
        }

        Sprite[] frames = (ready && Has(readyFrames)) ? readyFrames : idleFrames;

        if (!Has(frames))
        {
            sprite.sprite = baseSprite; 
            return;
        }

        if (frames != currentFrames)
        {
            currentFrames = frames;
            frameTimer = 0f;
        }

        frameTimer += Time.deltaTime * framesPerSecond;
        frameTimer %= frames.Length;
        sprite.sprite = frames[(int)frameTimer];
    }

    private void UpdateEffects()
    {
        float scaleFactor = 1f;

        if (pulse)
        {
            float speed = pulseSpeed * Mathf.Lerp(1f, readyPulseMultiplier, readyBlend);
            float pulse01 = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;
            scaleFactor += pulse01 * pulseAmount;
        }

        Color color = tintWhenReady ? Color.Lerp(baseColor, readyColor, readyBlend) : baseColor;

        if (popTimer >= 0f)
        {
            popTimer += Time.deltaTime;
            float t = Mathf.Clamp01(popTimer / popDuration);
            float curve = Mathf.Sin(t * Mathf.PI);

            scaleFactor *= 1f + (popScale - 1f) * curve;
            color = Color.Lerp(color, flashColor, curve);

            if (t >= 1f) popTimer = -1f;
        }

        color.a = baseColor.a; 
        transform.localScale = baseScale * scaleFactor;
        sprite.color = color;
    }
}