using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public float cellSize = 1f;

    public LocationSelector locationSelector;

    // Used for rendering the path
    public GameObject pathStartPrefab;
    public GameObject pathMidPrefab;
    public GameObject pathEndPrefab;

    public string pathObjectTag = "PathObject";

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

        // Plug-in user interactions
        locationSelector.OnStartLocationSelected += HandleStartLocationSelected;
        locationSelector.OnEndLocationSelected += HandleEndLocationSelected;

        // Initialize some starting values
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
        
        int maxRowCount = (int)Mathf.Floor(bounds.size.z / cellSize);
        int maxColCount = (int)Mathf.Floor(bounds.size.x / cellSize);

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

    Vector2Int? TransformWorldToGridPoint(Vector3 point)
    {
        Bounds bounds = ground.GetComponent<Renderer>().bounds;
        Vector3 xzBoundsCenter = bounds.center + new Vector3(0, bounds.extents.y, 0);
        Vector3 gridTopLeft = xzBoundsCenter + new Vector3(-bounds.extents.x, 0, bounds.extents.z);

        // Translate with respect to new origin
        Vector3 locationWithRespectToBounds = point - gridTopLeft;
        
        // Scale and flip in order to obtain point in grid space with rows increasing downward, columns increasing rightward
        Vector2 locationInGridSpace = new Vector2(
            -locationWithRespectToBounds.z * graph.RowCount / bounds.size.z,
            locationWithRespectToBounds.x * graph.ColCount / bounds.size.x
            );

        // Truncate to a discrete grid coordinate
        Vector2Int index = new Vector2Int((int)Mathf.Floor(locationInGridSpace.x), (int)Mathf.Floor(locationInGridSpace.y));

        return  index.x >= 0 && index.x < graph.RowCount &&
                index.y >= 0 && index.y < graph.ColCount
                ? new Vector2Int?(index) : null;
    }

    void HandleStartLocationSelected(Vector3 location)
    {
        Vector2Int? index = TransformWorldToGridPoint(location);
        if (index.HasValue)
        {
            graph.Start = index.GetValueOrDefault();
        }
    }

    void HandleEndLocationSelected(Vector3 location)
    {
        Vector2Int? index = TransformWorldToGridPoint(location);
        if (index.HasValue)
        {
            graph.End = index.GetValueOrDefault();
        }
    }

    // TODO: Debug only
    private void OnDrawGizmos()
    {
        if (graph != null)
            graph.DrawDebug();
    }

    GameObject ConstructPathObject(GameObject prefab, Tile tile)
    {
        GameObject pathObject = Instantiate(prefab);
        pathObject.transform.position = new Vector3(
            tile.WorldRegion.x,
            pathObject.transform.position.y, // retain the prefab's Y component - we don't want to make any assumption on the height of the model
            tile.WorldRegion.z
            );
        pathObject.transform.SetParent(transform);

        return pathObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (graph.Start.HasValue && graph.End.HasValue)
        {
            // TODO: implement caching system...only if the newly generated path has changed should we clear and redraw
            
            // Clear existing path
            foreach (GameObject pathObject in GameObject.FindGameObjectsWithTag(pathObjectTag))
            {
                Destroy(pathObject);
            }

            // Compute new path with latest parameters and draw it
            List<Tile> path = graph.ComputePath(graph.Start.GetValueOrDefault(), graph.End.GetValueOrDefault());
            if (path.Count > 1)
            {
                ConstructPathObject(pathStartPrefab, path[0]);

                for (int i = 1; i < path.Count-1; i++)
                {
                    ConstructPathObject(pathMidPrefab, path[i]);
                }

                ConstructPathObject(pathEndPrefab, path[path.Count - 1]);
            }

            graph.Start = null;
        }
    }
}
