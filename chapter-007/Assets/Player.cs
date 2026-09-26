using UnityEngine;

public class Player : MonoBehaviour
{

    public Rigidbody2D rb;

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector2(Input.GetAxis("Horizontal"), rb.linearVelocityY);
    }
}
