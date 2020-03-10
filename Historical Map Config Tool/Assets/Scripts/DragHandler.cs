using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour
{
    /// <summary>
    /// Bool for tracking whether overlay is being dragged
    /// </summary>
    private bool IsDragging = false;

    /// <summary>
    /// Offset for click handler to make sure the overlay doesn't "snap" to mouse click,
    /// but instead moves in relation to the mouse movement
    /// </summary>
    private Vector3 Offset;

    public GameObject OverlayMap;
    public float PanSpeed = 0.5f;
    public float ZoomSpeed = 0.5f;
    public float MinZoom = 15f;
    public float MaxZoom = 90f;
    private Vector3 lastPos;

    /// <summary>
    /// UPDATE method for Unity to call every frame
    /// </summary>
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            // mouse button is clicked, check what was clicked
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!IsPanning() && Physics.Raycast(ray, out hit))
            {
                // user clicked on something specifically
                if (hit.collider.gameObject.tag == "Overlay" && !EventSystem.current.IsPointerOverGameObject())
                {
                    // user clicked the overlay, get location of the raycast hit
                    Vector3 hitPos = hit.point;
                    if (!IsDragging)
                    {
                        // get offset (it's a new drag so we need to reset offset to the relation
                        // between mouse click and current overlay position... avoids "snapping")
                        Offset = hit.collider.gameObject.transform.position - hitPos;
                        IsDragging = true;
                    }
                    // move overlay plane to mouse click location (taking offset into account)
                    hit.collider.gameObject.transform.position = new Vector3(hitPos.x + Offset.x, 0.1f, hitPos.z + Offset.z);
                }
            }
        }
        else
        {
            // no longer dragging... this will signal the Offset to be set again on next drag
            IsDragging = false;

            // check for zooming
            CheckForZoom();
        }
    }


    /// <summary>
    /// This method zooms based on mouse wheel input
    /// </summary>
    void CheckForZoom()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            // we're moving the historical overlay map
            float xLen = OverlayMap.transform.localScale.x;
            float zLen = OverlayMap.transform.localScale.z;
            float change = Input.GetAxis("Mouse ScrollWheel");
            if (change > 0)
            {
                xLen = xLen * 1.01f;
                zLen = zLen * 1.01f;
                OverlayMap.transform.localScale = new Vector3(xLen, 1f, zLen);
            }
            else if (change < 0)
            {
                xLen = xLen * 0.99f;
                zLen = zLen * 0.99f;
                OverlayMap.transform.localScale = new Vector3(xLen, 1f, zLen);
            }
        }
        else
        {
            float change = Input.GetAxis("Mouse ScrollWheel");
            Camera.main.orthographicSize += (change / -1);
        }
    }


    /// <summary>
    /// This method pans the camera along x and y based on mouse movement
    /// </summary>
    bool IsPanning()
    {
        bool isPanning = false;
        if (Input.GetKey("space"))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // this only happens once for every mouse click, not every frame that mouse is held
                lastPos = Input.mousePosition;
            }

            // apply movement from mouse to the camera for panning
            Vector3 camPos = Camera.main.transform.position;
            var delta = Input.mousePosition - lastPos;
            delta = delta * -1f;
            Camera.main.transform.Translate(delta.x * PanSpeed, delta.y * PanSpeed, 0);
            lastPos = Input.mousePosition;
            isPanning = true;
        }
        return isPanning;
    }
}
