using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationSelector : MonoBehaviour
{
    public Camera thisCamera;

    public string leftInputButton = "Fire1";
    public string rightInputButton = "Fire2";

    public delegate void LocationSelectedHandler(Vector3 location);
    public event LocationSelectedHandler OnStartLocationSelected;
    public event LocationSelectedHandler OnEndLocationSelected;

    // Start is called before the first frame update
    void Start()
    {
        thisCamera = thisCamera != null ? thisCamera : Camera.main;
    }

    Vector3? ComputeSelectionPointOnWorld()
    {
        return ComputeSelectionPointOnWorld(Input.mousePosition);
    }

    Vector3? ComputeSelectionPointOnWorld(Vector3 point)
    {        
        RaycastHit hit;
        if (Physics.Raycast(thisCamera.ScreenPointToRay(point), out hit))
        {
            return hit.point;
        }

        return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonUp(leftInputButton))
        {
            Vector3? point = ComputeSelectionPointOnWorld();
            if (point.HasValue)
            {
                OnStartLocationSelected(point.GetValueOrDefault());
            }
        }
        else if (Input.GetButtonUp(rightInputButton))
        {
            Vector3? point = ComputeSelectionPointOnWorld();
            if (point.HasValue)
            {
                OnEndLocationSelected(point.GetValueOrDefault());
            }
        }
    }
}
