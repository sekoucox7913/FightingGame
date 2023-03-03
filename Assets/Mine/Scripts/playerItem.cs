using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class playerItem : MonoBehaviour
{
    public List<Sprite> m_ChatacterList;
    public Image m_Character;
    public TextMeshProUGUI m_PlayerName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetData(int m_index, string m_name)
    {
        m_Character.sprite = m_ChatacterList[m_index];
        m_PlayerName.text = m_name;
    }
}
