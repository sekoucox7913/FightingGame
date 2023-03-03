using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    [SerializeField]
    private Tile[,] board = new Tile[8,3]; // initially [8,4]

    [SerializeField]
    private List<Tile> Row0;
    [SerializeField]
    private List<Tile> Row1;
    [SerializeField]
    private List<Tile> Row2;
    [SerializeField]
    private List<Tile> Row3;

    private static TileController instance;
    public static Vector2Int BoardSize => new Vector2Int(instance.board.GetLength(0), instance.board.GetLength(1));

    public static Tile[,] Board { get { return instance.board; } }

    private void Awake()
    {
        instance = this;
        for(int x = 0; x < board.GetLength(0); x++)
        {
            board[x, 0] = Row0[x];
            board[x, 1] = Row1[x];
            board[x, 2] = Row2[x];
            // Todo: removing one line on the board.
            //board[x, 3] = Row3[x];
        }
    }

    public static Tile Info(Vector2Int pos) => instance.board[pos.x, pos.y];

    private static List<Vector2Int> GetAdjacentTilesCoordinate(Vector2Int fromTile)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();

        // x : columns, y : rows
        tiles.Add(new Vector2Int(fromTile.x - 1, fromTile.y - 1));
        tiles.Add(new Vector2Int(fromTile.x, fromTile.y - 1));
        tiles.Add(new Vector2Int(fromTile.x + 1, fromTile.y - 1));
        tiles.Add(new Vector2Int(fromTile.x + 1, fromTile.y));
        tiles.Add(new Vector2Int(fromTile.x + 1, fromTile.y + 1));
        tiles.Add(new Vector2Int(fromTile.x, fromTile.y + 1));
        tiles.Add(new Vector2Int(fromTile.x - 1, fromTile.y + 1));
        tiles.Add(new Vector2Int(fromTile.x - 1, fromTile.y));

        return tiles;
    }

    private static bool IsValidCoordinates(Vector2Int coord)
    {
        int rowCount = instance.board.GetLength(1);
        int columnCount = instance.board.GetLength(0);

        Debug.Log("Is Valid Coordinate : " + coord);

        // x : columns, y : rows
        if ( 0 > coord.y || coord.y >= rowCount || 0 > coord.x || coord.x >= columnCount)
        {
            return false;
        }

        return true;
    }

    public static List<Vector2Int> GetAdjacentTiles(Vector2Int tileIndex)
    {
        List<Vector2Int> results = new List<Vector2Int>();

        var tiles = GetAdjacentTilesCoordinate(tileIndex);
        if(tiles != null)
        {
            foreach(var t in tiles)
            {
                if(IsValidCoordinates(t))
                {
                    results.Add(t);
                }
            }
        }
        else
        {
            Debug.Log("Tile Controller: list seem to be null");
        }

        return results;
    }

    public static List<Vector2Int> GetAllLastRows(int direction)
    {
        List<Vector2Int> results = new List<Vector2Int>();

        var columnIndex = direction > 0 ? instance.board.GetLength(0) - 1 : 0;

        for(int i = 0; i < instance.board.GetLength(1); i++)
        {
            results.Add(new Vector2Int(columnIndex, i));
        }

        return results;
    }
}
