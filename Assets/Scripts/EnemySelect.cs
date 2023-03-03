using UnityEngine;
using UnityEngine.UI;

public class EnemySelect : MonoBehaviour
{
    public void Select(Dropdown dropdown)
    {
        CharacterController.Enemy.characterName = (CharacterName)dropdown.value;
        GameController.SelectEnemySprite(dropdown.value);
    }
}
