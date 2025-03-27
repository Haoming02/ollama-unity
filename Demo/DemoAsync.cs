using UnityEngine;

/// <summary>Showcase the non-blocking on main thread</summary>
public class DemoAsync : MonoBehaviour
{
    private const int speed = 30;
    void Awake() { Application.targetFrameRate = 60; }
    void Update() { transform.Rotate(0.0f, 0.0f, speed * Time.deltaTime); }
}
