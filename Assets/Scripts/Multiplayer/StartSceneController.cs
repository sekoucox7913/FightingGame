using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneController : MonoBehaviour
{
    [SerializeField] InputField INP_PlayerName;
    [SerializeField] Button BTN_Connect;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.DeleteAll();
        string playerName = PlayerPrefs.GetString("PlayerName", "");
        if (playerName != "")
        {
            Global.UserName = playerName;
            //FadeManager._Instance.Show_Fade("Welcome " + playerName);
            FadeManager._Instance.Show_Fade();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
        else
        {
            INP_PlayerName.text = "Guest" + Random.Range(0, 9999).ToString("0000");
            FadeManager._Instance.Hide_Fade();
        }
        BTN_Connect.onClick.AddListener(Click_Connect);
    }

	private void Click_Connect()
	{
        string playerName = INP_PlayerName.text;
        if(playerName != "")
		{
            Global.UserName = playerName;
            //FadeManager._Instance.Show_Fade("Welcome " + playername);
            FadeManager._Instance.Show_Fade();
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
}
