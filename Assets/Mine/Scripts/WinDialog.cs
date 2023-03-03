using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinDialog : MonoBehaviour
{
    public TextMeshProUGUI m_Message;

    private void OnEnable()
    {
        m_Message.text = "Congratulations! " + Global.UserName + $" has won!\nPlease take a screenshot of this message and contact us @_Shoejackcity on Instagram.";
        gameObject.transform.localScale = new Vector3(0, 1, 1);
        LeanTween.scaleX(gameObject, 1, 0.3f);
    }

    private void OnDisable()
    {
        gameObject.transform.localScale = Vector3.zero;
    }
}
