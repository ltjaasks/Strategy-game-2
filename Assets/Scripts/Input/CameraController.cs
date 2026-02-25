using System;
using UnityEngine;


/// <summary>
/// Camera controller class to handle player input for moving and zooming the camera.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 600f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 100f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleZoom();
    }


    /// <summary>
    /// Movement of camera
    /// </summary>
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        if (h == 0 && v == 0)
            return;

        // Move relative to camera orientation, but only on XZ plane
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * v + right * h;

        transform.position += direction * moveSpeed * Time.deltaTime;
    }


    /// <summary>
    /// Camera zoom.
    /// </summary>
    private void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0)
            return;

        float newFov = cam.fieldOfView - scroll * zoomSpeed * Time.deltaTime;
        cam.fieldOfView = Mathf.Clamp(newFov, minZoom, maxZoom);
    }
}
