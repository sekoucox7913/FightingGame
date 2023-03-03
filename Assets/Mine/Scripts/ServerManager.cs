using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Globalization;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

//Manage server data(Send/receive data, access API)
public class ServerManager : MonoBehaviour
{
    public string m_Server = "https://shoejackcity.howtoreality.com/"; //Server url
    public string m_SurveyLink; //Survey link

    ////
    public List<product> m_productList = new List<product>(); //shoes product list
    public List<tournament> m_tournamentList = new List<tournament>(); //tournament list
    public List<users> m_usersList = new List<users>(); //all users list of tournament
    public List<game> m_gameList = new List<game>(); //match list of tournament

    public users m_Mine = new users();
    public bool bExist;

    //API list
    public string g_APIGetProducts = "getproduct.php"; //get product data
    public string g_APIGetTournament = "gettournament.php"; //get tournament data
    public string g_APIGetUsers = "getusers.php"; //get user data

    public string g_APIGetGame = "getgame.php"; //get match data
    public string g_APIAddUser = "adduser.php"; //add new user's data

    public string g_APIUpdateUser = "updateuser.php"; //update exist user's data
    public string g_APIUpdateScore = "updatescore.php"; //update user's match level

    public string g_APIDeleteUser = "deleteuser.php"; //remove dropped user
    public string g_APIDeleteGame = "deletegame.php"; //passed match data

    public string g_APIAddReview = "addreview.php";


    public int m_MaxPlayer = 120; //Max players count
    public float m_Delay = 5.0f;

    public List<string> m_MessageList = new List<string>();

    public int m_status;
    bool bFocus = true;
    DateTime m_FocusTime;

    public bool bWait = false;
    public float m_WaitTime = 0;

    public static ServerManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void OnEnable()
    {
        ////Requesting WRITE_EXTERNAL_STORAGE and CAMERA permissions simultaneously
        //AndroidRuntimePermissions.Permission[] result = AndroidRuntimePermissions.RequestPermissions(
        //    "android.permission.BIND_JOB_SERVICE");
        //if (result[0] == AndroidRuntimePermissions.Permission.Granted)
        //    Debug.Log("We have all the permissions!");
        //else
        //    Debug.Log("Some permission(s) are not granted...");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("tournament"))
        {
            PlayerPrefs.SetInt("tournament", 0);
        }

        if (!PlayerPrefs.HasKey("PlayerName"))
        {
            GetUsername();
        }

        Global.playmode = 0;
        InitValue();

        Global.UserName = PlayerPrefs.GetString("PlayerName");

        FadeManager._Instance.Show_Fade();
        SceneManager.LoadScene("Game");
    }

    void InitValue()
    {
        PlayerPrefs.SetInt("restarted", 0);
        bExist = false;
        m_status = 0;

        bFocus = true;
        m_FocusTime = DateTime.Now;

        bWait = false;
        m_WaitTime = 0;
    }

