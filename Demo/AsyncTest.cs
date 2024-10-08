using UnityEngine;

/// <summary>Showcase the non-blocking on main thread</summary>
public class AsyncTest : MonoBehaviour
{
    private Vector3 original;
    private Vector3 movement;

    void Awake()
    {
        Application.targetFrameRate = 60;
        original = transform.position;
        movement = original;
    }

    void Update()
    {
        movement.x = original.x + Mathf.Cos(Time.time * 5.0f) * 32.0f;
        transform.position = movement;
    }
}
