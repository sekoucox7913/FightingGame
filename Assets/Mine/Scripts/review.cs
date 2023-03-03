using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class review : MonoBehaviour
{
    [SerializeField] List<Toggle> m_scaleList;
    [SerializeField] TMP_InputField m_comments;

    int m_scale = 1;
    public void SetScaleData(bool isOn)
    {
        for (int i = 0; i < m_scaleList.Count; i++)
        {
            if (m_scaleList[i].isOn)
            {
                m_scale = i + 1;
            }
        }
    }

    public void SubmitButtonClick()
    {
        transform.localScale = Vector3.zero;
        ServerManager.instance.SubmitReview(m_scale, m_comments.text);
    }

    public void CancelButtonClick()
    {
        transform.localScale = Vector3.zero;
    }
}
