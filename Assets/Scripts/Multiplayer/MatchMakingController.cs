using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using LitJson;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Networking;

public class MatchMakingController : MonoBehaviour
{
    [SerializeField] Button BTN_StartGame, BTN_Ready, BTN_Leave;
    
    [Header("------MasterClient---------")]
    [SerializeField] CanvasGroup CG_Master;
    [SerializeField] Text TXT_You_M, TXT_Name_M, TXT_Status_M;
    [SerializeField] Dropdown DRO_Char_M;

    [Header("------OpponentClient-------")]
    [SerializeField] CanvasGroup CG_Normal;
    [SerializeField] Text TXT_You_N, TXT_Name_N, TXT_Status_N;
    [SerializeField] Dropdown DRO_Char_N;

    private bool _isMasterClient = false;
    private int _indexMyChar = 0;
    private int _indexOpponentChar = 0;

    bool bReady = false;
    bool bStart = false;

    // Start is called before the first frame update
    void Start()
    {
        BTN_StartGame.onClick.AddListener(Click_StartGame);
        BTN_Ready.onClick.AddListener(Click_Ready);
        BTN_Leave.onClick.AddListener(Click_Leave);
        DRO_Char_M.onValueChanged.AddListener(Change_MasterChar);
        DRO_Char_N.onValueChanged.AddListener(Change_NormalChar);

        DRO_Char_M.enabled = false;
        DRO_Char_N.enabled = false;

        bReady = false;
        bStart = false;
    }

    private void OnEnable()
    {
        //GameController.instance.lobbyScreen.SetActive(false);
    }


    private void Click_StartGame()
	{
        ServerManager.instance.bWait = false;
        ServerManager.instance.m_WaitTime = 0;
        FadeManager._Instance.Hide_Fade();
        PunManager._Instance.SendStartSignal();
	}

    private void Click_Ready()
	{
        ServerManager.instance.m_WaitTime = 0;
        if(ServerManager.instance.m_status < 3)
        {
            ServerManager.instance.bWait = true;
            ServerManager.instance.m_status = 3;
        }

        GameController.instance.lobbyScreen.GetComponent<TournamentStartTime>().StopAllCoroutines();
        GameController.instance.lobbyScreen.GetComponent<TournamentStartTime>().CancelInvoke();

        FadeManager._Instance.Hide_Fade();
        PunManager._Instance.ChangePlayerStatus(_indexMyChar, true);
    }

    private void Click_Leave()
	{
        PunManager._Instance.LeaveRoom();
        Debug.Log("Playmode : " + Global.playmode.ToString());
	}

    private void Change_MasterChar(int index)
	{
        if(_isMasterClient == false)
		{
            return;
		}

        if (index == _indexMyChar)
        {
            return;
        }
        _indexMyChar = index;
        PunManager._Instance.ChangePlayerStatus(_indexMyChar, false);
    }

    private void Change_NormalChar(int index)
	{
        if(_isMasterClient == true)
		{
            return;
		}

        if(index == _indexMyChar)
		{
            return;
		}

        _indexMyChar = index;
        PunManager._Instance.ChangePlayerStatus(_indexMyChar, false);
    }

    public void StartGame()
	{
        Hashtable roomInfo = PhotonNetwork.CurrentRoom.CustomProperties;
        bool isMasterLeft = roomInfo["IsMasterLeft"] == null ? false : (bool)roomInfo["IsMasterLeft"];
        if (_isMasterClient)
        {
            //Global.MyCT = isMasterLeft ? CharacterType.Player : CharacterType.Enemy;
            Global.IsMaster = isMasterLeft;
        }
        else
        {
            //Global.MyCT = isMasterLeft ? CharacterType.Enemy : CharacterType.Player;
            Global.IsMaster = !isMasterLeft;
        }
        Global.MyCT = CharacterType.Player;

        if (Global.MyCT == CharacterType.Player)
        {
            GameController.SelectChampionSprite(_indexMyChar);
            GameController.SelectEnemySprite(_indexOpponentChar);
            CharacterController.Enemy.characterName = (CharacterName)_indexOpponentChar;
        }
        else
        {
            GameController.SelectChampionSprite(_indexOpponentChar);
            GameController.SelectEnemySprite(_indexMyChar);
        }
        GameController.StartGame();
    }

    public void UpdatedRoomInfo()
	{
        Hashtable roomInfo = PhotonNetwork.CurrentRoom.CustomProperties;
        bool isMasterLeft = roomInfo["IsMasterLeft"] == null ? false : (bool)roomInfo["IsMasterLeft"];

        CG_Master.gameObject.SetActive(false);
        CG_Normal.gameObject.SetActive(false);

        int countReady = 0;
        Dictionary<int, Photon.Realtime.Player> players = PhotonNetwork.CurrentRoom.Players;
        foreach(Photon.Realtime.Player player in players.Values)
		{
            Hashtable playerInfo = player.CustomProperties;
            string name = player.NickName;
            bool isMasterClient = player.IsMasterClient;
            bool isMyPlayer = Global.UserName == name;
            int indexChar = (int)playerInfo["IndexChar"];
            bool isReady = (bool)playerInfo["IsReady"];

            countReady += isReady ? 1 : 0;

			if (isMasterClient)
			{
                _isMasterClient = isMyPlayer;
                ShowMasterClient(isMyPlayer, name, indexChar, isReady);
			}
			else
			{
                ShowNormalClient(isMyPlayer, name, indexChar, isReady);
			}

			if (isMyPlayer)
			{
                _indexMyChar = indexChar;
			}
			else
			{
                _indexOpponentChar = indexChar;
			}
		}

        if(_isMasterClient && countReady == 2)
		{
            BTN_StartGame.interactable = true;
            if (Global.playmode == 2 && !bStart)
            {
                bStart = true;
                Click_StartGame();
                Debug.Log("Start Game");
            }
        }
        else
		{
            BTN_StartGame.interactable = false;
        }
    }

    private void ShowMasterClient(bool isYour, string name, int indexChar, bool isReady)
	{
        CG_Master.gameObject.SetActive(true);
        CG_Master.interactable = isYour && !isReady;
        TXT_You_M.gameObject.SetActive(isYour);
        TXT_Name_M.text = name;
        TXT_Status_M.text = isReady ? "On Ready" : "";
        DRO_Char_M.value = indexChar;

        if (isYour)
        {
            BTN_Ready.interactable = !isReady;

            if (Global.playmode == 2 && BTN_Ready.interactable && !bReady)
            {
                bReady = true;
                Invoke("Click_Ready", 2.0f);
                Debug.Log("Ready Game");
            }
        }
    }

    private void ShowNormalClient(bool isYour, string name, int indexChar, bool isReady)
    {
        CG_Normal.gameObject.SetActive(true);
        CG_Normal.interactable = isYour && !isReady;
        TXT_You_N.gameObject.SetActive(isYour);
        TXT_Name_N.text = name;
        TXT_Status_N.text = isReady ? "On Ready" : "";
        DRO_Char_N.value = indexChar;

		if (isYour)
		{
            BTN_Ready.interactable = !isReady;

            if (Global.playmode == 2 && BTN_Ready.interactable && !bReady)
            {
                bReady = true;
                Invoke("Click_Ready", 2.0f);
                Debug.Log("Ready Game");
            }
        }
    }
}
