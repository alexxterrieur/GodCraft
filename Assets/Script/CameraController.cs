using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging;
    private Vector3 origin;
    private Vector3 difference;

    [Header("Zoom Parameters")]
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;
    [SerializeField] private float zoomSensitivity;
    [SerializeField] private float zoomSmoothTime;
    private float targetZoom;
    private float zoomVelocity;


    [Header("Drag Parameters")]
    [SerializeField] private float movementBoundaryTop;
    [SerializeField] private float movementBoundaryBottom;
    [SerializeField] private float movementBoundaryRight;
    [SerializeField] private float movementBoundaryLeft;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        targetZoom = mainCamera.orthographicSize;
    }

    public void OnDrag(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            origin = GetMousePosition;
        }

        isDragging = ctx.started || ctx.performed;
    }

    public void OnZoom(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            return;
        }

        float scrollValue = ctx.ReadValue<Vector2>().y;

        targetZoom = Mathf.Clamp(mainCamera.orthographicSize - scrollValue * zoomSensitivity, minZoom, maxZoom);
    }

    private void LateUpdate()
    {
        if (isDragging)
        {
            difference = GetMousePosition - transform.position;
            Vector3 newPosition = origin - difference;

            newPosition.x = Mathf.Clamp(newPosition.x, movementBoundaryLeft, movementBoundaryRight);
            newPosition.y = Mathf.Clamp(newPosition.y, movementBoundaryBottom, movementBoundaryTop);

            transform.position = newPosition;
        }

        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
    }

    private Vector3 GetMousePosition => mainCamera.ScreenToWorldPoint((Vector3)Mouse.current.position.ReadValue());
}
