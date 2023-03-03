using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Display player's mana value
public class ShowMana : MonoBehaviour
{
    [SerializeField]
    private Slider m_Mana;

    private TextMeshProUGUI m_value;

    // Start is called before the first frame update
    void Start()
    {
        m_value = GetComponent<TextMeshProUGUI>();    
    }

    // Update is called once per frame
    void Update()
    {
        if(m_value != null)
        {
            m_value.text = m_Mana.value.ToString();
        }
    }
}
