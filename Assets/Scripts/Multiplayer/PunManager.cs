using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PunManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	public MatchMakingController MMC;
    public PUNROOMTYPE roomType = PUNROOMTYPE.NONE;
    public RoomOptions roomOptions = new RoomOptions();
    public string[] users = new string[1];

    bool bFocus = true;
    DateTime m_FocusTime;

    public bool bWait = false;
    public float m_WaitTime = 0;

    public static PunManager _Instance;

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

        SetInit();
    }

    private void Update()
    {
        if (Global.playmode != 2)
        {
            return;
        }

        //if the disconnected time(background mode) is over 30 seconds, auto-loss from match.
        if (bWait)
        {
            if (m_WaitTime >= 30)
            {
                bWait = false;
                m_WaitTime = 0;
                GameController.DisplayMessage(ServerManager.instance.m_MessageList[11], ServerManager.instance.m_Delay);
                Character myChar = Global.MyCT != CharacterType.Player ? CharacterController.Player : CharacterController.Enemy;
                myChar.SetHealth(-100);
                GameController.instance.EndGame();
            }
            else
            {
                m_WaitTime += Time.deltaTime;
            }
        }
    }

    #region MonoBehavious CONNECT & CREATE

    void SetRoomOptions()
    {
        roomOptions.MaxPlayers = 2;
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.PlayerTtl = 180000;
    }

    public void ShowResult()
    {
        Debug.Log("PhotonNetwork.UseAlternativeUdpPorts : " + PhotonNetwork.ServerPortOverrides.ToString());
        Debug.Log("PhotonNetwork.CrcCheckEnabled : " + PhotonNetwork.CrcCheckEnabled.ToString());
        Debug.Log("PhotonNetwork.IsMessageQueueRunning : " + PhotonNetwork.IsMessageQueueRunning.ToString());
        Debug.Log("PhotonNetwork.NetworkingClient.LoadBalancingPeer.MaximumTransferUnit : " + PhotonNetwork.NetworkingClient.LoadBalancingPeer.MaximumTransferUnit.ToString());
        Debug.Log("PhotonNetwork.QuickResends : " + PhotonNetwork.QuickResends.ToString());
        Debug.Log("PhotonNetwork.MaxResendsBeforeDisconnect : " + PhotonNetwork.MaxResendsBeforeDisconnect.ToString());
    }

    void SetInit()
    {
        PhotonNetwork.ServerPortOverrides = PhotonPortDefinition.AlternativeUdpPorts;
        PhotonNetwork.CrcCheckEnabled = true;
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.MaximumTransferUnit = 520;
        PhotonNetwork.QuickResends = 3;
        PhotonNetwork.MaxResendsBeforeDisconnect = 7;
    }

    public void JoinOrCreatRoom_GamePlay()
    {
        roomType = PUNROOMTYPE.MATCHING;

        if(ServerManager.instance.m_status != 3)
        {
            FadeManager._Instance.Show_Fade("Connecting...");
        }

        ServerManager.instance.bWait = true;
        ServerManager.instance.m_WaitTime = 0;
        ServerManager.instance.m_status = 3;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.NickName = Global.UserName;

            Hashtable playerInfo = new Hashtable();
            playerInfo.Add("IndexChar", (int)CharacterController.Player.characterName);
            playerInfo.Add("IsReady", false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerInfo);

            if (Global.playmode == 1) //PVP : join in random room
            {
                PhotonNetwork.JoinRandomRoom();
            }else if (Global.playmode == 2) //tournament : join in pre-defined room
            {
                SetRoomOptions();

                if (ServerManager.instance.m_Mine.username == ServerManager.instance.m_gameList[0].userid1)
                {
                    users[0] = ServerManager.instance.m_gameList[0].userid2;
                }
                else
                {
                    users[0] = ServerManager.instance.m_gameList[0].userid1;
                }
                PhotonNetwork.JoinOrCreateRoom(ServerManager.instance.m_gameList[0].roomid, roomOptions, TypedLobby.Default, null);
            }
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void ChangePlayerStatus(int indexChar, bool isReady)
	{
        Debug.Log("Ready!");
        Hashtable playerInfo = PhotonNetwork.LocalPlayer.CustomProperties;
        if(playerInfo.ContainsKey("IndexChar") == false)
		{
            playerInfo.Add("IndexChar", 0);
		}
        if(playerInfo.ContainsKey("IsReady") == false)
		{
            playerInfo.Add("IsReady", false);
		}
        playerInfo["IndexChar"] = indexChar;
        playerInfo["IsReady"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerInfo);
    }

    public void LeaveRoom()
	{
		if (Global.playmode == 0)
		{
            return;
		}
        PhotonNetwork.LeaveRoom();
	}

#if UNITY_EDITOR || UNITY_IOS
    void OnApplicationPause(bool pause)
    {
        if (roomType != PUNROOMTYPE.GAMEPLAYING)
        {
            return;
        }

        if (pause) LeaveApplication();
        else ResumeApplication();
    }
#elif UNITY_ANDROID
    void OnApplicationFocus(bool focus)
    {
        if(roomType != PUNROOMTYPE.GAMEPLAYING)
		{
            return;
		}

        if (focus) ResumeApplication();
        else LeaveApplication();
    }
#endif

    void LeaveApplication()
    {
        if (Global.IsPausePlay == false)
        {
            Global.IsPausePlay = true;
            Global.TimePaused = PhotonNetwork.Time;
            Global.IsGameStarted = false;
            DebugLogManager._Instance.AddLog("Paused Time : " + Global.TimePaused);

            if (Global.playmode == 2)
            {
                bFocus = false;
                m_FocusTime = DateTime.Now.ToUniversalTime();
            }
        }

        RaiseEventOptions option = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EVENT_CODE_PAUSE, 1, option, SendOptions.SendReliable);
        PhotonNetwork.SendAllOutgoingCommands();

        Debug.Log("Application pause");
    }

    //check disconnected time is over 30 seconds or not
    void ResumeApplication()
    {
        if (Global.playmode == 2)
        {
            if (!bFocus)
            {
                bFocus = true;
                TimeSpan result = DateTime.Now.ToUniversalTime() - m_FocusTime;
                if (result.TotalSeconds > 30)
                {
                    //GameController.DisplayMessage(ServerManager.instance.m_MessageList[4], ServerManager.instance.m_Delay);
                    Character myChar = Global.MyCT == CharacterType.Player ? CharacterController.Player : CharacterController.Enemy;
                    myChar.SetHealth(-100);
                    GameController.instance.EndGame();
                }
                else
                {
                    m_FocusTime = DateTime.Now.ToUniversalTime();
                    if (routine_resum != null)
                    {
                        StopCoroutine(routine_resum);
                    }
                    routine_resum = StartCoroutine(ResumGame());

                    Debug.Log("Application resume");
                }
            }
        }
        else if (Global.playmode == 1)
        {
            m_FocusTime = DateTime.Now.ToUniversalTime();
            if (routine_resum != null)
            {
                StopCoroutine(routine_resum);
            }
            routine_resum = StartCoroutine(ResumGame());

            Debug.Log("Application resume");
        }
        else
        {
            Global.IsGameStarted = true;
        }
    }

    Coroutine routine_resum;
    private System.Collections.IEnumerator ResumGame()
	{
        while (PhotonNetwork.Time == Global.TimePaused)
        {
            yield return null;
        }

        if (PhotonNetwork.Time - Global.TimePaused >= 30d)
        {
            //UnityEngine.SceneManagement.SceneManager.LoadScene("Lose");
            //LeaveRoom();
            GameController.DisplayMessage(ServerManager.instance.m_MessageList[21], ServerManager.instance.m_Delay);
            Character myChar = Global.MyCT == CharacterType.Player ? CharacterController.Player : CharacterController.Enemy;
            myChar.SetHealth(-100);
            GameController.instance.EndGame();
        }
        else
        {
            RaiseEventOptions option = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(EVENT_CODE_RESUM, 1, option, SendOptions.SendReliable);
            PhotonNetwork.SendAllOutgoingCommands();
            Global.IsGameStarted = true;

        }
    }

    #endregion

    #region SEND & RECEIVE

    const byte EVENT_CODE_START = 1;
    const byte EVENT_CODE_END = 11;
    const byte EVENT_CODE_IDLE = 2;
    const byte EVENT_CODE_MOVE = 3;
    const byte EVENT_CODE_TARGET = 4;
    const byte EVENT_CODE_ATTACK = 5;
    const byte EVENT_CODE_HEALTH = 6;
    const byte EVENT_CODE_MANA = 7;

    const byte EVENT_CODE_PAUSE = 141;
    const byte EVENT_CODE_RESUM = 142;
    const byte EVENT_CODE_RESUM_RESPONSE = 143;

    public void SendStartSignal()
	{
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        RaiseEventOptions option = new RaiseEventOptions();
        option.Receivers = ReceiverGroup.All;
        PhotonNetwork.RaiseEvent(EVENT_CODE_START, null, option, SendOptions.SendReliable);
	}

    public void SendEndSignal()
    {
        if (Global.playmode == 0)
        {
            return;
        }
        RaiseEventOptions option = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EVENT_CODE_END, null, option, SendOptions.SendReliable);
    }

    public void SendIdle()
	{
		if (Global.playmode == 0)
		{
            return;
		}
        RaiseEventOptions option = new RaiseEventOptions() { Receivers = ReceiverGroup.Others};
        PhotonNetwork.RaiseEvent(EVENT_CODE_IDLE, null, option, SendOptions.SendReliable);
    }

    public void SendMove(List<Vector2Int> path)
	{
        return;
        if (Global.playmode == 0)
        {
            return;
        }
        object[] content = new object[path.Count*2];
        for(int i = 0; i < path.Count; i++)
		{
            content[i*2] = path[i].x;
            content[i*2 + 1] = path[i].y;
		}
        RaiseEventOptions option = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EVENT_CODE_MOVE, content, option, SendOptions.SendReliable);
    }

    public void SendTarget(Vector2Int coordinate)
    {
        if (Global.playmode == 0)
        {
            return;
        }
		object[] content = { coordinate.x, coordinate.y};
        RaiseEventOptions option = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EVENT_CODE_TARGET, content, option, SendOptions.SendReliable);
    }

    public void SendAttack(Chip chip)
	{
        if (Global.playmode == 0)
        {
            return;
        }
        object[] content = { chip.displayName, chip.description, chip.Cost, chip.level, chip.Damage, (int)chip.atkType, chip.cooldown, chip.damageDelay};
        RaiseEventOptions option = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EVENT_CODE_ATTACK, content, option, SendOptions.SendReliable);
    }

    public void SendHealth(int health)
	{
        if (Global.playmode == 0)
        {
            return;
        }
        RaiseEventOptions option = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EVENT_CODE_HEALTH, health, option, SendOptions.SendReliable);
    }

    public void SendMana(int mana)
    {
        if (Global.playmode == 0)
        {
            return;
        }
        RaiseEventOptions option = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EVENT_CODE_MANA, mana, option, SendOptions.SendReliable);
    }

	public void OnEvent(EventData eventData)
	{
        if(PhotonNetwork.InRoom == false || roomType == PUNROOMTYPE.NONE)
		{
            return;
		}

        byte eventCode = eventData.Code;
        Debug.Log("EventCode" + eventCode + " Sender" + eventData.Sender);
		switch (eventCode)
		{
            case EVENT_CODE_START:
                MMC.StartGame();
                roomType = PUNROOMTYPE.GAMEPLAYING;
                break;
            case EVENT_CODE_END:
                ReceiveEndSignal();
                break;
            case EVENT_CODE_IDLE:
                ReceiveIdle();
                break;
            case EVENT_CODE_MOVE:
                ReceiveMove((object[])eventData.CustomData);
                break;
            case EVENT_CODE_TARGET:
                ReceiveTarget((object[])eventData.CustomData);
                break;
            case EVENT_CODE_ATTACK:
                ReceiveAttack((object[])eventData.CustomData);
                break;
            case EVENT_CODE_HEALTH:
                ReceiveHealth((int)eventData.CustomData);
                break;
            case EVENT_CODE_MANA:
                ReceiveMana((int)eventData.CustomData);
                break;
            case EVENT_CODE_PAUSE:
                ReceivePause();
                break;
            case EVENT_CODE_RESUM:
                ReceiveResum();
                break;
            case EVENT_CODE_RESUM_RESPONSE:
                ReceiveResumResponse();
                break;
            default:
                break;
		}
	}

    void ReceiveEndSignal()
    {
        Global.IsPausePlay = false;
        Global.TimePaused = 0d;
        Global.LengthPaused = 0d;
        roomType = PUNROOMTYPE.NONE;
    }

    void ReceiveIdle()
	{
        Debug.Log("ReceiveIdle");
        Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;
        opponentChar.SetState(new Idle(opponentChar));
	}

	void ReceiveMove(object[] content)
	{
        Debug.Log("ReceiveMove");
        List<Vector2Int> path = new List<Vector2Int>();
        for(int i = 0; i < content.Length; i+=2)
		{
            path.Add(new Vector2Int(7 - (int)(content[i]), (int)content[i+1]));
		}
        Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;
        //11/15/2022
        if(opponentChar.health > 0)
        {
            opponentChar.SetState(new Move(opponentChar, path));
        }
    }

    void ReceiveTarget(object[] targetInfo)
	{
        Debug.Log("ReceiveTarget");
        Vector2Int target = new Vector2Int(7 - (int)targetInfo[0], (int)targetInfo[1]);
        Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;

        //11/15/2022
        if (opponentChar.health > 0)
        {
            MoveRequestManager.Get.RequestMove(opponentChar, target);
        }
        //opponentChar.SetState(new SetTarget(opponentChar, target));
    }

	void ReceiveAttack(object[] chipInfo)
	{
        Debug.Log("Receive Attack!");
        //object[] chipInfo = { chip.displayName, chip.description, chip.Cost, chip.level, chip.Damage, (int)chip.atkType, chip.cooldown, chip.damageDelay };
        Chip chip = new Chip();
        chip.displayName = (string)chipInfo[0];
        chip.description = (string)chipInfo[1];
        chip.Cost = (int)chipInfo[2];
        chip.level = (int)chipInfo[3];
        chip.Damage = (int)chipInfo[4];
        chip.atkType = (AtkType)(int)chipInfo[5];
        chip.cooldown = (float)chipInfo[6];
        chip.damageDelay = (float)chipInfo[7];
        Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;

        //11/15/2022
        if (opponentChar.health > 0)
        {
            opponentChar.SetState(new Attack(opponentChar, chip));
            if(chip.atkType == AtkType.Basic)
            {
                //if the player type is Draco and used standard attack, show beam attack effect
                if (CharacterController.Enemy.characterName == CharacterName.Draco)
                {
                    var beam = CharacterController.Enemy.transform.GetChild(2);
                    beam.gameObject.SetActive(true);
                    beam.GetComponent<ParticleSystem>().Play();
                    StartCoroutine(EndBlasting(beam));
                }
            }
        }
    }

    private System.Collections.IEnumerator DisableAfterSeconds(Transform t, float time)
    {
        yield return new WaitForSeconds(time);
        t.gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator EndBlasting(Transform t)
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(DisableAfterSeconds(t, 0.5f));
        CharacterController.Enemy.GetComponent<Animator>().SetBool("ContinueBlasting", false);
    }

    void ReceiveHealth(int health)
	{
        Debug.Log("ReceiveHealth");
        Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;

        //11/15/2022
        if (opponentChar.health > 0)
        {
            opponentChar.SetHealth(health);
        }
    }

    void ReceiveMana(int mana)
    {
        Debug.Log("ReceiveMana " + mana);

        if (CharacterController.Enemy.health > 0)
        {
            CharacterController.Enemy.SetMana(CharacterController.Enemy.mana + mana);
        }

        if (CharacterController.Player.health > 0)
        {
            CharacterController.Player.SetMana(CharacterController.Player.mana - mana);
        }
    }

    void ReceivePause()
	{
        if(!Global.IsPausePlay)
		{
            Global.IsPausePlay = true;
            Global.TimePaused = PhotonNetwork.Time;

            if (Global.playmode == 2)
            {
                bWait = true;
                m_WaitTime = 0;
            }
        }
	}

    void ReceiveResum()
	{
		if (Global.IsPausePlay)
		{
            if (Global.playmode == 2)
            {
                bWait = false;
                m_WaitTime = 0;
            }

            Global.LengthPaused += PhotonNetwork.Time - Global.TimePaused;
		}
        Global.IsPausePlay = false;

        RaiseEventOptions option = new RaiseEventOptions() { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(EVENT_CODE_RESUM_RESPONSE, 1, option, SendOptions.SendReliable);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    void ReceiveResumResponse()
	{
        if (Global.IsPausePlay)
        {
            Global.LengthPaused += PhotonNetwork.Time - Global.TimePaused;
        }
        Global.IsPausePlay = false;
    }

#endregion

#region PUN CALLBACKS

	public override void OnConnectedToMaster()
    {
        Debug.Log("ConnectedToMaster");

		if (roomType == PUNROOMTYPE.MATCHING)
		{
            JoinOrCreatRoom_GamePlay();
		}
        string appversion = PhotonNetwork.AppVersion;
        string region_cloud = PhotonNetwork.CloudRegion;
        string gameversion = PhotonNetwork.GameVersion;

		Debug.Log(appversion + ":" + region_cloud + ":" + gameversion);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
        //if (Global.playmode == 2) return;
        NotificationManager._Instance.ShowNotification("Disconnection : " + cause);
        if(roomType == PUNROOMTYPE.MATCHING)
		{
            MMC.gameObject.SetActive(false);
        }
    }

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        
    }

    public override void OnJoinedLobby()
    {
        // whenever this joins a new lobby, clear any previous room lists
        string lobby_name = PhotonNetwork.CurrentLobby.Name;
		Debug.Log("Lobby Name : " + lobby_name);
	}

    // note: when a client joins / creates a room, OnLeftLobby does not get called, even if the client was in a lobby before
    public override void OnLeftLobby()
    {
        Debug.Log("OnLeftLobby");
    }

	public override void OnCreatedRoom()
	{
        Debug.Log("OnCreatedRoom");
        FadeManager._Instance.Show_Fade("Creating a room...");
    }

	public override void OnCreateRoomFailed(short returnCode, string message)
    {
        NotificationManager._Instance.ShowNotification(message);
        FadeManager._Instance.Hide_Fade();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        NotificationManager._Instance.ShowNotification(message);
        FadeManager._Instance.Hide_Fade();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string roomName = "Room " + UnityEngine.Random.Range(1000, 10000);
        //bool isMasterLeft = Random.Range(0f, 1f) > 0.5f;

        Hashtable customProperties = new Hashtable();
        customProperties.Add("IsMasterLeft", true);

        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 2, CustomRoomProperties = customProperties };

        PhotonNetwork.CreateRoom(roomName, roomOptions, null);
    }

    public override void OnJoinedRoom()
    {
        // joining (or entering) a room invalidates any cached lobby room list (even if LeaveLobby was not called due to just joining a room)
        Debug.Log("OnJoinedRoom");

        MMC.gameObject.SetActive(true);
        MMC.UpdatedRoomInfo();
        FadeManager._Instance.Hide_Fade();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRooom");

        Global.IsPausePlay = false;
        Global.TimePaused = 0d;
        Global.LengthPaused = 0d;
        Global.playmode = 0;
        
        if (roomType == PUNROOMTYPE.MATCHING)
		{
            MMC.gameObject.SetActive(false);
        }
        if(roomType == PUNROOMTYPE.GAMEPLAYING)
		{

        }
        roomType = PUNROOMTYPE.NONE;
    }

	public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("PlayerEnteredRoom");
        if(roomType == PUNROOMTYPE.MATCHING)
		{
            MMC.UpdatedRoomInfo();
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log("PlayerLeftRoom");
        if(roomType == PUNROOMTYPE.MATCHING)
		{
            MMC.UpdatedRoomInfo();
		}
        if(roomType == PUNROOMTYPE.GAMEPLAYING)
		{
            // You win
            Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;
            opponentChar.SetHealth(0); //11/15/2022
            GameController.instance.EndGame();
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log("MasterClientSwitched");
        if(roomType == PUNROOMTYPE.MATCHING)
		{
            MMC.UpdatedRoomInfo();
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        if(roomType == PUNROOMTYPE.MATCHING)
		{
            MMC.UpdatedRoomInfo();
        }
    }

	public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
        if (roomType == PUNROOMTYPE.MATCHING)
        {
            MMC.UpdatedRoomInfo();
        }
    }

#endregion

}

public enum PUNROOMTYPE { NONE, CONNECTING, MATCHING, GAMEPLAYING };
