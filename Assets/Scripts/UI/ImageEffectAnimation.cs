using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageEffectAnimation : MonoBehaviour
{
    [SerializeField]
    private Sprite[] m_Sprites;

    [SerializeField]
    private float m_TimeInterval;

    private Image m_Image;
    private float m_ElpasedTime;
    private int m_Index;
    private bool m_Loop;

    private void Awake()
    {
        enabled = false;
        m_Image = GetComponent<Image>();
        m_Image.enabled = false;
    }

    void OnEnable()
    {
        m_Index = 0;
        m_ElpasedTime = 0f;
        m_Image.sprite = m_Sprites[m_Index];
        m_Image.enabled = true;
    }

    void Update()
    {
        m_ElpasedTime += Time.deltaTime;
        if(m_ElpasedTime > m_TimeInterval)
        {
            m_Index += 1;
            m_Image.sprite = m_Sprites[m_Index];
            m_ElpasedTime = 0f;

            if(m_Index == m_Sprites.Length - 1)
            {
                if (m_Loop)
                {
                    m_Index = 0;
                }
                else
                {
                    enabled = false;
                    m_Image.enabled = false;
                }
            }
        }
    }

    public void OnValueChanged(float value)
    {
        if(value > 0.99f && !isActiveAndEnabled)
        {
            m_Loop = true;
            enabled = true;
        }
        else if(value < 0.99 && isActiveAndEnabled)
        {
            m_Loop = false;
        }
    }
}
