using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum TileType
{
    Free = 0,
    Obstacle,
    Start,
    End
}

public interface IMovementCostComputer
{
    // Cost of moving from tile of type `from` to a neighboring tile of type `to`
    float CostOfMovement(TileType from, TileType to);
}

public class UnitarianMovementCost : IMovementCostComputer
{
    static UnitarianMovementCost _Instance;
    public static UnitarianMovementCost Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new UnitarianMovementCost();
            }
            return _Instance;
        }
    }

    private UnitarianMovementCost() {}

    public float CostOfMovement(TileType from, TileType to)
    {
        return 1f;
    }
}

public enum HeuristicToEndStrategy
{
    ManhattanDistance,
    EuclideanDistance
}

public class Tile
{
    public Vector2Int Index { get; }
    public TileType Type { get; }
    // TODO: World region thingy...for now, just store the point
    public Vector3 WorldRegion { get; }

    public Tile(Vector2Int index, TileType type, Vector3 region)
    {
        Index = index;
        Type = type;
        WorldRegion = region;
    }
}

public class TileGraph
{
    Tile startTile;
    Vector2Int? _Start;
    public Vector2Int? Start
    {
        get
        {
            return _Start;
        }

        set
        {
            _Start = value;
            if (_Start.HasValue)
            {
                startTile = grid[(int)_Start.GetValueOrDefault().x, (int)_Start.GetValueOrDefault().y];
            }
            else
            {
                startTile = null;
            }
        }
    }

    Tile endTile;
    Vector2Int? _End;
    public Vector2Int? End
    {
        get
        {
            return _End;
        }

        set
        {
            _End = value;
            if (_End.HasValue)
            {
                endTile = grid[(int)_End.GetValueOrDefault().x, (int)_End.GetValueOrDefault().y];
            }            
            else
            {
                endTile = null;
            }
        }
    }

    Tile[,] grid;

    public int RowCount { get { return grid.GetLength(0); } }
    public int ColCount { get { return grid.GetLength(1); } }
    public int TileCount { get { return grid.Length; } }

    IMovementCostComputer movementCostComputer;

    public HeuristicToEndStrategy EndGoalStrategy { get; set; } = HeuristicToEndStrategy.EuclideanDistance;

    Path lastComputedPath;

    public TileGraph(Tile[,] grid)
    {
        this.grid = grid;
        this.movementCostComputer = UnitarianMovementCost.Instance;
    }

    public TileGraph(Tile[,] grid, IMovementCostComputer movementCostComputer)
    {
        this.grid = grid;
        this.movementCostComputer = movementCostComputer;
    }

    public Tile Tile(Vector2Int index)
    {
        return grid[(int)index.x, (int)index.y];
    }

    public Tile Tile(int row, int col)
    {
        return grid[row, col];
    }

    List<Tile> Neighbors(Vector2Int index)
    {
        int r = index.x;
        int c = index.y;

        List<Tile> neighbors = new List<Tile>(4); // only possible neighbors are N, S, E, W

        Vector2Int[] candidates = new Vector2Int[]
        {
            new Vector2Int(r - 1, c), // north
            new Vector2Int(r, c + 1), // east
            new Vector2Int(r + 1, c), // south
            new Vector2Int(r, c - 1), // west
        };

        // Remember: x is row index, y is col index
        foreach (Vector2Int candidate in candidates)
        {
            if (
                candidate.x >= 0 && candidate.x < grid.GetLength(0) &&
                candidate.y >= 0 && candidate.y < grid.GetLength(1)
            ) // bounds check
            {
                neighbors.Add(Tile(candidate));
            }
        }

        return neighbors;
    }

    List<Tile> UnobstructedNeighbors(Vector2Int index)
    {
        List<Tile> neighbors = Neighbors(index);
        neighbors.RemoveAll(tile => tile.Type == TileType.Obstacle);
        return neighbors;
    }

    float CostOfMovement(Vector2Int from, Vector2Int to)
    {
        return movementCostComputer.CostOfMovement(Tile(from).Type, Tile(to).Type);
    }

    float HeuristicToEnd(Vector2Int from)
    {
        if (!End.HasValue)
        {
            throw new UnityException("End tile is expected to be set");
        }

        switch(EndGoalStrategy)
        {
            case HeuristicToEndStrategy.ManhattanDistance:
                return Mathf.Abs(End.GetValueOrDefault().x - from.x) + Mathf.Abs(End.GetValueOrDefault().y - from.y);
            case HeuristicToEndStrategy.EuclideanDistance:
                return Vector2Int.Distance(from, End.GetValueOrDefault());
            default:
                throw new UnityException($"No valid end goal heuristic strategy was specified: {EndGoalStrategy}");
        }
    }

