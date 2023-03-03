using LitJson;
using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TournamentStartTime : MonoBehaviour
{
    public GameObject MyItem;
    public GameObject LobbyPlayerItemPrefab;
    public Transform m_Parent;

    public GameObject m_TimerGrp;
    public TextMeshProUGUI m_TimerTxt;
    bool bCountdown;
    float fTmpTime;

    [Header("Dependencies")]
    [SerializeField]
    private GameController controller = null;

    [Header("Components")]
    [SerializeField]
    private TMP_Text text = null;
    [SerializeField]
    private TextMeshProUGUI m_UserCountTxt;
    [SerializeField]
    private Button m_BackBtn;

    public GameObject m_WaitTxt;

    private DateTime startTime;

    private void OnEnable()
    {
        //check tournament status and match list
        ServerManager.instance.m_status = 2;

        InvokeRepeating("ShowUserData", 1f, 10f);

        bCountdown = false;

        DateTime oriTime = DateTime.ParseExact(ServerManager.instance.m_tournamentList[0].starttime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        //startTime = oriTime.AddSeconds(15);//.AddHours(ServerManager.instance.CheckDaylightSavingTime());
        startTime = oriTime;//.AddHours(ServerManager.instance.CheckDaylightSavingTime());

        m_TimerGrp.SetActive(false);

        int result = DateTime.Compare(DateTime.Now.ToUniversalTime().AddHours(ServerManager.instance.CheckDaylightSavingTime()), startTime);
        if (result < 0)
        {
            text.text = string.Format(ServerManager.instance.m_MessageList[9] + "{0 :MM/dd/yyyy HH:mm} (EST)", oriTime);
            TimeSpan span = ServerManager.instance.GetTimeDifference(startTime);

            StartCoroutine(WaitForMatchStart(span.TotalSeconds));
        }
        else
        {
            CheckGame();
        }

        CheckEndData();
    }

    private void Start()
    {
        if (Global.playmode != 2)
            return;
    }

    private void Update()
    {
        if (!bCountdown) return;
        if (fTmpTime <= 0)
        {
            ShowTime();
            fTmpTime = 1.0f;
        }
        else
        {
            fTmpTime -= Time.deltaTime;
        }
    }

    //show remained time
    void ShowTime()
    {
        TimeSpan span = ServerManager.instance.GetTimeDifference(startTime);
        if(span.TotalDays >= 1)
        {
            m_TimerTxt.text = span.Days.ToString() + "d " + span.Hours.ToString() + "h " + span.Minutes.ToString() + "m " + span.Seconds.ToString() + "s";
        }
        else if (span.TotalHours >= 1)
        {
            m_TimerTxt.text = span.Hours.ToString() + "h " + span.Minutes.ToString() + "m " + span.Seconds.ToString() + "s";
        }
        else if(span.TotalSeconds >= 0)
        {
            m_TimerTxt.text = span.Minutes.ToString() + "m " + span.Seconds.ToString() + "s";
        }
        else
        {
            bCountdown = false;
            m_TimerGrp.SetActive(false);
        }

        //Debug.LogError(span.TotalSeconds);
    }

    //show user's list
    void ShowUserData()
    {
        Debug.Log("ShowUserData");
        StartCoroutine(ServerManager.instance.GetUsers("all", callbackGetUser));
    }

    //check remaiend match data
    void ShowGameData()
    {
        Debug.Log("ShowGameData");
        StartCoroutine(ServerManager.instance.GetGame(PlayerPrefs.GetString("PlayerName"), ServerManager.instance.m_tournamentList[0].id.ToString(), callbackGetGame));
    }

    //check the match is ended
    void CheckEndData()
    {
        Debug.Log("CheckEndData");
        StartCoroutine(ServerManager.instance.GetTournament(ServerManager.instance.m_tournamentList[0].id.ToString(), callbackEndGame));
    }

    //check the player is champion
    void CheckWinData()
    {
        Debug.Log("CheckWinData");
        StartCoroutine(ServerManager.instance.GetUsers("all", callbackWinUser));
    }


#if UNITY_EDITOR || UNITY_IOS
    void OnApplicationPause(bool pause)
    {
        if (pause) LeaveApplication();
        else ResumeApplication();
    }
#elif UNITY_ANDROID
    void OnApplicationFocus(bool focus)
    {
        if (focus) ResumeApplication();
        else LeaveApplication();
    }
#endif

    void LeaveApplication()
    {

    }

    void ResumeApplication()
    {
        if (Global.playmode != 2)
            return;

        StartCoroutine(ServerManager.instance.GetGame(PlayerPrefs.GetString("PlayerName"), ServerManager.instance.m_tournamentList[0].id.ToString(), callbackGetGame));
        StartCoroutine(ServerManager.instance.GetUsers("all", callbackGetUser));
    }



    private IEnumerator WaitForMatchStart(double m_time)
    {
        Debug.Log("WaitForMatchStart");

        ShowTime();
        m_TimerGrp.SetActive(true);
        fTmpTime = 1.0f;
        bCountdown = true;
        yield return new WaitForSeconds((float)m_time);
        CheckGame();
    }

    public void CheckGame()
    {
        Debug.Log("CheckGame");
        Debug.Log(PlayerPrefs.GetInt("restarted"));

        m_TimerGrp.SetActive(false);
        if (PlayerPrefs.GetInt("restarted") == 0)
        {
            m_WaitTxt.SetActive(false);
            InvokeRepeating("ShowGameData", 1f, 10f);
            //InvokeRepeating("CheckWinData", 20f, 20f);
        }
        else if (PlayerPrefs.GetInt("restarted") == 1)
        {
            m_WaitTxt.SetActive(true);
            m_BackBtn.gameObject.SetActive(false);
            GameController.DisplayMessage(ServerManager.instance.m_MessageList[13], ServerManager.instance.m_Delay);
            text.text = string.Format(ServerManager.instance.m_MessageList[13]);
            InvokeRepeating("ShowGameData", 1f, 10f);
            InvokeRepeating("CheckWinData", 1f, 15f);
        }
        else if (PlayerPrefs.GetInt("restarted") == 2)
        {
            m_WaitTxt.SetActive(true);
            m_BackBtn.gameObject.SetActive(false);
            InvokeRepeating("ShowGameData", 1f, 10f);
            InvokeRepeating("CheckWinData", 1f, 15f);
        }
    }

    private IEnumerator WaitForGameStart(double m_time)
    {
        Debug.Log("WaitForGameStart");
        yield return new WaitForSeconds((float)m_time);
        StartGame();
    }

    void StartGame()
    {
        Debug.Log("StartGame");
        m_BackBtn.gameObject.SetActive(false);
        PunManager._Instance.JoinOrCreatRoom_GamePlay();
    }

    public void BackButtonClick()
    {
        PlayerPrefs.SetInt("restarted", 0);
        CancelInvoke();
        StopAllCoroutines();
        GameController.instance.menuScreen.SetActive(true);
        GameController.instance.menuScreen.GetComponent<MenuController>().ShowPanel(0);
        gameObject.SetActive(false);
    }

    public void ResetButtonClick()
    {
        PlayerPrefs.SetInt("restarted", 0);
        CancelInvoke();
        StopAllCoroutines();
        GameController.instance.menuScreen.SetActive(true);
        GameController.instance.menuScreen.GetComponent<MenuController>().ShowPanel(0);
        gameObject.SetActive(false);
        ServerManager.instance.ShowReviewDialog();
    }

    //check tournament status and process the result based on tournament status
    int callbackEndGame(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            ServerManager.instance.m_tournamentList.Clear();
            for (int i = 0; i < json["data"].Count; i++)
            {
                tournament m_tournament = new tournament();
                m_tournament.id = int.Parse(json["data"][i]["id"].ToString());
                m_tournament.name = json["data"][i]["name"].ToString();
                m_tournament.productid = int.Parse(json["data"][i]["productid"].ToString());
                m_tournament.starttime = json["data"][i]["starttime"].ToString();
                m_tournament.level = int.Parse(json["data"][i]["level"].ToString());
                m_tournament.status = int.Parse(json["data"][i]["status"].ToString());

                ServerManager.instance.m_tournamentList.Add(m_tournament);
            }
            if (ServerManager.instance.m_tournamentList.Count > 0)
            {
                PlayerPrefs.SetInt("tournament", ServerManager.instance.m_tournamentList[0].id);

                if (ServerManager.instance.m_tournamentList[0].status == 3)
                {
                    GameController.DisplayMessage(ServerManager.instance.m_MessageList[14], ServerManager.instance.m_Delay);
                    ServerManager.instance.ResetUserData();
                    BackButtonClick();
                }
                else if (ServerManager.instance.m_tournamentList[0].status == 4)
                {
                    GameController.DisplayMessage(ServerManager.instance.m_MessageList[22], ServerManager.instance.m_Delay);
                    ServerManager.instance.ResetUserData();
                    BackButtonClick();
                }
            }
        }
        else if (status == "fail")
        {
        }

        return 0;
    }

    int callbackGetUser(UnityWebRequest www)
    {
        foreach (Transform child in m_Parent)
        {
            Destroy(child.gameObject);
        }

        if (www.downloadHandler.text == "")
        {
            return 0;
        }
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            bool bExist = false;

            m_UserCountTxt.text = json["data"].Count.ToString();
            for (int i = 0; i < json["data"].Count; i++)
            {
                string username = json["data"][i]["username"].ToString();
                int characterid = int.Parse(json["data"][i]["characterid"].ToString());

                if (ServerManager.instance.m_Mine.username != username)
                {
                    GameObject obj = Instantiate(LobbyPlayerItemPrefab);
                    obj.name = username;
                    obj.transform.parent = m_Parent;
                    obj.transform.localScale = Vector3.one;
                    obj.GetComponent<playerItem>().SetData(characterid, username);
                }
                else
                {
                    bExist = true;
                    MyItem.SetActive(true);
                    MyItem.GetComponent<playerItem>().SetData(ServerManager.instance.m_Mine.characterid, ServerManager.instance.m_Mine.username + "(Me)");
                }
            }

            if (!bExist)
            {
                MyItem.SetActive(false);
                PlayerPrefs.SetInt("restarted", 4);
                ServerManager.instance.LoadNewGame();
                //ServerManager.instance.ResetUserData();
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[4], ServerManager.instance.m_Delay);
                //Invoke("ResetButtonClick", ServerManager.instance.m_Delay);
            }
        }
        else if (status == "fail")
        {
            Debug.Log("GetUser: Failed");
            //MobileNative.Alert("Error", "Can't find user data", "OK");
            GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
        }

        return 0;
    }

    int callbackWinUser(UnityWebRequest www)
    {
        if (www.downloadHandler.text == "")
        {
            return 0;
        }
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            if(json["data"].Count == 1 && ServerManager.instance.m_Mine.username == json["data"][0]["username"].ToString())
            {
                CancelInvoke();
                ServerManager.instance.ResetUserData();
                ServerManager.instance.ShowReviewDialog();
            }
        }
        else if (status == "fail")
        {
        }
        return 0;
    }

    int callbackGetGame(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            ServerManager.instance.m_gameList.Clear();
            for (int i = 0; i < json["data"].Count; i++)
            {
                game m_game = new game();
                m_game.id = int.Parse(json["data"][i]["id"].ToString());
                m_game.tournamentid = int.Parse(json["data"][i]["tournamentid"].ToString());
                m_game.roomid = json["data"][i]["roomid"].ToString();
                m_game.starttime = json["data"][i]["starttime"].ToString();
                m_game.level = int.Parse(json["data"][i]["level"].ToString());
                m_game.userid1 = json["data"][i]["userid1"].ToString();
                m_game.userid2 = json["data"][i]["userid2"].ToString();
                m_game.status = int.Parse(json["data"][i]["status"].ToString());

                ServerManager.instance.m_gameList.Add(m_game);
            }


            if (ServerManager.instance.m_gameList.Count > 0)
            {
                DateTime oriTime = DateTime.ParseExact(ServerManager.instance.m_gameList[0].starttime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                //startTime = oriTime.AddSeconds(15);//.AddHours(ServerManager.instance.CheckDaylightSavingTime());
                startTime = oriTime;//.AddHours(ServerManager.instance.CheckDaylightSavingTime());

                Debug.Log("WaitForGameStart");

                int result = DateTime.Compare(DateTime.Now.ToUniversalTime().AddHours(ServerManager.instance.CheckDaylightSavingTime()), startTime);
                Debug.Log("result : " + result.ToString());
                if (result < 0)
                {
                    text.text = string.Format(ServerManager.instance.m_MessageList[8] + "{0 :HH:mm} (EST)", oriTime);
                    TimeSpan span = ServerManager.instance.GetTimeDifference(startTime);

                    //ShowTime();
                    //m_TimerGrp.SetActive(true);
                    fTmpTime = 1.0f;
                    bCountdown = true;

                    StartCoroutine(WaitForGameStart(span.TotalSeconds));
                }
                else
                {
                    StartGame();
                }
            }
            else
            {
                //StartCoroutine(ServerManager.instance.GetGame(PlayerPrefs.GetString("PlayerName", ""), ServerManager.instance.m_tournamentList[0].id.ToString(), callbackGetGame));
            }
        }
        else if (status == "fail")
        {
            Debug.Log("GetGames: Failed");
            //MobileNative.Alert("Error", "Can't find game data", "OK");
            //GameController.DisplayMessage("Can't find game data", ServerManager.instance.m_Delay);
        }

        return 0;
    }
}
