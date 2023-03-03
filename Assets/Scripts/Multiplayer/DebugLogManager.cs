using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugLogManager : MonoBehaviour
{
    [SerializeField] GameObject Panel_Log;
    [SerializeField] Text TXT_Log;

    public static DebugLogManager _Instance;
	private void Awake()
	{
		if(_Instance == null)
		{
			_Instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(this.gameObject);
		}
	}

	public void SetLog(string text)
	{
		TXT_Log.text = text;
	}

	public void AddLog(string text)
	{
		TXT_Log.text += "\n" + text;
	}
}