    //Generate new user name or get generated username
    void GetUsername()
    {
        // random int
        int num1 = UnityEngine.Random.Range(10000, 99999);
        int num2 = UnityEngine.Random.Range(1000, 9999);
        PlayerPrefs.SetString("PlayerName", "Guest_" + num1.ToString() + num2.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    Screen.sleepTimeout = SleepTimeout.SystemSetting;
        //    Application.Quit();
        //}

        if (Global.playmode != 2)
        {
            return;
        }

        //if the opponent's disconnected time is over 30 seconds, you win.
        if (m_status == 3 && bWait)
        {
            if (m_WaitTime >= 30)
            {
                bWait = false;
                m_WaitTime = 0;

                if(FadeManager._Instance.Panel_Fade.GetComponent<CanvasGroup>().alpha == 1)
                {
                    FadeManager._Instance.Hide_Fade();
                }

                GameController.DisplayMessage(m_MessageList[11], m_Delay);
                m_Mine.score++;
                PlayerPrefs.SetInt("restarted", 1);
                StartCoroutine(DeleteUser(PunManager._Instance.users[0], callbackDeleteUser2));
            }
            else
            {
                m_WaitTime += Time.deltaTime;
            }
        }
    }

    //calculate daylight saving time
    public double CheckDaylightSavingTime()
    {
        TimeZoneInfo mstz = TimeZoneInfo.Local;

        if (mstz.IsDaylightSavingTime(DateTime.Now))
        {
            return -4;
        }
        else
        {
            return -5;
        }
    }

    public TimeSpan GetTimeDifference(DateTime stTime)
    {
        TimeSpan span = stTime - DateTime.Now.ToUniversalTime().AddHours(CheckDaylightSavingTime());
        return span;
    }

    //Show champion message dialog
    public void ShowReviewDialog()
    {
        FindObjectOfType<GameController>().winDialog.SetActive(true);
    }

    public void SubmitReview(int m_scale, string m_comments)
    {
        StartCoroutine(AddReview(m_scale, m_comments, callbackAddreview));
    }

    //process received messages from notifications
    public void ProcessMessage(string title, string body)
    {
        if (title == "News!")
        {
            if (m_status == 0 || m_status == 1)
            {
                MobileNative.Alert("Alert", $"The tournament is started.\n Would you like to enter into the tournament now?",
                    "Yes", () => {
                        PlayerPrefs.SetInt("restarted", 2);
                        SceneManager.LoadScene("Game");
                    },
                    "No", () => {
                        ResetUserData();
                        StartCoroutine(DeleteUser(PlayerPrefs.GetString("PlayerName"), null));
                    }
                );
                return;
            }
            {
                GameController.DisplayMessage(body, m_Delay);
                return;
            }
        }

        if (body == "The tournament has stopped.")
        {
            GameController.DisplayMessage(body, m_Delay);
            ResetUserData();
            Invoke("LoadNewGame", m_Delay);
            //StartCoroutine(DeleteUser(PlayerPrefs.GetString("PlayerName"), callbackDeleteUser));
            return;
        }

        if (title.Contains("Congratulations"))
        {
            ResetUserData();
            Invoke("ShowReviewDialog", m_Delay);
            //StartCoroutine(DeleteUser(PlayerPrefs.GetString("PlayerName"), callbackDeleteUser));
            return;
        }

        if (Global.playmode != 2) return;
        GameController.DisplayMessage(body, m_Delay);

        if (title == "Good News!")
        {
            PlayerPrefs.SetInt("restarted", 1);
            Invoke("LoadNewGame", m_Delay);
        }
    }

    //reset user's data, character, chip library data
    public void ResetUserData()
    {
        InitValue();
        CharacterController.Player.deck.Clear();
        PlayerPrefs.DeleteKey("characterid3");
        PlayerPrefs.DeleteKey("decklist3");

        if (GameController.instance.lobbyScreen.activeSelf)
        {
            GameController.instance.lobbyScreen.GetComponent<TournamentStartTime>().BackButtonClick();
        }
    }

    public void LoadNewGame()
    {
        SceneManager.LoadScene("Game");
    }



#if UNITY_EDITOR || UNITY_IOS
    void OnApplicationPause(bool pause)
    {
        //if (Global.playmode != 2) return;
        if (pause) LeaveApplication();
        else ResumeApplication();
    }
#elif UNITY_ANDROID
    void OnApplicationFocus(bool focus)
    {
        //if (Global.playmode != 2) return;
        if (focus) ResumeApplication();
        else LeaveApplication();
    }
#endif

    //the game is background mode
    public void LeaveApplication()
    {
        if (Global.playmode == 2)
        {
            bFocus = false;
            m_FocusTime = DateTime.Now.ToUniversalTime();
        }
    }

    //the game is restored from background mode
    public void ResumeApplication()
    {
        if (m_status == 0 || m_status == 1)
        {
            //StartCoroutine(GetTournament("all", callbackCheckTournament));
        }

        if (Global.playmode != 2) return;

        if (m_status == 2)
        {
            if (PlayerPrefs.GetInt("restarted") == 0)
            {
                StartCoroutine(GetTournament("all", callbackGetTournament));
            }
        }
        else if (m_status == 3)
        {
            if (!bFocus)
            {
                bFocus = true;
                TimeSpan result = DateTime.Now.ToUniversalTime() - m_FocusTime;
                if (result.TotalSeconds > 30)
                {
                    //ResetUserData();
                    //GameController.DisplayMessage(m_MessageList[4], m_Delay);
                    //StartCoroutine(DeleteUser(PlayerPrefs.GetString("PlayerName"), callbackDeleteUser));
                    //if (PhotonNetwork.CurrentRoom != null)
                    {
                        PunManager._Instance.LeaveRoom();
                    }
                    PlayerPrefs.SetInt("restarted", 4);
                    LoadNewGame();
                }
                else
                {
                    m_FocusTime = DateTime.Now.ToUniversalTime();
                    Debug.Log("Application resume");
                    StartCoroutine(GetUsers(m_Mine.username, callbackGetUser));
                }
            }
        }
        else if (m_status == 4)
        {
            StartCoroutine(GetUsers(m_Mine.username, callbackGetUser));
        }
    }

    int callbackAddreview(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
        }
        else if (status == "fail")
        {
            Debug.Log("GetUser: Failed");
            //MobileNative.Alert("Error", "Can't find user data", "OK");
            //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
        }

        //ServerManager.instance.LoadNewGame();

        return 0;
    }

