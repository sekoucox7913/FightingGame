using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ManaDebug : MonoBehaviour
{
    public TextMeshProUGUI m_Text;

    private string m_BotStringBlock;
    private string m_PlayerStringBlock;

    private void Start()
    {
        m_Text = GetComponent<TextMeshProUGUI>();
    }

    public void UpdateBotMana(int amount)
    {
        m_BotStringBlock = "Bot : " + amount.ToString();
        UpdateText();
    }

    public void UpdatePlayerMana(int amount)
    {
        m_PlayerStringBlock = "Player : " + amount.ToString();
        UpdateText();
    }

    private void UpdateText()
    {
        m_Text.text = m_PlayerStringBlock + " / " + m_BotStringBlock;
    }
}
