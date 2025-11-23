using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    
    [Header ("References")]
    public PlayerController[] players;
    public Ball ball;
    public BoxCollider2D leftHoop;
    public BoxCollider2D rightHoop;
    public BoxCollider2D activeHoop;
    
    [Header ("Game Variables")]
    public bool facingRight = true;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
    }

    // Update is called once per frame
    void Update()
    {
        if (facingRight)
        {
            activeHoop = rightHoop;
        }
        else
        {
            activeHoop = leftHoop;
        }
    }

    public void Scored()
    {
        ball.Possess(players[0].playerIndex);
    }

    public PlayerController GetPlayerByIndex(int index)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].playerIndex == index)
            {
                return players[i];
            }
        }
        return null;
    }
}
