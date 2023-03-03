using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    public Text m_DebugTxt;

#if UNITY_EDITOR
    public TimeZoneInfo easternZone; //= TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
#else
    public TimeZoneInfo easternZone;// = TimeZoneInfo.FindSystemTimeZoneById("America/New York");
#endif

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("time zone id : " + easternZone.Id + " display name : " + easternZone.DisplayName);
        m_DebugTxt.text += "time zone id : " + easternZone.Id + " display name : " + easternZone.DisplayName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
