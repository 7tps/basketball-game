using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    private Rigidbody2D rb;
    public Slider shotMeter;
    public RectTransform shotMeterTransform;
    public Camera mainCamera; 
    public Ball ball;

    [Header("Index")] 
    public int playerIndex = -1;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float velocityDecayRate = 10f; //suggestion: double move speed

    [Header("Shot Meter Settings")]
    public float xOffset = 75f;
    public float chargeSpeed = 1f; //suggestion: half of move speed
    private bool isShotMeterActive = false;
    private float timeToDisplay = .5f;
    public float meterTimer = 0f;
    public bool frozen = false;

    [Header("Yellow and Green Windows")] 
    public RectTransform background;
    public RectTransform yellow;
    public RectTransform green;
    [Range(0f, 1f)]
    public float yellowHeightMultiplier = 0.4f; // height of yellow window as fraction of background
    [Range(0f, 1f)]
    public float greenHeightMultiplier = 0.15f; // height of green window as fraction of background
    [Range(0f, 1f)]
    public float windowOffset = 0f; // offset of yellow and green windows as a percentage of height, starts from halfway up the slider
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Initialize slider to 0
        if (shotMeter != null)
        {
            shotMeter.value = 0f;
        }
        
        SetShotWindows();
    }
    
    void FixedUpdate()
    {
        if (playerIndex < 0) {return;} //ensure index is initialized
        
        //movement
        float horizontal = Input.GetAxis("Horizontal"); 
        float vertical = Input.GetAxis("Vertical");     
        
        if (rb != null)
        {
            if (!frozen)
            {
                Vector2 movement = new Vector2(horizontal, vertical) * moveSpeed;
                rb.linearVelocity = new Vector2(movement.x, movement.y);
            }
            else
            {
                // Decay velocity to 0 when shot meter is active
                Vector2 currentVelocity = rb.linearVelocity;
                Vector2 decayedVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, velocityDecayRate * Time.fixedDeltaTime);
                rb.linearVelocity = decayedVelocity;
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (playerIndex < 0) {return;} //ensure index is initialized
        
        //shot meter position
        if (shotMeterTransform != null && mainCamera != null)
        {
            // Convert world position to screen position
            Vector3 worldPos = new Vector3(rb.position.x, rb.position.y, 0f);
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            
            shotMeterTransform.position = screenPos + new Vector3(xOffset, 0f, 0f);
        }
        
        //shot meter logic
        if (shotMeter != null)
        {
            if (ball.InPossession(playerIndex))
            {
                if (!frozen)
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        shotMeter.gameObject.SetActive(true);
                        SetShotWindows();
                        isShotMeterActive = true;
                        frozen = true;
                        shotMeter.value += chargeSpeed * Time.deltaTime;
                    
                        shotMeter.value = Mathf.Clamp(shotMeter.value, shotMeter.minValue, shotMeter.maxValue);
                    }
                    else if (Input.GetKeyUp(KeyCode.Space))
                    {
                        meterTimer = timeToDisplay;
                        ball.Shoot();
                        frozen = false;
                    }
                    else
                    {
                        shotMeter.value = shotMeter.minValue;
                        shotMeter.gameObject.SetActive(false);
                        isShotMeterActive = false;
                        ResetShotWindows();
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.Space))
                    {
                        shotMeter.gameObject.SetActive(true);
                        shotMeter.value += chargeSpeed * Time.deltaTime;
                        shotMeter.value = Mathf.Clamp(shotMeter.value, shotMeter.minValue, shotMeter.maxValue);
                    }
                    else if (Input.GetKeyUp(KeyCode.Space))
                    {
                        meterTimer = timeToDisplay;
                        ball.Shoot();
                        frozen = false;
                    }
                    else
                    {
                        meterTimer -= Time.deltaTime;
                        if (meterTimer <= 0)
                        {
                            shotMeter.value = shotMeter.minValue;
                            shotMeter.gameObject.SetActive(false);
                            isShotMeterActive = false;
                            ResetShotWindows();
                            frozen = false;
                        }
                    }
                }
            }
            else if (isShotMeterActive)
            {
                meterTimer -= Time.deltaTime;
                if (meterTimer <= 0)
                {
                    shotMeter.value = shotMeter.minValue;
                    shotMeter.gameObject.SetActive(false);
                    isShotMeterActive = false;
                    ResetShotWindows();
                    frozen = false;
                }
            }
        }
    }

    void ResetShotWindows()
    {
        yellow.localPosition = Vector3.zero;
        green.localPosition = Vector3.zero;
    }
    
    void SetShotWindows()
    {
        //relate to position on the court
        //relate to stats
        
        if (background != null && !isShotMeterActive)
        {
            float backgroundHeight = background.rect.height;
            float offsetHeight = windowOffset/2 * background.rect.height;
            
            // Set yellow window height
            if (yellow != null)
            {
                Vector2 yellowSize = yellow.sizeDelta;
                yellowSize.y = backgroundHeight * yellowHeightMultiplier;
                yellow.sizeDelta = yellowSize;
                yellow.localPosition = new Vector3(yellow.localPosition.x, yellow.localPosition.y + offsetHeight, yellow.localPosition.z);
            }
            
            // Set green window height
            if (green != null)
            {
                Vector2 greenSize = green.sizeDelta;
                greenSize.y = backgroundHeight * greenHeightMultiplier;
                green.sizeDelta = greenSize;
                green.localPosition = new Vector3(green.localPosition.x, green.localPosition.y + offsetHeight, green.localPosition.z);
            }
        }
    }
}
