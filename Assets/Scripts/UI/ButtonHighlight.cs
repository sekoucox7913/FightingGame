using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHighlight : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private Image m_Image;
    private float m_Speed = 5f;

    private const float MAX_ALPHA = 0.4f;
    private const float MIN_ALPHA = 0f;

    private Coroutine m_Coroutine;

    void Start()
    {
        if(m_Image == null)
        {
            Debug.Log("ButtonHighlight : " + "Missing Image");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(m_Coroutine != null)
        {
            StopCoroutine(m_Coroutine);
        }

        m_Coroutine = StartCoroutine(HighlightIn());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (m_Coroutine != null)
        {
            StopCoroutine(m_Coroutine);
        }

        m_Coroutine = StartCoroutine(HighlightOut());
    }

    private IEnumerator HighlightIn()
    {
        var col = m_Image.color;

        while(col.a < MAX_ALPHA)
        {
            col.a += Time.deltaTime * m_Speed;
            m_Image.color = col;
            yield return null;
        }
    }

    private IEnumerator HighlightOut()
    {
        var col = m_Image.color;

        while(col.a > MIN_ALPHA)
        {
            col.a -= Time.deltaTime * m_Speed;
            m_Image.color = col;
            yield return null;
        }

        col.a = MIN_ALPHA;
        m_Image.color = col;
    }
}
