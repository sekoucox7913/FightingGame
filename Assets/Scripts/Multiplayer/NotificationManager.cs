using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    [SerializeField] GameObject PanelNotifi;
    [SerializeField] Text TXT_Notifi;
    [SerializeField] Button BTN_Continue;

	public static NotificationManager _Instance;
	private void Awake()
	{
		if (_Instance == null)
		{
			_Instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(this.gameObject);
		}
	}

	// Start is called before the first frame update
	void Start()
    {
        BTN_Continue.onClick.AddListener(Click_Continue);
        Click_Continue();
    }

    private void Click_Continue()
	{
        PanelNotifi.SetActive(false);
	}

    public void ShowNotification(string notifi)
	{
        TXT_Notifi.text = notifi;
        PanelNotifi.SetActive(true);
	}
}
