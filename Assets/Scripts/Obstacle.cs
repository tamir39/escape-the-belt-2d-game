using NUnit.Framework.Internal;
using UnityEngine;

public class Obstacle : MonoBehaviour
{

    public float minSize = 1f;
    public float maxSize = 3f;

    public float minSpeed = 5f;
    public float maxSpeed = 10f;

    public float maxSpinSpeed = 5f;
    public GameObject bounceEffect;
    private float currentSize;
    public AudioClip crashSound;
    Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        // Save the size of the obstacle
        currentSize = GetRandomSize();
        transform.localScale = new Vector3(currentSize, currentSize, 1);

        // The start force
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        rb.AddForce(randomDirection * GetRandomSpeed());
        rb.AddTorque(GetRandomTorque());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float currentSpeedSqr = rb.linearVelocity.sqrMagnitude;
        float maxSpeedSqr = maxSpeed * maxSpeed;
        float minThresholdSqr = maxSpeedSqr / 3f;

        // Too slow, need boosting
        if (currentSpeedSqr < minThresholdSqr)
        {
            float forceAmount = GetRandomSpeed() / currentSize;
            rb.AddForce(Random.insideUnitCircle.normalized * forceAmount);
        }

        // Limit the speed
        if (currentSpeedSqr > maxSpeedSqr)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (crashSound != null)
        {
            AudioSource.PlayClipAtPoint(crashSound, transform.position);
        }

        if (bounceEffect != null)
        {
            Vector2 contactPoint = collision.GetContact(0).point;
            GameObject effect = Instantiate(bounceEffect, contactPoint, Quaternion.identity);
            Destroy(effect, 1.0f);
        }
    }

    float GetRandomSize() => Random.Range(minSize, maxSize);
    float GetRandomSpeed() => Random.Range(minSpeed, maxSpeed);
    float GetRandomTorque() => Random.Range(-maxSpinSpeed, maxSpinSpeed);
}
