using UnityEngine;

public class Tornado : MonoBehaviour
{
    [Header("Base Settings")]
    public float minSize = 1f;
    public float maxSize = 4f;

    [Header("Multipliers")]
    public float forceMultiplier = 70f;
    public float durationMultiplier = 3f;

    [Header("Visual Elements")]
    public Transform innerCircleTransform; // First ring (Inner)
    public Transform outerCircleTransform; // Second ring (Outer)

    [Header("Animation Settings")]
    public float shrinkSpeed = 5f;        // Base speed of the suction animation
    public float resetThreshold = 0.1f;   // Scale at which rings reset to the start

    [Header("Audio Settings")]
    public AudioSource tornadoAudioSource; //Sound
    public float pitchMin = 0.8f;
    public float pitchMax = 1.2f;

    private float finalSize;
    private float pullForce;
    private float maxRadius;
    private float initialScale;

    void Start()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();

        // 1. Randomize overall tornado size
        finalSize = Random.Range(minSize, maxSize);

        // 2. Calculate logic and physics stats
        maxRadius = finalSize * 2f;
        pullForce = finalSize * forceMultiplier;
        float lifeDuration = finalSize * durationMultiplier;

        // 3. Set Physics Radius
        if (col != null) col.radius = maxRadius;

        // 4. Calculate initial scale for the rings (Diameter = Radius * 2)
        initialScale = maxRadius * 2f;

        // Setup starting positions for the two rings to be different
        if (innerCircleTransform != null)
            innerCircleTransform.localScale = new Vector3(initialScale, initialScale, 1);

        if (outerCircleTransform != null)
            // Start the outer ring at half-way to create a staggered effect immediately
            outerCircleTransform.localScale = new Vector3(initialScale * 0.5f, initialScale * 0.5f, 1);

        if (tornadoAudioSource != null)
        {
            // Big -> big pitch, small -> low pitch
            float sizeFactor = (finalSize - minSize) / (maxSize - minSize);
            tornadoAudioSource.pitch = Mathf.Lerp(pitchMax, pitchMin, sizeFactor);

            // Adjust the sound as its size
            tornadoAudioSource.volume = Mathf.Lerp(0.5f, 1f, sizeFactor);
        }
        // 5. Destruction timer
        Destroy(gameObject, lifeDuration);
    }

    void Update()
    {
        // Animate both rings
        AnimateRing(innerCircleTransform);
        AnimateRing(outerCircleTransform);
    }

    void AnimateRing(Transform ring)
    {
        if (ring == null) return;

        // Shrink the ring scale
        float newScale = ring.localScale.x - (shrinkSpeed * Time.deltaTime);

        // If the ring hits the center, reset it to the outer boundary
        if (newScale <= resetThreshold)
        {
            newScale = initialScale;
        }

        ring.localScale = new Vector3(newScale, newScale, 1);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D targetRb = other.attachedRigidbody;

        if (targetRb != null && (targetRb.CompareTag("Player") || targetRb.CompareTag("Obstacle")))
        {
            Vector2 direction = (Vector2)transform.position - (Vector2)other.transform.position;
            float distance = direction.magnitude;

            float forceFactor = 1 - (distance / maxRadius);
            float forceMagnitude = Mathf.Clamp01(forceFactor) * pullForce;

            targetRb.AddForce(direction.normalized * forceMagnitude, ForceMode2D.Force);
        }
    }
}