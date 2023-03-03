using UnityEngine;

public class ValidateChipPacks : MonoBehaviour
{
    private void OnEnable()
    {
        foreach(var pack in GetComponentsInChildren<ChipPack>(true))
        {
            if (pack.limitToPlayer == Player.None || (int)pack.limitToPlayer == (int)CharacterController.Player.characterName + 1)
                pack.gameObject.SetActive(true);
            else
                pack.gameObject.SetActive(false);
        }
    }
}
