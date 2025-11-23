using UnityEngine;

public class Ball : MonoBehaviour
{

    [Header("References")] 
    public GameManager gameManager;
    public Transform hoop;
    
    [Header("Offsets")]
    public float ballOffsetX = 0.5f;
    public float ballOffsetY = 0f;
    
    [Header("Player References")]
    public bool inPossession = false;
    public int playerIndex = -9;
    public Transform playerTransform;
    
    [Header("Shooting Settings")]
    [Tooltip("Launch angle in degrees")]
    public float launchAngle = 45f;
    [Tooltip("Time of flight in seconds (as alternative)")]
    public float timeOfFlight = 2f;
    [Tooltip("Use time of flight instead of launch angle")]
    public bool useTimeOfFlight = false;
    
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (inPossession && playerTransform != null)
        {
            transform.position = playerTransform.position + new Vector3(ballOffsetX, ballOffsetY, 0f);
            
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
        }
        else if (rb != null)
        {
            rb.gravityScale = 1f;
        }
    }

    public bool InPossession(int index)
    {
        return index == playerIndex;
    }
    
    public void Possess(int index)
    {
        inPossession = true;
        playerIndex = index;
        playerTransform = GameManager.instance.GetPlayerByIndex(index).transform;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void Shoot()
    {
        //Transform hoop = getHoopLocation(playerIndex); //implement later

        if (hoop == null)
        {
            Debug.LogWarning("Hoop reference is not set! Cannot shoot.");
            return;
        }
        
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 1f;
        
        inPossession = false;
        playerIndex = -1;
        playerTransform = null;
        
        // calc target position
        Vector2 startPos = transform.position;
        Vector2 targetPos = hoop.position;
        Vector2 displacement = targetPos - startPos;
        
        // get gravity value (Unity's Physics2D gravity)
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        
        Vector2 initialVelocity;
        
        if (useTimeOfFlight)
        {
            // calc initial velocity based on time of flight
            float t = timeOfFlight;
            float vx = displacement.x / t;
            float vy = (displacement.y + 0.5f * gravity * t * t) / t;
            initialVelocity = new Vector2(vx, vy);
        }
        else
        {
            // calc initial velocity based on launch angle
            float angleRad = launchAngle * Mathf.Deg2Rad;
            float distance = displacement.magnitude;
            
            // calc initial velocity magnitude using projectile motion equations
            float x = displacement.x;
            float y = displacement.y;
            
            float cosAngle = Mathf.Cos(angleRad);
            float tanAngle = Mathf.Tan(angleRad);
            
            // avoid div by zero or negative values
            float denominator = 2f * cosAngle * cosAngle * (x * tanAngle - y);
            
            if (denominator <= 0f || Mathf.Abs(cosAngle) < 0.01f)
            {
                // fallback
                float v0 = Mathf.Sqrt(gravity * distance / Mathf.Sin(2f * angleRad));
                initialVelocity = new Vector2(v0 * Mathf.Cos(angleRad), v0 * Mathf.Sin(angleRad));
            }
            else
            {
                float v0Squared = gravity * x * x / denominator;
                if (v0Squared < 0f)
                {
                    // if calculation fails, use fallback
                    float v0 = Mathf.Sqrt(gravity * distance / Mathf.Sin(2f * angleRad));
                    initialVelocity = new Vector2(v0 * Mathf.Cos(angleRad), v0 * Mathf.Sin(angleRad));
                }
                else
                {
                    float v0 = Mathf.Sqrt(v0Squared);
                    initialVelocity = new Vector2(v0 * Mathf.Cos(angleRad), v0 * Mathf.Sin(angleRad));
                }
            }
        }
        
        rb.linearVelocity = initialVelocity;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameManager != null && gameManager.activeHoop != null)
        {
            if (other == gameManager.activeHoop)
            {
                gameManager.Scored();
            }
        }
    }
}
