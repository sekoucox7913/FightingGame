using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Networking;
using System.Globalization;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public List<GameObject> m_PanelList;

    public TextMeshProUGUI m_TournamentTime;
    public TextMeshProUGUI m_ProductName;
    public TextMeshProUGUI m_Description;
    public TextMeshProUGUI m_ResaleText;
    public TextMeshProUGUI m_RetailText;

    public Image m_ProductImg;

    [SerializeField] Button BTN_Offline;
    [SerializeField] Button BTN_Online;
    [SerializeField] Button BTN_Tournament;

    [SerializeField] Button BTN_Champion1;
    [SerializeField] Button BTN_ChipLib1;

    [SerializeField] Button BTN_Play1;
    [SerializeField] Button BTN_Back1;

    [SerializeField] Button BTN_Champion2;
    [SerializeField] Button BTN_ChipLib2;

    [SerializeField] Button BTN_Play2;
    [SerializeField] Button BTN_Back2;

    //Tournament
    [SerializeField] Button BTN_Champion3;
    [SerializeField] Button BTN_ChipLib3;

    [SerializeField] Button BTN_RSVP;
    [SerializeField] Button BTN_Back3;

    [SerializeField] GameObject Panel_Champion, Panel_ChipLib;
    [SerializeField] List<Toggle> infoList;

    public GameObject m_LoadingObj;
    public List<ChipPack> ChipPackList;
    public TextMeshProUGUI m_RSVPText;

    [SerializeField] Button BTN_Profile;

    // Start is called before the first frame update
    void Start()
    {
        ShowPanel(0);

        BTN_Offline.onClick.AddListener(Click_Offline);
        BTN_Online.onClick.AddListener(Click_Online);
        BTN_Tournament.onClick.AddListener(Click_Tournament);

        BTN_Play1.onClick.AddListener(Click_PlayOffline);
        BTN_Back1.onClick.AddListener(Click_Back);
        BTN_Champion1.onClick.AddListener(Click_Champion);
        BTN_ChipLib1.onClick.AddListener(Click_ChipLib);


        BTN_Play2.onClick.AddListener(Click_PlayOnline);
        BTN_Back2.onClick.AddListener(Click_Back);
        BTN_Champion2.onClick.AddListener(Click_Champion);
        BTN_ChipLib2.onClick.AddListener(Click_ChipLib);


        BTN_RSVP.onClick.AddListener(Click_RSVP);
        BTN_Back3.onClick.AddListener(Click_Back);
        BTN_Champion3.onClick.AddListener(Click_Champion);
        BTN_ChipLib3.onClick.AddListener(Click_ChipLib);

        //04/11/2022
        Panel_Champion.SetActive(true);
        Panel_ChipLib.SetActive(true);
        //

        BTN_Profile.onClick.AddListener(Click_Profile);

        if (!PlayerPrefs.HasKey("restarted"))
        {
            PlayerPrefs.SetInt("restarted", 0);
        }

        if (PlayerPrefs.GetInt("restarted") == 0)
        {
            //join into tournament first.
            StartCoroutine(ServerManager.instance.GetProducts("all", callbackGetProducts));
        }
        else if (PlayerPrefs.GetInt("restarted") == 1)
        {
            //auto-enter into tournament after the player win.
            Global.playmode = 2;
            ServerManager.instance.bExist = true;

            GameController.SelectChampionSprite(ServerManager.instance.m_Mine.characterid);
            CharacterController.Player.characterName = (CharacterName)ServerManager.instance.m_Mine.characterid;
            PlayerPrefs.SetInt("characterid3", ServerManager.instance.m_Mine.characterid);

            //Split
            CharacterController.Player.deck.Clear();
            string[] splitArray = ServerManager.instance.m_Mine.decklist.Split(char.Parse(","));
            for (int j = 0; j < splitArray.Length; j++)
            {
                int index = int.Parse(splitArray[j]);
                CharacterController.Player.deck.Add(ChipPackList[index]);
            }

            gameObject.SetActive(false);
            GameController.instance.OpenLobby();
        }
        else if (PlayerPrefs.GetInt("restarted") == 2)
        {
            PlayerPrefs.SetInt("restarted", 0);
            Global.playmode = 2;
            ServerManager.instance.bExist = true;

            GameController.SelectChampionSprite(ServerManager.instance.m_Mine.characterid);
            CharacterController.Player.characterName = (CharacterName)ServerManager.instance.m_Mine.characterid;
            PlayerPrefs.SetInt("characterid3", ServerManager.instance.m_Mine.characterid);

            //Split
            CharacterController.Player.deck.Clear();
            string[] splitArray = ServerManager.instance.m_Mine.decklist.Split(char.Parse(","));
            for (int j = 0; j < splitArray.Length; j++)
            {
                int index = int.Parse(splitArray[j]);
                CharacterController.Player.deck.Add(ChipPackList[index]);
            }

            gameObject.SetActive(false);
            GameController.instance.OpenLobby();
        }
        else if (PlayerPrefs.GetInt("restarted") == 3)
        {
            if (ServerManager.instance.m_productList.Count > 0)
            {
                m_ProductName.text = ServerManager.instance.m_productList[0].name;
                m_Description.text = ServerManager.instance.m_productList[0].description;
                m_ResaleText.text = "Resale : " + ServerManager.instance.m_productList[0].resale;
                m_RetailText.text = "Retail : " + ServerManager.instance.m_productList[0].retail;
                StartCoroutine(ServerManager.instance.LoadProductImage(ServerManager.instance.m_productList[0].imglink, m_ProductImg));
            }
            PlayerPrefs.SetInt("restarted", 0);

            //Click_Tournament();
            m_LoadingObj.SetActive(true);
            m_TournamentTime.text = "";
            StartCoroutine(ServerManager.instance.GetTournament("all", callbackGetTournament));
        }
    }

    public void ShowPanel(int index)
    {
        for (int i = 0; i < m_PanelList.Count; i++)
        {
            if (i == index)
            {
                m_PanelList[i].SetActive(true);
            }
            else
            {
                m_PanelList[i].SetActive(false);
            }
        }

        if (PlayerPrefs.HasKey("restarted") && PlayerPrefs.GetInt("restarted") == 2) return;

        if (index == 0)
        {
            ServerManager.instance.m_status = 0;
        }
        else if (index == 2)
        {
            ServerManager.instance.m_status = 1;
        }
    }

    private void Click_Offline()
    {
        Global.playmode = 0;
        ShowPanel(1);
        FindObjectOfType<ChipLibrary>().Init();
    }

    private void Click_Online()
    {
        Global.playmode = 1;
        ShowPanel(2);
        FindObjectOfType<ChipLibrary>().Init();
    }

    //When the user click tournament button
    private void Click_Tournament()
    {
        m_LoadingObj.SetActive(true);
        m_TournamentTime.text = "";
        StartCoroutine(ServerManager.instance.GetTournament("all", callbackGetTournament));
        StartCoroutine(ServerManager.instance.GetProducts("all", callbackGetProducts));
    }

    private void Click_Champion()
    {
        //25/10/2022
        Panel_Champion.transform.localScale = Vector3.one;
        Panel_Champion.GetComponent<ShowCharacter>().Init();
        //
        gameObject.SetActive(false);
    }

    private void Click_ChipLib()
    {
        //25/10/2022
        Panel_ChipLib.transform.localScale = Vector3.one;
        Panel_ChipLib.GetComponent<ChipLibrary>().Init();
        //
        gameObject.SetActive(false);
    }

    private void Click_PlayOffline()
    {
        Global.playmode = 0;
        GameController.instance.OpenLobby();
    }

    private void Click_PlayOnline()
    {
        Global.playmode = 1;
        GameController.instance.OpenLobby();
    }

    //when the user click "RSVP" button
    private void Click_RSVP()
    {
        //if (CharacterController.Player.deck.Sum(x => x.qty) < 5)
        if (CharacterController.Player.GetChipCount() < 5)
        {
            GameController.DisplayMessage(ServerManager.instance.m_MessageList[16], 2f);
            return;
        }

        if (!PlayerPrefs.HasKey("characterid3"))
        {
            GameController.DisplayMessage(ServerManager.instance.m_MessageList[17], 2f);
            return;
        }

        m_LoadingObj.SetActive(true);

        ServerManager.instance.m_Mine.username = PlayerPrefs.GetString("PlayerName", "");
        ServerManager.instance.m_Mine.characterid = PlayerPrefs.GetInt("characterid3", 0);
        ServerManager.instance.m_Mine.decklist = PlayerPrefs.GetString("decklist3", "");
        ServerManager.instance.m_Mine.productid = ServerManager.instance.m_tournamentList[0].productid;
        for (int i = 0; i < infoList.Count; i++)
        {
            if (infoList[i].isOn)
            {
                ServerManager.instance.m_Mine.productinfo = (7 + i * 0.5f).ToString();
                i = infoList.Count;
            }
        }
        ServerManager.instance.m_Mine.score = 0;
        ServerManager.instance.m_Mine.token = PlayerPrefs.GetString("token");

        Debug.Log("token: " + ServerManager.instance.m_Mine.token);

        if(m_RSVPText.text == "RSVP")
        {
            StartCoroutine(ServerManager.instance.AddUser(null, callbackAddUser));
        }
        else if (m_RSVPText.text == "Update")
        {
            StartCoroutine(ServerManager.instance.UpdateUser(ServerManager.instance.m_Mine.username, callbackUpdateUser));
        }else if(m_RSVPText.text == "Lobby")
        { 
            gameObject.SetActive(false);
            GameController.instance.OpenLobby();
            m_LoadingObj.SetActive(false);
        }
    }

    private void Click_Back()
    {
        ShowPanel(0);
    }

    private void Click_Profile()
    {
        Application.OpenURL(ServerManager.instance.m_SurveyLink);
    }

    //Set shoe size
    public void SetSizeData(bool isOn)
    {
        if (m_RSVPText.text != "Lobby") return;
        float tmp1 = float.Parse(ServerManager.instance.m_Mine.productinfo);
        int tmp2 = (int)((tmp1 - 7f) / 0.5f);

        for (int i = 0; i < infoList.Count; i++)
        {
            if(infoList[i].isOn)
            {
                if (i != tmp2)
                {
                    m_RSVPText.text = "Update";
                }
            }
        }
    }

    //Get shoes data
    int callbackGetProducts(UnityWebRequest www)
    {
        m_ProductName.text = "";
        m_Description.text = "";
        m_ResaleText.text = "";
        m_RetailText.text = "";
        
        m_ProductImg.enabled = false;

        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            ServerManager.instance.m_productList.Clear();
            for (int i = 0; i < json["data"].Count; i++)
            {
                product m_product = new product();
                m_product.id = int.Parse(json["data"][i]["id"].ToString());
                m_product.name = json["data"][i]["name"].ToString();
                m_product.imglink = json["data"][i]["imglink"].ToString();
                m_product.description = json["data"][i]["description"].ToString();
                m_product.resale = json["data"][i]["resale"].ToString();
                m_product.retail = json["data"][i]["retail"].ToString();

                ServerManager.instance.m_productList.Add(m_product);
            }
            if (ServerManager.instance.m_productList.Count > 0)
            {
                //Debug.LogError("Get product");
                m_ProductName.text = ServerManager.instance.m_productList[0].name;
                m_Description.text = ServerManager.instance.m_productList[0].description;
                m_ResaleText.text = "Resale : " + ServerManager.instance.m_productList[0].resale;
                m_RetailText.text = "Retail : " + ServerManager.instance.m_productList[0].retail;
                StartCoroutine(ServerManager.instance.LoadProductImage(ServerManager.instance.m_productList[0].imglink, m_ProductImg));
            }
        }
        else if (status == "fail")
        {
            //Debug.Log("GetProducts: Failed");
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
            if(ServerManager.instance.m_tournamentList.Count > 0 && int.Parse(json["data"].Count.ToString()) > 0)
            {
                if(ServerManager.instance.m_tournamentList[0].starttime != json["data"][0]["starttime"].ToString())
                {
                    ServerManager.instance.m_tournamentList.Clear();
                    ServerManager.instance.ResetUserData();
                    PlayerPrefs.SetInt("restarted", 3);

                    SceneManager.LoadScene("Game");
                }
            }

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
                DateTime m_Time = DateTime.ParseExact(ServerManager.instance.m_tournamentList[0].starttime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                m_TournamentTime.text = string.Format("{0 :MM/dd/yyyy HH:mm} (EST)", m_Time);

                PlayerPrefs.SetInt("tournament", ServerManager.instance.m_tournamentList[0].id);

                if (ServerManager.instance.m_tournamentList[0].status == 0)
                {
                    ServerManager.instance.ResetUserData();
                    GameController.DisplayMessage(ServerManager.instance.m_MessageList[3], ServerManager.instance.m_Delay);
                }
                else if (ServerManager.instance.m_tournamentList[0].status == 1)
                {
                    StartCoroutine(ServerManager.instance.GetUsers("all", callbackUserCount));
                }
                else if (ServerManager.instance.m_tournamentList[0].status == 2)
                {
                    DateTime oriTime = DateTime.ParseExact(ServerManager.instance.m_tournamentList[0].starttime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    DateTime startTime = oriTime;//.AddHours(ServerManager.instance.CheckDaylightSavingTime());

                    TimeSpan span = ServerManager.instance.GetTimeDifference(startTime);
                    if (span.TotalSeconds >= 30 || !PlayerPrefs.HasKey("characterid3"))
                    {
                        ServerManager.instance.ResetUserData();
                        GameController.DisplayMessage(ServerManager.instance.m_MessageList[1], ServerManager.instance.m_Delay);
                        StartCoroutine(ServerManager.instance.DeleteUser(PlayerPrefs.GetString("PlayerName"), callbackDeleteUser));
                    }
                    else
                    {
                        StartCoroutine(ServerManager.instance.GetUsers(PlayerPrefs.GetString("PlayerName"), callbackGetUser));
                    }
                }
                else if (ServerManager.instance.m_tournamentList[0].status == 3)
                {
                    ServerManager.instance.ResetUserData();
                    GameController.DisplayMessage(ServerManager.instance.m_MessageList[3], ServerManager.instance.m_Delay);
                }
                else if (ServerManager.instance.m_tournamentList[0].status == 4)
                {
                    ServerManager.instance.ResetUserData();
                    GameController.DisplayMessage(ServerManager.instance.m_MessageList[3], ServerManager.instance.m_Delay);
                }
            }
        }
        else if (status == "fail")
        {
            //Debug.Log("GetTournament: Failed");
        }

        m_LoadingObj.SetActive(false);

        return 0;
    }

    //get total users count
    int callbackUserCount(UnityWebRequest www)
    {
        if (www.downloadHandler.text == "")
        {
            ShowPanel(3);
            return 0;
        }
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            if (json["data"].Count >= ServerManager.instance.m_MaxPlayer)
            {
                GameController.DisplayMessage(ServerManager.instance.m_MessageList[12], ServerManager.instance.m_Delay);
            }
            else
            {
                StartCoroutine(ServerManager.instance.GetUsers(PlayerPrefs.GetString("PlayerName"), callbackGetUser));
            }
        }
        else if (status == "fail")
        {
            //Debug.Log("GetUser: Failed");
        }

        return 0;
    }

    //get user's list
    int callbackGetUser(UnityWebRequest www)
    {
        if (www.downloadHandler.text == "")
        {
            Global.playmode = 2;

            //FindObjectOfType<ChipLibrary>().Init();

            ShowPanel(3);
            return 0;
        }
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            for (int i = 0; i < json["data"].Count; i++)
            {
                ServerManager.instance.m_Mine.id = int.Parse(json["data"][i]["id"].ToString());
                ServerManager.instance.m_Mine.username = json["data"][i]["username"].ToString();
                ServerManager.instance.m_Mine.characterid = int.Parse(json["data"][i]["characterid"].ToString());
                ServerManager.instance.m_Mine.decklist = json["data"][i]["decklist"].ToString();
                ServerManager.instance.m_Mine.productid = int.Parse(json["data"][i]["productid"].ToString());
                ServerManager.instance.m_Mine.productinfo = json["data"][i]["productinfo"].ToString();
                ServerManager.instance.m_Mine.score = int.Parse(json["data"][i]["score"].ToString());
                ServerManager.instance.m_Mine.token = json["data"][i]["token"].ToString();
            }

            if (int.Parse(json["data"].Count.ToString()) > 0)
            {
                if (ServerManager.instance.m_Mine.score >= ServerManager.instance.m_tournamentList[0].level)
                {
                    Global.playmode = 2;

                    ServerManager.instance.bExist = true;

                    GameController.SelectChampionSprite(ServerManager.instance.m_Mine.characterid);
                    CharacterController.Player.characterName = (CharacterName)ServerManager.instance.m_Mine.characterid;
                    PlayerPrefs.SetInt("characterid3", ServerManager.instance.m_Mine.characterid);

                    //Split
                    CharacterController.Player.deck.Clear();
                    string[] splitArray = ServerManager.instance.m_Mine.decklist.Split(char.Parse(","));
                    for (int j = 0; j < splitArray.Length; j++)
                    {
                        int index = int.Parse(splitArray[j]);
                        CharacterController.Player.deck.Add(ChipPackList[index]);
                    }

                    if (ServerManager.instance.m_tournamentList[0].status == 1)
                    {
                        float tmp1 = float.Parse(ServerManager.instance.m_Mine.productinfo);
                        int tmp2 = (int)((tmp1 - 7f) / 0.5f);
                        for (int i = 0; i < infoList.Count; i++)
                        {
                            if (i == tmp2)
                            {
                                infoList[i].isOn = true;
                            }
                            else
                            {
                                infoList[i].isOn = false;
                            }
                        }
                        m_RSVPText.text = "Lobby";
                        ShowPanel(3);
                    }
                    else if (ServerManager.instance.m_tournamentList[0].status == 2)
                    {
                        gameObject.SetActive(false);
                        GameController.instance.OpenLobby();
                    }
                }
                else
                {
                    StartCoroutine(ServerManager.instance.DeleteUser(PlayerPrefs.GetString("PlayerName"), callbackDeleteUser));
                }
            }
            else
            {
                Global.playmode = 2;

                ShowPanel(3);
            }
        }
        else if (status == "fail")
        {
            //Debug.Log("GetUser: Failed");
        }

        return 0;
    }

    //register new user
    int callbackAddUser(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            gameObject.SetActive(false);
            GameController.instance.OpenLobby();
        }
        else if (status == "fail")
        {
            //Debug.Log("AddUser: Failed");
        }

        m_LoadingObj.SetActive(false);

        return 0;
    }

    //upgrade user's level
    int callbackUpdateUser(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            gameObject.SetActive(false);
            GameController.instance.OpenLobby();
        }
        else if (status == "fail")
        {
            //Debug.Log("AddUser: Failed");
        }

        m_LoadingObj.SetActive(false);

        return 0;
    }

    //delete current user
    int callbackDeleteUser(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
        }
        else if (status == "fail")
        {
            //Debug.Log("DeleteUser: Failed");
        }

        return 0;
    }
}
