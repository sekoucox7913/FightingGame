using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ChipAnimation : MonoBehaviour
{
    private const float MOVE_AMOUNT = 50f;
    private const float MOVE_SPEED = 240f;
    private const float FADE_SPEED = 3f;

    private RectTransform m_Rect;
    private Image m_Image;

    public void DoAnimation(UnityAction onFinish = null)
    {
        //transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = false;
        transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
        m_Rect = GetComponent<RectTransform>();
        m_Image = GetComponent<Image>();

        StartCoroutine(Animate());
        StartCoroutine(Fade(() => {
            Debug.Log("send event for graveyard from chip anim ended");
            onFinish?.Invoke();
        }));
    }

    public IEnumerator Animate()
    {
        var pos = m_Rect.anchoredPosition;
        var targetPosY = pos.y + MOVE_AMOUNT;

        while(pos.y < targetPosY)
        {
            pos.y += Time.deltaTime * MOVE_SPEED;
            m_Rect.anchoredPosition = pos;
            
            yield return null;
        }
    }

    private IEnumerator Fade(UnityAction onFinish)
    {
        var col = m_Image.color;

        while(col.a > 0f)
        {
            col.a -= Time.deltaTime * FADE_SPEED;
            m_Image.color = col;

            yield return null;
        }


        //var chip = GetComponent<Chip>();
        //chip.SetEmpty();
        transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;
        onFinish?.Invoke();
    }
}
