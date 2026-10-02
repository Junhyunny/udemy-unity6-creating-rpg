using UnityEngine;

public class Example : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("Awake was called");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start was called");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Update was called");
    }

    // TODO: [todos/chapter-007/fixedupdate-and-fixed-timestep.md](../../todos/chapter-007/fixedupdate-and-fixed-timestep.md)
    private void FixedUpdate()
    {
        Debug.Log("FixedUpdate was called");
    }
}
