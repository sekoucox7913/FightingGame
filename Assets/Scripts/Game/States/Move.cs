using System.Collections.Generic;
using UnityEngine;

public class Move : State
{
    private List<Vector2Int> path = null;

    public Move (Character character, List<Vector2Int> path) : base(character)
    {
        this.path = path;
        stateType = StateType.Move;
    }

    public override void OnStateEnter()
    {
        if (Global.MyCT == character.characterType)
        {
            PunManager._Instance.SendMove(path);
        }

        if (path.Count > 2)
            character.PlayAnim("Flip");
        else
            character.PlayAnim("Dash");
    }

    public override void Tick()
    {
        if (character.waiting)
            return;

        LerpToNextTile();
    }

    private void LerpToNextTile()
    {
        if (!CheckValidTarget(path[0]))
        {
            character.SetState(new SetTarget(character, path[path.Count - 1]));
            return;
        }

        var nextTile = TileController.Info(path[0]);

        if (Vector3.Distance(character.transform.position,nextTile.transform.position) < 0.1f)
        {
            character.currentPos = nextTile.coordinate;
            path.RemoveAt(0);

            if(path.Count > 0 && !character.cancelMove)
                nextTile = TileController.Info(path[0]);
            else
            {
                character.SetState(new Idle(character));
                return;
            }
        }

        character.transform.position = Vector3.MoveTowards(character.transform.position, nextTile.transform.position, character.moveSpeed);
    }

    private bool CheckValidTarget(Vector2Int target)
    {
        return TileController.Info(target) != null
            && TileController.Info(target).OwnerType == character.characterType
            && TileController.Info(target).walkable == true;
    }
}
