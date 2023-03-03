using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private int currentDeckIndex = 0;
    public void StartGame()
    {
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        yield return new WaitForSeconds(3.0f);

        while (true)
        {
            //10/11/2022
            if (GameController.instance.bEnd)
            {
                yield break;
            }
            if (Global.IsPausePlay)
            {
                yield break;
            }

            if(Global.playmode > 0)
            {
                yield break;
            }

            //bot's health is below than 0, can't move/use any card
            if(GetComponent<Character>().health <= 0)
            {
                yield break;
            }

            //bot's status is stunned, can't move/use any card
            if (GetComponent<Character>().Stun > 0)
            {
                if (GetComponent<Character>().Mode == 1 || GetComponent<Character>().Mode == 2)
                {
                    yield return null;
                }
            }
            //

            yield return new WaitForSeconds(Random.Range(0.0f, 2.0f));

            List<Vector2Int> validTiles = new List<Vector2Int>();

            for (int x = 0; x < TileController.BoardSize.x; x++)
            {
                for (int y = 0; y < TileController.BoardSize.y; y++)
                {
                    var tile = TileController.Info(new Vector2Int(x, y));
                    if (tile.OwnerType == CharacterType.Enemy && tile.walkable == true && tile.boulder == null)
                        validTiles.Add(tile.coordinate);
                }
            }

            while (CharacterController.Enemy.CurrentStateType != StateType.Idle)
                yield return null;

            if (validTiles.Count > 0)
            {
                CharacterController.Enemy.SetState(new SetTarget(CharacterController.Enemy, validTiles[Random.Range(0, validTiles.Count)]));
                yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));
            }

            if (Random.Range(0, 2) == 0)
            {
                var chip = CharacterController.Enemy.deck[currentDeckIndex].chip;
                Debug.Log("Enemy will attack with a chip mana of : " + chip.Mana);

                if (CharacterController.Enemy.CanUseChip(chip.Mana))
                {
                    CharacterController.Enemy.SetState(new Attack(CharacterController.Enemy, CharacterController.Enemy.deck[currentDeckIndex].chip));
                    currentDeckIndex++;

                    //02/11/2022
                    CharacterController.Enemy.UpdateMana(-chip.Mana);
                    CharacterController.Player.UpdateMana(chip.Mana);
                    //

                    if (currentDeckIndex > CharacterController.Enemy.deck.Count - 1)
                        currentDeckIndex = 0;
                }
                else
                {
                    Debug.Log("#### Not enough mana to use this chip");
                }
            }
        }
    }
}