    //Get user data
    int callbackGetUser(UnityWebRequest www)
    {
        if (www.downloadHandler.text == "")
        {
            return 0;
        }
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            if (int.Parse(json["data"].Count.ToString()) == 0)
            {
                //ResetUserData();
                //GameController.DisplayMessage(m_MessageList[4], m_Delay);
                //Invoke("ShowReviewDialog", m_Delay);
                PlayerPrefs.SetInt("restarted", 4);
                LoadNewGame();
            }
            else
            {
                StartCoroutine(GetGame(m_Mine.username, m_tournamentList[0].id.ToString(), callbackGetGame));
            }
        }
        else if (status == "fail")
        {
            Debug.Log("GetUser: Failed");
            //MobileNative.Alert("Error", "Can't find user data", "OK");
            //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
        }

        return 0;
    }

    //get match data
    int callbackGetGame(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            //m_game.starttime = json["data"][i]["starttime"].ToString();
            if (int.Parse(json["data"].Count.ToString()) == 0)
            {
                //ResetUserData();
                //GameController.DisplayMessage(m_MessageList[4], m_Delay);
                //Invoke("ShowReviewDialog", m_Delay);
                PlayerPrefs.SetInt("restarted", 4);
                LoadNewGame();
            }
            else
            {
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

    //delete the user data
    int callbackDeleteUser(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            Invoke("ShowReviewDialog", m_Delay);
        }
        else if (status == "fail")
        {
            //Debug.Log("DeleteUser: Failed");
        }

        return 0;
    }

    //Get tournament data
    int callbackGetTournament(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            m_tournamentList.Clear();
            for (int i = 0; i < json["data"].Count; i++)
            {
                tournament m_tournament = new tournament();
                m_tournament.id = int.Parse(json["data"][i]["id"].ToString());
                m_tournament.name = json["data"][i]["name"].ToString();
                m_tournament.productid = int.Parse(json["data"][i]["productid"].ToString());
                m_tournament.starttime = json["data"][i]["starttime"].ToString();
                m_tournament.level = int.Parse(json["data"][i]["level"].ToString());
                m_tournament.status = int.Parse(json["data"][i]["status"].ToString());

                m_tournamentList.Add(m_tournament);
            }
            if (m_tournamentList.Count > 0)
            {
                PlayerPrefs.SetInt("tournament", m_tournamentList[0].id);

                if (m_tournamentList[0].status == 1)
                {

                }
                else if (m_tournamentList[0].status == 2)
                {
                    StartCoroutine(GetUsers(m_Mine.username, callbackGetUser));
                }
                else if (m_tournamentList[0].status == 3)
                {
                    ResetUserData();
                    GameController.DisplayMessage(m_MessageList[14], m_Delay);
                    Invoke("LoadNewGame", m_Delay);
                    //StartCoroutine(DeleteUser(PlayerPrefs.GetString("PlayerName"), callbackDeleteUser));
                }
                else if (m_tournamentList[0].status == 4)
                {
                    ResetUserData();
                    GameController.DisplayMessage(m_MessageList[3], m_Delay);
                    Invoke("LoadNewGame", m_Delay);
                    //StartCoroutine(DeleteUser(PlayerPrefs.GetString("PlayerName"), callbackDeleteUser));
                }
            }
        }
        else if (status == "fail")
        {
            //Debug.Log("GetTournament: Failed");
        }

        return 0;
    }

    int callbackCheckTournament(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            m_tournamentList.Clear();
            for (int i = 0; i < json["data"].Count; i++)
            {
                tournament m_tournament = new tournament();
                m_tournament.id = int.Parse(json["data"][i]["id"].ToString());
                m_tournament.name = json["data"][i]["name"].ToString();
                m_tournament.productid = int.Parse(json["data"][i]["productid"].ToString());
                m_tournament.starttime = json["data"][i]["starttime"].ToString();
                m_tournament.level = int.Parse(json["data"][i]["level"].ToString());
                m_tournament.status = int.Parse(json["data"][i]["status"].ToString());

                m_tournamentList.Add(m_tournament);
            }
            if (m_tournamentList.Count > 0)
            {
                if (m_tournamentList[0].status == 1)
                {

                }
                else if (m_tournamentList[0].status == 2)
                {
                    DateTime oriTime = DateTime.ParseExact(m_tournamentList[0].starttime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    //DateTime startTime = oriTime.AddSeconds(15);//.AddHours(CheckDaylightSavingTime());
                    DateTime startTime = oriTime;//.AddHours(CheckDaylightSavingTime());

                    TimeSpan span = GetTimeDifference(startTime);
                    if (span.TotalSeconds < 30 && PlayerPrefs.HasKey("characterid3"))
                    {
                        MobileNative.Alert("Alert", $"The tournament has started.\n Would you like to enter the tournament now?",
                            "Yes", () => {
                                PlayerPrefs.SetInt("restarted", 2);
                                SceneManager.LoadScene("Game");
                            },
                            "No", () => {
                                ResetUserData();
                                StartCoroutine(DeleteUser(PlayerPrefs.GetString("PlayerName"), null));
                            }
                        );
                    }
                    else
                    {
                        //ResetUserData();
                        //GameController.DisplayMessage(m_MessageList[4], m_Delay);
                        //StartCoroutine(DeleteUser(PlayerPrefs.GetString("PlayerName"), callbackDeleteUser));
                        PlayerPrefs.SetInt("restarted", 4);
                        LoadNewGame();
                    }
                }
                else if (m_tournamentList[0].status == 3)
                {
                    ResetUserData();
                    GameController.DisplayMessage(m_MessageList[14], m_Delay);
                    StartCoroutine(DeleteUser(PlayerPrefs.GetString("PlayerName"), callbackDeleteUser));
                }
                else if (m_tournamentList[0].status == 4)
                {
                    ResetUserData();
                    GameController.DisplayMessage(m_MessageList[3], m_Delay);
                    StartCoroutine(DeleteUser(PlayerPrefs.GetString("PlayerName"), callbackDeleteUser));
                }
            }
        }
        else if (status == "fail")
        {
            //Debug.Log("GetTournament: Failed");
        }

        return 0;
    }

    //delete user data
    int callbackDeleteUser2(UnityWebRequest www)
    {
        StartCoroutine(DeleteGame(m_Mine.username, callbackDeleteGame1));

        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            Debug.Log("loss");
        }
        else if (status == "fail")
        {
        }

        return 0;
    }

    //delete match data
    int callbackDeleteGame1(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            StartCoroutine(UpdateScore(m_Mine.username, m_Mine.score.ToString(), callbackUpdateScore));
        }
        else if (status == "fail")
        {
        }

        return 0;
    }

    //upgrade user's match level
    int callbackUpdateScore(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            //if (PhotonNetwork.CurrentRoom != null)
            {
                PunManager._Instance.LeaveRoom();
            }
            Invoke("LoadNewGame", m_Delay);
        }
        else if (status == "fail")
        {
        }

        return 0;
    }


    //download product image
    public IEnumerator LoadProductImage(string strImgPath, Image m_ProductImg)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(strImgPath);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            m_ProductImg.sprite = sp;
            m_ProductImg.preserveAspect = true;
            m_ProductImg.enabled = true;
        }
    }

    public IEnumerator AddReview(int m_param1, string m_param2, Func<UnityWebRequest, int> callbackFunction = null)
    {
        string strUrl = m_Server + g_APIAddReview;

        WWWForm form = new WWWForm();
        form.AddField("username", PlayerPrefs.GetString("PlayerName"));
        form.AddField("scale", m_param1);
        form.AddField("subscribe", m_param2);

        using (UnityWebRequest www = UnityWebRequest.Post(strUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
            }
            else
            {
                if (callbackFunction != null)
                {
                    callbackFunction(www);
                }
            }

            LoadNewGame();
        }
    }

    //get shoes product data
    public IEnumerator GetProducts(string m_param, Func<UnityWebRequest, int> callbackFunction = null)
    {
        string strUrl = m_Server + g_APIGetProducts;

        WWWForm form = new WWWForm();
        form.AddField("id", m_param);

        using (UnityWebRequest www = UnityWebRequest.Post(strUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
            }
            else
            {
                if (callbackFunction != null)
                {
                    callbackFunction(www);
                }
            }
        }
    }

    //Get tournament data
    public IEnumerator GetTournament(string m_param, Func<UnityWebRequest, int> callbackFunction = null)
    {
        string strUrl = m_Server + g_APIGetTournament;

        WWWForm form = new WWWForm();
        form.AddField("id", m_param);

        using (UnityWebRequest www = UnityWebRequest.Post(strUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                if (FindObjectOfType<MenuController>() != null)
                {
                    FindObjectOfType<MenuController>().m_LoadingObj.SetActive(false);
                }
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
            }
            else
            {
                if (callbackFunction != null)
                {
                    callbackFunction(www);
                }
            }
        }
    }

    //get user's list
    public IEnumerator GetUsers(string m_param, Func<UnityWebRequest, int> callbackFunction = null)
    {
        string strUrl = m_Server + g_APIGetUsers;

        WWWForm form = new WWWForm();
        form.AddField("id", m_param);

        using (UnityWebRequest www = UnityWebRequest.Post(strUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
            }
            else
            {
                if (callbackFunction != null)
                {
                    callbackFunction(www);
                }
            }
        }
    }

    //Get match list
    public IEnumerator GetGame(string m_param1, string m_param2, Func<UnityWebRequest, int> callbackFunction = null)
    {
        string strUrl = m_Server + g_APIGetGame;

        WWWForm form = new WWWForm();
        form.AddField("userid", m_param1);
        form.AddField("tournamentid", m_param2);

        using (UnityWebRequest www = UnityWebRequest.Post(strUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
            }
            else
            {
                if (callbackFunction != null)
                {
                    callbackFunction(www);
                }
            }
        }
    }

    //register new user
    public IEnumerator AddUser(string m_param, Func<UnityWebRequest, int> callbackFunction = null)
    {
        string strUrl = m_Server + g_APIAddUser;

        WWWForm form = new WWWForm();
        form.AddField("username", m_Mine.username);
        form.AddField("characterid", m_Mine.characterid.ToString());
        form.AddField("decklist", m_Mine.decklist);
        form.AddField("productid", m_Mine.productid.ToString());
        form.AddField("productinfo", m_Mine.productinfo);
        form.AddField("score", m_Mine.score.ToString());
        form.AddField("token", m_Mine.token.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(strUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                if (FindObjectOfType<MenuController>() != null)
                {
                    FindObjectOfType<MenuController>().m_LoadingObj.SetActive(false);
                }
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
            }
            else
            {
                if (callbackFunction != null)
                {
                    callbackFunction(www);
                }
            }
        }
    }

    //update user's data
    public IEnumerator UpdateUser(string m_param, Func<UnityWebRequest, int> callbackFunction = null)
    {
        string strUrl = m_Server + g_APIUpdateUser;

        WWWForm form = new WWWForm();
        form.AddField("userid", m_param);
        form.AddField("characterid", m_Mine.characterid.ToString());
        form.AddField("decklist", m_Mine.decklist);
        form.AddField("productid", m_Mine.productid.ToString());
        form.AddField("productinfo", m_Mine.productinfo);
        form.AddField("score", m_Mine.score.ToString());
        form.AddField("token", m_Mine.token.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(strUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                if (FindObjectOfType<MenuController>() != null)
                {
                    FindObjectOfType<MenuController>().m_LoadingObj.SetActive(false);
                }
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
            }
            else
            {
                if (callbackFunction != null)
                {
                    callbackFunction(www);
                }
            }
        }
    }

    //update only user's level
    public IEnumerator UpdateScore(string m_param1, string m_param2, Func<UnityWebRequest, int> callbackFunction = null)
    {
        string strUrl = m_Server + g_APIUpdateScore;

        WWWForm form = new WWWForm();
        form.AddField("userid", m_param1);
        form.AddField("score", m_param2);

        using (UnityWebRequest www = UnityWebRequest.Post(strUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
            }
            else
            {
                if (callbackFunction != null)
                {
                    callbackFunction(www);
                }
            }
        }
    }

    //delete user's data
    public IEnumerator DeleteUser(string m_param, Func<UnityWebRequest, int> callbackFunction = null)
    {
        string strUrl = m_Server + g_APIDeleteUser;

        WWWForm form = new WWWForm();
        form.AddField("userid", m_param);

        using (UnityWebRequest www = UnityWebRequest.Post(strUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
            }
            else
            {
                if (callbackFunction != null)
                {
                    callbackFunction(www);
                }
            }
        }
    }

    //delete match data
    public IEnumerator DeleteGame(string m_param, Func<UnityWebRequest, int> callbackFunction = null)
    {
        string strUrl = m_Server + g_APIDeleteGame;

        WWWForm form = new WWWForm();
        form.AddField("userid", m_param);

        using (UnityWebRequest www = UnityWebRequest.Post(strUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                //GameController.DisplayMessage(ServerManager.instance.m_MessageList[0], ServerManager.instance.m_Delay);
            }
            else
            {
                if (callbackFunction != null)
                {
                    callbackFunction(www);
                }
            }
        }
    }
}


//define product structure 
public class product
{
    public int id;
    public string name;
    public string imglink;
    public string description;
    public string resale;
    public string retail;
}

//define tournament structure
public class tournament
{
    public int id;
    public string name;
    public int productid;
    public string starttime;
    public int level;
    public int status;
}

//define user's structure
public class users
{
    public int id;
    public string username;
    public int characterid;
    public string decklist;
    public int productid;
    public string productinfo;
    public int score;
    public string token;
}

//define match structure
public class game
{
    public int id;
    public int tournamentid;
    public string roomid;
    public string starttime;
    public int level;
    public string userid1;
    public string userid2;
    public int status;
}