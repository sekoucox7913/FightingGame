using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Display player's mana value
public class ManaDisplay : MonoBehaviour
{
    [SerializeField]
    private Slider m_Mana;
    [SerializeField]
    private TMP_Text text;
    [SerializeField]
    private Character character;

    void Update()
    {
        m_Mana.value = character.mana;
        text.text = character.mana.ToString();
    }
}
