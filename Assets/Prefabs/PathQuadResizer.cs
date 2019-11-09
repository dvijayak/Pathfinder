using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathQuadResizer : MonoBehaviour
{
    [SerializeField]
    Pathfinder pathfinder; // to obtain the cell size

    // Start is called before the first frame update
    void Start()
    {
        pathfinder = pathfinder ?? GameObject.FindGameObjectWithTag("World").GetComponent<Pathfinder>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(pathfinder.cellSize, pathfinder.cellSize, pathfinder.cellSize);
    }
}
