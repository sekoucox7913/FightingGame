using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Display message on screen
//(win, loss, ...)
public class MessageManager : MonoBehaviour
{
    public TextMeshProUGUI m_MsgTxt;
    public void ShowMessage(string m_message, float m_delay)
    {
        m_MsgTxt.text = m_message;
        LeanTween.scaleX(gameObject, 1, 0.3f);
        StartCoroutine(HideMessage(m_delay - 0.3f));
    }

    IEnumerator HideMessage(float m_delay)
    {
        yield return new WaitForSeconds(m_delay);
        LeanTween.scaleX(gameObject, 0, 0.3f);
    }
}
