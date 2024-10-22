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
    private float worldWidth = 200f;
    private float worldHeight = 200f;

    private float movementBoundaryTop;
    private float movementBoundaryBottom;
    private float movementBoundaryRight;
    private float movementBoundaryLeft;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        targetZoom = mainCamera.orthographicSize;
        UpdateMovementBoundaries();
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

        //Clamp the zoom value
        targetZoom = Mathf.Clamp(mainCamera.orthographicSize - scrollValue * zoomSensitivity, minZoom, maxZoom);

        //Update boundaries and reposition the camera if necessary
        UpdateMovementBoundaries();
        RepositionCameraWithinBoundaries();
    }

    private void LateUpdate()
    {
        UpdateMovementBoundaries();

        if (isDragging)
        {
            difference = GetMousePosition - transform.position;
            Vector3 newPosition = origin - difference;

            newPosition.x = Mathf.Clamp(newPosition.x, movementBoundaryLeft, movementBoundaryRight);
            newPosition.y = Mathf.Clamp(newPosition.y, movementBoundaryBottom, movementBoundaryTop);

            transform.position = newPosition;
        }

        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
        RepositionCameraWithinBoundaries();
    }

    //Dynamically calculate the boundaries
    private void UpdateMovementBoundaries()
    {
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = mainCamera.aspect * cameraHeight;

        //Check if the camera is showing more than the world width/height
        if (cameraWidth * 2 > worldWidth)
        {
            movementBoundaryLeft = movementBoundaryRight = 0;
        }
        else
        {
            movementBoundaryLeft = -worldWidth / 2 + cameraWidth;
            movementBoundaryRight = worldWidth / 2 - cameraWidth;
        }

        if (cameraHeight * 2 > worldHeight)
        {
            movementBoundaryBottom = movementBoundaryTop = 0;
        }
        else
        {
            movementBoundaryBottom = -worldHeight / 2 + cameraHeight;
            movementBoundaryTop = worldHeight / 2 - cameraHeight;
        }
    }

    //Ensure the camera stays within the boundaries
    private void RepositionCameraWithinBoundaries()
    {
        Vector3 clampedPosition = transform.position;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, movementBoundaryLeft, movementBoundaryRight);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, movementBoundaryBottom, movementBoundaryTop);

        transform.position = clampedPosition;
    }

    private Vector3 GetMousePosition => mainCamera.ScreenToWorldPoint((Vector3)Mouse.current.position.ReadValue());
}
