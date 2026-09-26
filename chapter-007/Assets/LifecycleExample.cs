using UnityEngine;

public class LifecycleExample : MonoBehaviour
{

    public Rigidbody2D rb;
    public string playerName = "Bob"; // This box(variable) called "playerName" holds the text "Bob".
    public int age = 25;
    public int characterLevel = 80;
    public float moveSpeed = 5.0f;
    public bool gameOver = true;
    public int currentHp = 100;

    // TODO: [todos/chapter-007/update-and-frame-rate.md](../../todos/chapter-007/update-and-frame-rate.md)
    void Update()
    {
        rb.linearVelocity = new Vector2(Input.GetAxis("Horizontal"), rb.linearVelocityY);
    }

    private void Start()
    {
        playerName = "Bob the hero";
        Debug.Log("Start");
        GetPlayerInfo();
        TakeDamage(25);
    }

    private void Awake()
    {
        Debug.Log("Awake");
        GetPlayerInfo();
    }

    private void GetPlayerInfo()
    {
        Debug.Log("GetPlayerInfo");
        Debug.Log("Player name is: " + playerName);
        Debug.Log("Player age is: " + age);
        Debug.Log("Player level is: " + characterLevel);
    }

    private void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log("Player took " + damage + " damage. Current HP: " + currentHp);
    }
}
