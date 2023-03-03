using UnityEngine;
using TMPro;
public class HealthDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;
    [SerializeField]
    private Character character;


    private void OnDisable()
    {
        Debug.Log("health disabled");
    }

    void Update()
    {
        //11/15/2022
        if(character.health < 0)
        {
            character.health = 0;
        }
        //
        text.text = character.health + "HP";
    } 
}