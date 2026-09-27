using UnityEngine;

public class Damaged_Example : MonoBehaviour
{

    private SpriteRenderer sr;
    public float redColorDuration = 0.5f;
    public float timer;
    public float currentTimeInGame;
    public float lastTimeWasDamaged;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Debug.Log(Time.deltaTime);
        // timer = timer - Time.deltaTime;
        // timer -= Time.deltaTime;
        // if(timer < 0 && sr.color != Color.white)
        // {
        //     TurnWhite();
        // }
        currentTimeInGame = Time.time;
        if (currentTimeInGame > lastTimeWasDamaged + redColorDuration)
        {
            Debug.Log("meet");
            if (sr.color != Color.white)
            {
                Debug.Log("meet");
                TurnWhite();
            }
        }
    }

    [ContextMenu("Update Timer")]
    private void updateTimer()
    {
        timer = redColorDuration;
    }

    public void TakeDamage()
    {
        Debug.Log("Enermy took damage");
        sr.color = Color.red;
        // timer = redColorDuration;
        lastTimeWasDamaged = Time.time;
        // TODO: [todos/chapter-007/invoke-and-delayed-calls.md](../../todos/chapter-007/invoke-and-delayed-calls.md)
        // Invoke("TurnWhite", redColorDuration);
        // Invoke(nameof(TurnWhite), redColorDuration);
    }

    private void TurnWhite()
    {
        sr.color = Color.white;
    }
}