    public Path ComputePath(Vector2Int from, Vector2Int to)
    {
        List<Tile> bestPath = new List<Tile>(); // if this never gets filled, it means there is no path between `from` and `to`
        List<Tile> pathOfExploration = new List<Tile>();

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        SortedDictionary<float, Queue<Vector2Int>> frontier = new SortedDictionary<float, Queue<Vector2Int>>(); // a priority queue of tile node indices sorted by their cumulative cost starting from the `from` node

        Hashtable cameFrom = new Hashtable(); // dest => source

        // Init
        frontier.Add(0, new Queue<Vector2Int>(new Vector2Int[] {from}));
        visited.Add(from);
        cameFrom.Add(from, null);

        // Go find your path!
        while (frontier.Count > 0)
        {
            float costSoFar = frontier.Keys.First();
            Queue<Vector2Int> queue = frontier[costSoFar];
            Vector2Int index = queue.Peek();

            pathOfExploration.Add(Tile(index));

            if (index == to)
            {
                break;
            }

            // Pop from the priority queue
            queue.Dequeue();
            if (queue.Count == 0)
            {
                frontier.Remove(costSoFar);
            }

            // Explore neighbors
            foreach (Tile neighbor in UnobstructedNeighbors(index))
            {
                if (!visited.Contains(neighbor.Index))
                {
                    float cumulativeCost = costSoFar +
                        CostOfMovement(index, neighbor.Index) + // minimize movement cost
                        HeuristicToEnd(neighbor.Index) // minimize heuristic to goal
                        ;

                    Queue<Vector2Int> existingQueueForCost;
                    if (frontier.TryGetValue(cumulativeCost, out existingQueueForCost))
                    {
                        existingQueueForCost.Enqueue(neighbor.Index);
                    }
                    else
                    {
                        frontier.Add(cumulativeCost, new Queue<Vector2Int>(new Vector2Int[] {neighbor.Index}));
                    }

                    visited.Add(neighbor.Index);

                    // Keep track of where we came from - this is how we trace back the "best" chosen path
                    cameFrom.Add(neighbor.Index, Tile(index));
                }
            }
        }

        // Retrace chosen path by following the breadcrumbs
        if (cameFrom.ContainsKey(to)) // we should have reached the destination in order to compute a path
        {
            Vector2Int index = to; // init
            while (true)
            {
                bestPath.Add(Tile(index));

                Tile next = cameFrom[index] as Tile;
                if (next == null)
                {
                    break;
                }
                index = next.Index;
            }
            
            bestPath.Reverse();
        }


        Path computedPath = new Path(Tile(from), Tile(to), bestPath, pathOfExploration);
        
        // TODO: How to cache?

        return computedPath;
    }

    public class Path
    {
        Tile _start;
        public Tile Start { get { return _start; } }

        Tile _end;
        public Tile End { get { return _end; } }
        
        // Note: the first and last nodes of the best path don't HAVE to 
        // correspond to the Start and End nodes. This can happen when
        // Start or End are unreachable.
        List<Tile> _bestPath;
        public List<Tile> BestPath { get { return _bestPath; } }

        // The "journey" the algorithm, in order of node encounter
        List<Tile> _exploredSpace;
        public List<Tile> ExploredSpace { get { return _exploredSpace; } }

        public bool Exists { get { return _bestPath.Count > 0; } }

        public Path(Tile start, Tile end, List<Tile> bestPath, List<Tile> journey)
        {
            _start = start;
            _end = end;
            _bestPath = bestPath;
            _exploredSpace = journey;
        }

        // Concatenate hashCode of all tile indices
        public override int GetHashCode()
        {
            StringBuilder hashCodeStr = new StringBuilder($"{_start.Index},{_end.Index}", 100); // initialize with high enough capacity
            foreach (Tile tile in _bestPath)
            {
                hashCodeStr.Append($",{tile.Index}");
            }
            return hashCodeStr.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Path other = obj as Path;
            if (other == null)
            {
                return false;
            }

            if (_start.Index == other._start.Index && _end.Index == other._end.Index)
            {
                // Ensure computed best path is the same
                if (_bestPath.Count == other._bestPath.Count)
                {
                    for (int i = 0; i < _bestPath.Count; i++)
                    {
                        if (_bestPath[i].Index != other._bestPath[i].Index)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }

    public void DrawDebug() {
        float cellSize = 0.1f; //Mathf.Abs(grid[0,0].WorldRegion.x - grid[1,0].WorldRegion.x);
        foreach (Tile tile in grid)
        {
            Gizmos.color = tile.Type == TileType.Obstacle ? Color.red : Color.yellow;
            Gizmos.DrawSphere(tile.WorldRegion, cellSize);
        }
    }
}
