using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SetTarget : State
{
    List<Vector2Int> path = null;
    Vector2Int target;
    public SetTarget(Character character, Vector2Int target) : base(character)
    {
        this.target = target;
        stateType = StateType.SetTarget;
    }

    public override void OnStateEnter()
    {
        if (character.waiting)
        {
            character.SetState(new Idle(character));
            return;
        }

        var currentStateType = character.CurrentStateType;

        if (CheckValidTarget(target))
        {
            if (Global.MyCT == character.characterType)
            {
                PunManager._Instance.SendTarget(target);
            }

            (bool foundPath, List<Vector2Int> Path) = RigidMovement(target, character.currentPos.x, character.currentPos.y);
            path = Path.OrderByDescending(x => Vector2Int.Distance(x, target)).Distinct().ToList();
        }
    }

    public override void Tick()
    {
        if (path != null && path.Count > 0)
            character.SetState(new Move(character, path));
        else
            character.SetState(new Idle(character));
    }

    private bool CheckValidTarget(Vector2Int target) => (TileController.Info(target) != null && TileController.Info(target).OwnerType == character.characterType);

    private (bool, List<Vector2Int>) RigidMovement(Vector2Int target, int startX, int startY)
    {
        (bool validPath, List<Vector2Int> path) pathData;
        pathData.validPath = true;
        pathData.path = new List<Vector2Int>();

        if (target.x > startX)
        {
            for (int x = startX; x <= target.x; x++)
            {
                if (CheckValidTarget(new Vector2Int(x, startY)))
                    pathData.path.Add(new Vector2Int(x, startY));
                else
                    return TryMoveToTile(target, pathData.path, startX, startY);
            }
        }
        else
        {
            for (int x = startX; x >= target.x; x--)
            {
                if (CheckValidTarget(new Vector2Int(x, startY)))
                    pathData.path.Add(new Vector2Int(x, startY));
                else
                    return TryMoveToTile(target, pathData.path, startX, startY);
            }
        }

        if (target.y > startY)
        {
            for (int y = startY; y <= target.y; y++)
            {
                if (CheckValidTarget(new Vector2Int(target.x, y)))
                    pathData.path.Add(new Vector2Int(target.x, y));
                else
                    return TryMoveToTile(target, pathData.path, startX, startY);
            }
        }
        else
        {
            for (int y = startY; y >= target.y; y--)
            {
                if (CheckValidTarget(new Vector2Int(target.x, y)))
                    pathData.path.Add(new Vector2Int(target.x, y));
                else
                    return TryMoveToTile(target, pathData.path, startX, startY);
            }
        }

        return pathData;
    }
    private (bool, List<Vector2Int>) TryMoveToTile(Vector2Int target, List<Vector2Int> currentPath, int startX, int startY)
    {
        // If tile does not match player status
        if (!CheckValidTarget(target))
            return (false, new List<Vector2Int>());

        // If tile is not walkable
        if (!TileController.Info(target).walkable)
            return (false, new List<Vector2Int>());

        // Default path values, currentPath is used in recursion
        (bool foundPath, List<Vector2Int> Path) data = (false, currentPath);
        Vector2Int startPosition;

        // -1 is used as a flag for starting at the mover's current position
        if (startY == -1)
            startPosition = character.currentPos;
        else // Otherwise the recursive data is used
            startPosition = new Vector2Int(startX, startY);

        // Add current tile as first step in path
        if (data.Path.Count == 0)
            data.Path.Add(startPosition);

        // If mover is already on the destination
        if (character.currentPos == target)
            return (true, new List<Vector2Int>());

        var neighbours = GetNeighbours(startPosition);

        // No neighbours = invalid path
        if (neighbours.Count == 0)
            return (false, data.Path);

        // Sort by distance to target as a heuristic
        neighbours = neighbours.OrderBy(x => Vector2.Distance(x.coordinate, target)).ToList();

        foreach (var neighbour in neighbours)
        {
            // If we've found the target
            if (neighbour.coordinate == target)
            {
                // Add it to the list and return it
                data.Path.Add(neighbour.coordinate);
                data.foundPath = true;
                return data;
            }
            else // Otherwise call TryMoveToTile recursively
            {
                if (currentPath.Contains(neighbour.coordinate))
                    continue;
                currentPath.Add(neighbour.coordinate);
                data = TryMoveToTile(target, currentPath, neighbour.coordinate.x, neighbour.coordinate.y);

                if (data.foundPath)
                    return data;
            }
        }

        return data;
    }

    private List<Tile> GetNeighbours(Vector2Int tilePosition)
    {
        var neighbours = new List<Tile>();
        // Check N,S,E,W
        if (tilePosition.x > 0)
            neighbours.Add(TileController.Info(tilePosition - Vector2Int.right));
        if (tilePosition.x < TileController.BoardSize.x - 1)
            neighbours.Add(TileController.Info(tilePosition + Vector2Int.right));

        if (tilePosition.y > 0)
            neighbours.Add(TileController.Info(tilePosition - Vector2Int.up));
        if (tilePosition.y < TileController.BoardSize.y - 1)
            neighbours.Add(TileController.Info(tilePosition + Vector2Int.up));

        //  Remove invalid neighbours
        neighbours.RemoveAll(x => !x.walkable || !CheckValidTarget(x.coordinate));

        return neighbours;
    }
}
