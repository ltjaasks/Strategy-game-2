using UnityEngine;


/// <summary>
/// Creates a slow, random rotation for the fog to make game visually more pleasing
/// </summary>
public class FogAnimator : MonoBehaviour
{
    private float _rotationSpeed;
    private float _rotationDirection;


    /// <summary>
    /// Sets a random rotation speed and direction for the fog on start.
    /// </summary>
    void Start()
    {
        SetSeed();

        // Random slow speed
        _rotationSpeed = Random.Range(2f, 16f);   // degrees per second

        // Random direction (clockwise or counterclockwise)
        _rotationDirection = Random.value > 0.5f ? 1f : -1f;
    }

    /// <summary>
    /// Update is called once per frame. Rotates the fog slowly.
    /// </summary>
    void Update()
    {
        transform.Rotate(
            0f,
            _rotationSpeed * _rotationDirection * Time.deltaTime,
            0f
        );
    }

    /// <summary>
    /// Sets a random starting rotation for visual variety.
    /// </summary>
    private void SetSeed()
    {
        float randomAngle = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0f, randomAngle, 0f);
    }
}
