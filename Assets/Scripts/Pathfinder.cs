using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public float cellSize = 1f;

    TileGraph graph;

    GameObject ground;

    // Start is called before the first frame update
    void Start()
    {
        ground = GameObject.Find("Ground");
        if (ground == null)
        {
            throw new UnityException("No Ground found");
        }

        MakeGraph();

        // TEST: Force the test path to be drawn
        graph.Start = Vector2Int.zero;
        graph.End = Vector2Int.one;
    }

    //
    // Summary:
    // Divide the x-z plane of the ground into a grid given the cell size
    void MakeGraph()
    {        
        Bounds bounds = ground.GetComponent<Renderer>().bounds;

        Vector3 xzBoundsCenter = bounds.center + new Vector3(0, bounds.extents.y, 0);
        Vector3 gridTopLeft = xzBoundsCenter + new Vector3(-bounds.extents.x, 0, bounds.extents.z);        

        Vector2 gridArea = new Vector2(bounds.size.x, bounds.size.z);
        int maxRowCount = (int)Mathf.Floor(gridArea.y / cellSize);
        int maxColCount = (int)Mathf.Floor(gridArea.x / cellSize);

        Tile[,] grid = new Tile[maxRowCount, maxColCount];

        Vector3 offsetToCenter = new Vector3(cellSize/2, 0, -cellSize/2);
        for (int r = 0; r < grid.GetLength(0); r++)
        {
            for (int c = 0; c < grid.GetLength(1); c++)
            {
                // World space point that corresponds to the center of the tile
                Vector3 worldPoint = gridTopLeft + new Vector3(c * cellSize, 0, -(r * cellSize)) + offsetToCenter;

                // Determine overlapping obstacles
                TileType tileType = TileType.Free;
                Collider[] colliders = Physics.OverlapSphere(worldPoint, cellSize);
                if (colliders != null && colliders.Length > 0)
                {
                    bool foundCollision = true;

                    // Check for collision with someone other than ourselves
                    Collider ourCollider = ground.GetComponent<Collider>();
                    if (ourCollider != null) {
                        foundCollision = false;
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            if (colliders[i] != ourCollider)
                            {
                                foundCollision = true;
                                break;
                            }
                        }
                    } // if we don't have a collider, we are definitely colliding with something else

                    if (foundCollision)
                    {
                        tileType = TileType.Obstacle;
                    }
                }

                // Make tile
                grid[r, c] = new Tile(new Vector2Int(r, c), tileType, worldPoint);
            }
        }

        graph = new TileGraph(grid);
    }

    // TODO: Debug only
    private void OnDrawGizmos()
    {
        if (graph != null)
            graph.DrawDebug();
    }

    // Update is called once per frame
    void Update()
    {
        if (graph.Start.HasValue && graph.End.HasValue)
        {            
            List<Tile> path = graph.ComputePath(graph.Start.GetValueOrDefault(), graph.End.GetValueOrDefault());
            
            // TODO: Draw better path, not just a stupid debug line            
            for (int i = 1; i < path.Count; i++)
            {
                Debug.DrawLine(path[i - 1].WorldRegion, path[i].WorldRegion, Color.blue, 300f);
            }

            graph.Start = null;
            graph.End = null;
        }
    }
}
