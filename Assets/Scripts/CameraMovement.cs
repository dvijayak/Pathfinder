using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Implements the good ol' mouse-drag camera movement 
// TODO: Camera rotation!
public class CameraMovement : MonoBehaviour
{
    public string inputButton = "Fire3";

    private Camera thisCamera;

    Vector3 mouseDragStartPositionInWorld;
    bool isDragging = false;

    // Start is called before the first frame update
    void Start()
    {
        thisCamera = GetComponent<Camera>();
    }

    Vector3 MousePositionInWorld()
    {
        return thisCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    // Update is called once per frame
    void Update()
    {        
        if (Input.GetButton(inputButton))
        {
            if (!isDragging) {                
                isDragging = true;

                mouseDragStartPositionInWorld = MousePositionInWorld();
            }
            else
            {
                // We are dragging now, so apply displacement from the drag start point
                if (mouseDragStartPositionInWorld == null) {
                    throw new UnityException("Logical error: `prevMousePositionInWorld` should have been set by now");
                }                

                Vector3 currentMousePositionInWorld = MousePositionInWorld();
                Vector3 displacement = currentMousePositionInWorld - mouseDragStartPositionInWorld;
                Vector3 cameraDisplacement = -1 * new Vector3(displacement.x, 0, displacement.z); // pan effect is achieved by moving camera in reverse direction of displacement
                transform.position += cameraDisplacement; // since the camera position moves by the absolute displacement magnitude, no need to keep updating the mouse drag start position
            }
        }
        else
        {
            // Reset upon release button
            isDragging = false;
        }
    }
}
