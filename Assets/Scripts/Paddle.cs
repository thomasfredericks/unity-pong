using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Paddle : MonoBehaviour
{
    protected Rigidbody2D rb;
    
    // Position tracking for velocity calculation
    private Queue<Vector3> positionHistory;
    private int sampleCount;
    protected Vector2 paddleVelocity;
    
    [Header("Dynamic Bounce Settings")]
    [Tooltip("Changes how the ball bounces off the paddle depending on where it hits the paddle.")]
    public bool useDynamicBounce = false;
    
    [Tooltip("Maximum angle change based on hit position (center = 0°, edges = ±maxBounceAngle).")]
    public float maxPositionBounceAngle = 45f;
    
    [Tooltip("How much the paddle's velocity affects the bounce angle. Higher values = more dramatic angle shifts.")]
    public float velocityInfluenceMultiplier = 1.5f;
    
    [Tooltip("Maximum contribution from paddle velocity to bounce angle (degrees).")]
    public float maxVelocityBounceAngle = 60f;
    
    [Header("Velocity Calculation Settings")]
    [Tooltip("Number of frames to average velocity over. Higher = smoother but more lag.")]
    public int velocityAverageFrames = 5;
    
    [Tooltip("Minimum movement threshold before considering paddle 'moving'.")]
    public float minimumVelocityThreshold = 0.1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Initialize history buffer
        positionHistory = new Queue<Vector3>(velocityAverageFrames);
        sampleCount = 0;
        paddleVelocity = Vector2.zero;
    }

    private void Start()
    {
        // Fill initial history with current position to avoid warmup lag
        for (int i = 0; i < velocityAverageFrames; i++)
        {
            positionHistory.Enqueue(transform.position);
        }
    }

    private void LateUpdate()
    {
        // Add current position to history (LateUpdate ensures this happens after all movement)
        positionHistory.Enqueue(transform.position);
        
        // Remove oldest position if we exceed buffer size
        if (positionHistory.Count > velocityAverageFrames)
        {
            positionHistory.Dequeue();
        }
        else
        {
            // Increment sample count until buffer is full
            sampleCount++;
        }
        
        // Calculate average velocity from the entire window
        if (positionHistory.Count >= 2)
        {
            var positions = positionHistory.ToArray();
            float totalTime = (positions.Length - 1) * Time.fixedDeltaTime;
            
            if (totalTime > 0)
            {
                Vector3 totalDelta = positions[positions.Length - 1] - positions[0];
                paddleVelocity = new Vector2(totalDelta.x / totalTime, totalDelta.y / totalTime);
            }
        }
    }

    public virtual void ResetPosition()
    {
        rb.velocity = Vector2.zero;
        rb.position = new Vector2(rb.position.x, 0f);
        
        // Clear and reset velocity tracking
        positionHistory.Clear();
        for (int i = 0; i < velocityAverageFrames; i++)
        {
            positionHistory.Enqueue(transform.position);
        }
        sampleCount = velocityAverageFrames;
        paddleVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (!useDynamicBounce) return;
        
        if (collision.gameObject.CompareTag("Ball"))
        {
            Rigidbody2D ball = collision.rigidbody;
            
            // Determine initial ball direction based on which side of the board it's on
            Vector2 ballDirection = ball.transform.position.x > 0 ? Vector2.left : Vector2.right;
            
            Vector2 baseBounceDirection = GetBounceDirection(ball, collision);
            
            // Apply paddle velocity influence if enabled
            if ( paddleVelocity.magnitude > minimumVelocityThreshold)
            {
                float velocityAngleContribution = CalculateVelocityAngleContribution();
                
                // Add velocity influence to the direction
                Quaternion velocityRotation = Quaternion.Euler(0, 0, velocityAngleContribution);
                baseBounceDirection = velocityRotation * baseBounceDirection;
            }
            
            // Apply the final bounce with slight speed increase
            float speedMultiplier = 1.1f;
            //float speedLimit = 15f; // Prevent ball from getting too fast
            
            // Vector2 finalVelocity = baseBounceDirection.normalized * Mathf.Min(
            //     ball.velocity.magnitude * speedMultiplier, 
            //     speedLimit
            // );
            Vector2 finalVelocity = baseBounceDirection.normalized * 
                ball.velocity.magnitude * speedMultiplier
            ;
            
            ball.velocity = finalVelocity;
        }
    }

    protected virtual Vector2 GetBounceDirection(Rigidbody2D ball, Collision2D collision)
    {
        // Get contact point for calculating hit position
        Collider2D paddle = collision.otherCollider;
        Vector2 contactPoint = collision.GetContact(0).point;
        Vector2 paddleCenter = transform.position;
        
        // Calculate normalized hit position (-1 to 1, where 0 is center)
        float hitPositionY = (contactPoint.y - paddleCenter.y) / (paddle.bounds.size.y / 2);
        hitPositionY = Mathf.Clamp(hitPositionY, -1f, 1f);
        
        // Convert hit position to angle
        float positionAngle = hitPositionY * maxPositionBounceAngle;
        
        // Create initial bounce direction with position-based angle
        Quaternion positionRotation = Quaternion.Euler(0, 0, positionAngle);
        return positionRotation * (ball.transform.position.x > 0 ? Vector2.left : Vector2.right);
    }

    private float CalculateVelocityAngleContribution()
    {
        // Normalize velocity to percentage of maximum expected paddle speed
        float maxExpectedPaddleSpeed = 20f; // Tune this based on your game
        float normalizedSpeed = Mathf.Clamp(paddleVelocity.y / maxExpectedPaddleSpeed, -1f, 1f);
        
        // Convert to angle with configurable influence
        return normalizedSpeed * maxVelocityBounceAngle * velocityInfluenceMultiplier;
    }
}