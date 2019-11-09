using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

public class Tile
{
    public Vector2 Index { get; }
    public TileType Type { get; }
    // TODO: World region thingy...for now, just store the point
    public Vector3 WorldRegion { get; }

    public Tile(Vector2 index, TileType type, Vector3 region)
    {
        Index = index;
        Type = type;
        WorldRegion = region;
    }
}

public class TileGraph
{
    Tile startTile;
    Vector2? _Start;
    public Vector2? Start
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
    Vector2? _End;
    public Vector2? End
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

    public Tile Tile(Vector2 index)
    {
        return grid[(int)index.x, (int)index.y];
    }

    public Tile Tile(int row, int col)
    {
        return grid[row, col];
    }

    List<Tile> Neighbors(Vector2 index)
    {
        // TODO
        return new List<Tile>();
    }

    List<Tile> UnobstructedNeighbors(Vector2 index)
    {
        List<Tile> neighbors = Neighbors(index);
        neighbors.RemoveAll(tile => tile.Type == TileType.Obstacle);
        return neighbors;
    }

    float CostOfMovement(Vector2 from, Vector2 to)
    {
        return movementCostComputer.CostOfMovement(Tile(from).Type, Tile(to).Type);
    }

    float HeuristicToEnd(Vector2 from)
    {
        if (!End.HasValue)
        {
            throw new UnityException("End tile is expected to be set");
        }

        // Manhattan distance
        return Mathf.Abs(End.GetValueOrDefault().x - from.x) + Mathf.Abs(End.GetValueOrDefault().y - from.y);
    }

    public List<Tile> ComputePath(Vector2 from, Vector2 to)
    {
        List<Tile> path = new List<Tile>();

        // TODO

        // TEST
        int cellCount = Random.Range(5, 10);
        for (int i = 0; i < cellCount; i++)
        {
           int r = (int)Random.Range(0, RowCount);
           int c = (int)Random.Range(0, ColCount);
           path.Add(Tile(r, c));
        }

        return path;
    }
}
