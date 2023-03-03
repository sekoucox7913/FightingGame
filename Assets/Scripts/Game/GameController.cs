using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using LitJson;
using Photon.Pun;
using UnityEngine.Networking;

public enum GameInType { Character, Boulder };


public class GameController : MonoBehaviour
{
    public bool skipLobby;
    [SerializeField]
    private TMP_Text versionText;
    [SerializeField]
    private FadeIn fadeIn;
    //[SerializeField]
    //private ChargeMeter chargeMeter_L, chargeMeter_R;
    [SerializeField]
    public GameObject reviewScreen;
    [SerializeField]
    public GameObject lobbyScreen;
    [SerializeField]
    public GameObject menuScreen;
    [SerializeField]
    private GameObject matchMakingScreen, pauseScreen, reconnectScreen, winScreen, loseScreen;
    [SerializeField]
    private GameObject controllers_L, controllers_R;
    public static GameController instance;

    [Header("Character anmator")]
    [SerializeField]
    private RuntimeAnimatorController Yasuke;
    [SerializeField]
    private RuntimeAnimatorController Draco;
    [SerializeField]
    private RuntimeAnimatorController Marbelle;

    [Header("Head up display")]
    [SerializeField]
    private MessageManager messageDlg;
    [SerializeField]
    private GameTimer timer;
    public GameObject winDialog;

    [Header("Chip animation prefab")]
    [SerializeField]
    private GameObject areaAdvanceOrb;
    [SerializeField]
    private GameObject lightningPrefab;
    public static GameObject LightningPrefab => instance.lightningPrefab;
    [SerializeField]
    private GameObject m_WolfProjectile;
    [SerializeField]
    private GameObject orbitarProjectile;
    [SerializeField]
    private GameObject m_FireBlastPrefab;
    [SerializeField]
    private GameObject m_BoulderPrefab;
    [SerializeField]
    private Transform m_BoulderParent;

    [SerializeField]
    private GameObject m_BubbleShieldPrefab;
    [SerializeField]
    private GameObject m_EarthquakePrefab;
    [SerializeField]
    private GameObject m_PeacemakerPrefab;
    [SerializeField]
    private GameObject m_PopFire1Prefab;
    [SerializeField]
    private GameObject m_PopFire2Prefab;
    [SerializeField]
    private GameObject m_RadiantBurstDashPrefab;
    [SerializeField]
    private GameObject m_RadiantBurstFirePrefab;
    [SerializeField]
    private GameObject m_Recovery50Prefab;
    [SerializeField]
    private GameObject m_SlapOffPrefab;

    [Space]
    [SerializeField]
    private List<Shake> shakeObjects;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private List<Material> beamMaterials;
    public static List<Material> BeamMaterials => instance.beamMaterials;

    //24/10/2022
    public List<GameObject> m_BoulderList = new List<GameObject>();
    //

    public static Canvas Canvas => instance.canvas;
    public static GameObject PauseScreen => instance.pauseScreen;
    public static GameObject WinScreen => instance.winScreen;
    public static GameObject LoseScreen => instance.loseScreen;
    public static GameObject ReconnectScreen => instance.reconnectScreen;

    private Coroutine m_TimerRoutine = null;
    
    public void OpenLobby()
    {
        //check different chip count is below than 5 or not
        if (CharacterController.Player.GetChipCount() < 5)
        {
            DisplayMessage(ServerManager.instance.m_MessageList[16], 2f);
            return;
        }

        if (Global.playmode == 0) //PVE
        {
            menuScreen.SetActive(false);
            if (skipLobby)
                StartGame();
            else
                lobbyScreen.SetActive(true);
        }
        else if (Global.playmode == 1) //PVP
        {
            PunManager._Instance.JoinOrCreatRoom_GamePlay();
        }
        else if (Global.playmode == 2) //Tournament
        {
            lobbyScreen.SetActive(true);
        }
    }

    Coroutine endgame = null;

    public static void SetTimer(float time, System.Action onTimeUp)
    {
        instance.m_TimerRoutine = instance.StartCoroutine(instance.Timer(time, () =>
        {
            Debug.Log("GAME CONTROLLER : ON TIME UP");
            onTimeUp?.Invoke();
        }));
    }

    private IEnumerator Timer(float time, System.Action onTimeUp)
    {
        yield return new WaitForSeconds(time);
        onTimeUp?.Invoke();
    }

    public static void DisableAfterSeconds(Transform t, float time) => instance.StartCoroutine(instance.DisableAfterTime(t, time));

    private IEnumerator DisableAfterTime(Transform t, float time)
    {
        yield return new WaitForSeconds(time);
        t.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (endgame != null)
            return;

        //if (CharacterController.Player.health <= 0)
        //    endgame = StartCoroutine(EndGame("Lose"));
        //else if (CharacterController.Enemy.health <= 0)
        //    endgame = StartCoroutine(EndGame("Win"));

        Character myChar = Global.MyCT == CharacterType.Player ? CharacterController.Player : CharacterController.Enemy;
        Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;

        //if (myChar.health <= 0)
        //    endgame = StartCoroutine(EndGame("Lose"));
        //else if (opponentChar.health <= 0)
        //    endgame = StartCoroutine(EndGame("Win"));

        //if(myChar.health <= 0 || opponentChar.health <= 0)
        //{
        //    EndGame();
        //}
    }

    //private IEnumerator EndGame(string sceneName)
    //{
    //    //if (sceneName == "Lose")
    //    //    CharacterController.Player.PlayAnim("Die");
    //    //else
    //    //    CharacterController.Enemy.PlayAnim("Die");
    //    Character myChar = Global.MyCT == CharacterType.Player ? CharacterController.Player : CharacterController.Enemy;
    //    Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;
    //    if (sceneName == "Lose")
    //        myChar.PlayAnim("Die");
    //    else
    //        opponentChar.PlayAnim("Die");

    //    yield return new WaitForSeconds(1.0f);
    //      PunManager._Instance.LeaveRoom();
    //    SceneManager.LoadScene(sceneName);
    //    endgame = null;
    //}

    public static void StartGame()
    {
        ServerManager.instance.bWait = false;
        ServerManager.instance.m_WaitTime = 0;

        ServerManager.instance.m_status = 4;


        instance.fadeIn.Fade();
        instance.lobbyScreen.GetComponent<TournamentStartTime>().StopAllCoroutines();
        instance.lobbyScreen.GetComponent<TournamentStartTime>().CancelInvoke();
        instance.lobbyScreen.SetActive(false);
        
        CharacterController.Player.SetState(new SetTarget(CharacterController.Player, new Vector2Int(3, 2)));
		
        if (Global.playmode == 0) //PVE
		{
            Global.MyCT = CharacterType.Player;
            CharacterController.Enemy.GetComponent<EnemyController>().enabled = true;
            CharacterController.Enemy.GetComponent<EnemyController>().StartGame();
        }
		else //PVP or Tournament
		{
            CharacterController.Enemy.GetComponent<EnemyController>().enabled = false;
            if(Global.MyCT == CharacterType.Enemy)
            {
                CharacterController.Enemy.deck.Clear();
                foreach (ChipPack chipPack in CharacterController.Player.deck)
                {
                    CharacterController.Enemy.deck.Add(chipPack);
                }
            }
        }
        Global.IsPausePlay = false;
        Global.LengthPaused = 0f;
        Global.TimePaused = 0f;

        if(Global.MyCT == CharacterType.Player)
        {
            instance.controllers_R.SetActive(true);
            instance.controllers_L.SetActive(false);

            instance.controllers_R.GetComponent<ActionController>().OnGameStart();
        }
        else
        {
            instance.controllers_R.SetActive(false);
            instance.controllers_L.SetActive(true);

            instance.controllers_L.GetComponent<ActionController>().OnGameStart();
        }
        
        instance.timer.StartTimer();
        instance.menuScreen.SetActive(false);
        instance.matchMakingScreen.SetActive(false);
        Global.IsGameStarted = true;
    }

    void NewMatch()
    {
        SceneManager.LoadScene("Game");
    }

    public void CloseWinDialog()
    {
        winDialog.gameObject.SetActive(false);
        Application.OpenURL(ServerManager.instance.m_SurveyLink);
    }

    public static void StopGame()
    {

    }

    public void EndGame()
    {
        for(int i = 0; i < m_BoulderList.Count; i++)
        {
            Destroy(m_BoulderList[i]);
        }
        m_BoulderList.Clear();


        if (routineEndGame != null)
        {
            return;
        }

        if (bEnd) return;

        routineEndGame = StartCoroutine(ProcessEndGame());
        PunManager._Instance.roomType = PUNROOMTYPE.NONE;
        //if(Global.IsOfflinePlay == false)
        //{
        //    PunManager._Instance.SendEndSignal();
        //}

    }

    Coroutine routineEndGame = null;
    public bool bEnd = false;

    //process after the match is ended
    private IEnumerator ProcessEndGame()
    {
        Global.IsGameStarted = false;

        Character myChar = Global.MyCT == CharacterType.Player ? CharacterController.Player : CharacterController.Enemy;
        Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;

        PlayerPrefs.SetInt("restarted", 0);

        yield return new WaitForSeconds(0.3f);

        bool isWinner = myChar.health > opponentChar.health;
        if (isWinner)
        {
            //opponentChar.PlayAnim("Die");
            opponentChar.animator.SetInteger("Die", 0);

            if (reconnectScreen.activeSelf)
            {
                reconnectScreen.SetActive(false);
            }
            winScreen.SetActive(true);

            bEnd = true;

            if (Global.playmode == 2)
            {
                ServerManager.instance.m_Mine.score++;
                PlayerPrefs.SetInt("restarted", 1);
                //if (PhotonNetwork.CurrentRoom != null)
                {
                    PunManager._Instance.LeaveRoom();
                }

                StartCoroutine(ServerManager.instance.DeleteUser(PunManager._Instance.users[0], null));

                yield return new WaitForSeconds(3f);
                //winScreen.SetActive(false);
                StartCoroutine(ServerManager.instance.DeleteGame(ServerManager.instance.m_Mine.username, callbackDeleteGame1));
            }
        }
        else
        {
            //myChar.PlayAnim("Die");
            myChar.animator.SetInteger("Die", 0);

            if (reconnectScreen.activeSelf)
            {
                reconnectScreen.SetActive(false);
            }
            loseScreen.SetActive(true);

            bEnd = true;
            if (Global.playmode == 2)
            {
                //if (PhotonNetwork.CurrentRoom != null)
                {
                    PunManager._Instance.LeaveRoom();
                }

                ServerManager.instance.ResetUserData();

                yield return new WaitForSeconds(3f);
                //loseScreen.SetActive(false);
                PlayerPrefs.SetInt("restarted", 4);
                ServerManager.instance.LoadNewGame();
            }
        }

        if(Global.playmode == 0 || Global.playmode == 1)
        {
            yield return new WaitForSeconds(3f);

            if (Global.playmode == 1)
            {
                PunManager._Instance.LeaveRoom();
            }

            ServerManager.instance.LoadNewGame();
        }
    }

    //delete ended match data
    int callbackDeleteGame1(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            StartCoroutine(ServerManager.instance.UpdateScore(ServerManager.instance.m_Mine.username, ServerManager.instance.m_Mine.score.ToString(), callbackUpdateScore));
        }
        else if (status == "fail")
        {
        }

        return 0;
    }

    //delete ended match data
    int callbackDeleteGame2(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            StartCoroutine(ServerManager.instance.DeleteUser(ServerManager.instance.m_Mine.username, callbackDeleteUser));
        }
        else if (status == "fail")
        {
        }

        return 0;
    }

    //upgrade user's level
    int callbackUpdateScore(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            Invoke("NewMatch", 3.5f);
        }
        else if (status == "fail")
        {
        }

        return 0;
    }

    //delete dropped user's data
    int callbackDeleteUser(UnityWebRequest www)
    {
        JsonData json = JsonMapper.ToObject(www.downloadHandler.text);

        string status = json["status"].ToString();
        if (status == "success")
        {
            Debug.Log("loss");
            //SceneManager.LoadScene("Game");
            //ServerManager.instance.ShowReviewDialog();
            Application.OpenURL(ServerManager.instance.m_SurveyLink);
        }
        else if (status == "fail")
        {
        }

        return 0;
    }

    public static void SelectChampionSprite(int index)
    {
        if (index == 0)
        {
            CharacterController.Player.GetComponent<Animator>().runtimeAnimatorController = instance.Yasuke;
            CharacterController.Player.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 500);
        }

        if (index == 1)
        {
            CharacterController.Player.GetComponent<Animator>().runtimeAnimatorController = instance.Draco;
            CharacterController.Player.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 500);
        }

        if (index == 2)
        {
            CharacterController.Player.GetComponent<Animator>().runtimeAnimatorController = instance.Marbelle;
            CharacterController.Player.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 500);
        }
    }

    public static void SelectEnemySprite(int index)
    {
        if (index == 0)
        {
            CharacterController.Enemy.GetComponent<Animator>().runtimeAnimatorController = instance.Yasuke;
            CharacterController.Enemy.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 500);
        }

        if (index == 1)
        {
            CharacterController.Enemy.GetComponent<Animator>().runtimeAnimatorController = instance.Draco;
            CharacterController.Enemy.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 500);
        }

        if (index == 2)
        {
            CharacterController.Enemy.GetComponent<Animator>().runtimeAnimatorController = instance.Marbelle;
            CharacterController.Enemy.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 500);
        }
    }

    public void ChampionSelect()
    {

    }

    public void ChipLibrary()
    {

    }

    public static void ShakeScreen(int amount) => instance.StartCoroutine(instance.ShakeRoutine(amount));
    private IEnumerator ShakeRoutine(int amount)
    {

        for (int i = 0; i < 10; i++)
        {
            Vector2 rand = new Vector2(Random.Range(-0.001f * amount, 0.001f * amount), Random.Range(-0.001f * amount, 0.001f * amount));

            foreach (var o in shakeObjects)
                o.ShakeUIElement(rand);

            yield return new WaitForSeconds(Random.Range(0.0f, 0.1f));
        }

        foreach (var o in shakeObjects)
            o.ResetPosition();
    }

    /*
    private void OnEnable()
    {
        CharacterController.Player.health = 500;
        CharacterController.Enemy.health = 500;
    }
    */

    private void Awake()
    {
        instance = this;
        SetVersionText();
    }

    // ADD SCRIPT
	private void Start()
	{
        //define player, enemy health 

        // Todo : put the init here , because there is null ref using onenable ( due to roder of script call )
        CharacterController.Player.health = 500;
        CharacterController.Enemy.health = 500;

        PunManager._Instance.MMC = instance.matchMakingScreen.GetComponent<MatchMakingController>();
        FadeManager._Instance.Hide_Fade();

        if (PlayerPrefs.GetInt("restarted") == 4)
        {
            StartCoroutine(ProcessDropUser());
        }
    }

    //process dropped user's status
    IEnumerator ProcessDropUser()
    {
        PlayerPrefs.SetInt("restarted", 0);
        ServerManager.instance.ResetUserData();
        DisplayMessage(ServerManager.instance.m_MessageList[4], ServerManager.instance.m_Delay);
        yield return new WaitForSeconds(ServerManager.instance.m_Delay);
        StartCoroutine(ServerManager.instance.DeleteUser(ServerManager.instance.m_Mine.username, callbackDeleteUser));
    }

	private void SetVersionText() => versionText.text = "Version " + Application.version;


    public static void DisplayMessage(string message, float delay)
    {
        instance.messageDlg.ShowMessage(message, delay);
    }

    #region effect routine
    // Thor fury
    public static void SpawnOrb(Vector2Int target)
    {
        var targetPos = TileController.Info(target).transform.position;

        var orb = Instantiate(instance.areaAdvanceOrb, instance.canvas.transform);
        orb.transform.position = new Vector3(targetPos.x, targetPos.y + 10);

        instance.StartCoroutine(instance.OrbFallRoutine(orb.transform, targetPos));
    }

    private IEnumerator OrbFallRoutine(Transform orb, Vector2 target)
    {
        while (Vector3.Distance(orb.position, target) > 0.001f)
        {
            // todo: removed debug
            //Debug.Log(orb.position);
            orb.position = Vector3.MoveTowards(orb.position, target, 0.5f);
            yield return new WaitForFixedUpdate();
        }
    }

    public static void SpawnWolf(Character c, Vector2Int targetPosition)
    {
        var wolf = Instantiate(instance.m_WolfProjectile, instance.canvas.transform);
        wolf.transform.position = c.transform.position;

        wolf.transform.localScale = new Vector3(c == CharacterController.Player ? 1 : -1, 1, 1);
        instance.StartCoroutine(instance.WolfMoveRoutine(c, wolf, targetPosition));
        //Destroy(wolf, 1.5f);
    }

    private IEnumerator WolfMoveRoutine(Character c, GameObject wolf, Vector2Int target)
    {
        var xPos = TileController.Info(target).transform.position.x;
        var moveSpeed = 10f;

        if(c == CharacterController.Player)
        {
            while (wolf.transform.position.x < xPos)
            {
                wolf.transform.position = Vector3.MoveTowards(wolf.transform.position, new Vector3(xPos, c.transform.position.y), moveSpeed);
                yield return new WaitForFixedUpdate();
            }

            wolf.GetComponent<Animator>().SetTrigger("Bite");
            Destroy(wolf, 0.3f);
        }
        else
        {
            while(wolf.transform.position.x > xPos)
            {
                wolf.transform.position = Vector3.MoveTowards(wolf.transform.position, new Vector3(xPos, c.transform.position.y), moveSpeed);
                yield return new WaitForFixedUpdate();
            }

            wolf.GetComponent<Animator>().SetTrigger("Bite");
            Destroy(wolf, 0.3f);
        }
    }

    public static GameObject SpawnOrbitar(Character c, Vector2Int target)
    {
        var orbitar = Instantiate(instance.orbitarProjectile, instance.canvas.transform);
        orbitar.transform.position = c.transform.position;

        // ADD SCRIPT
        if(target.x < 0)
        {
            target.x = 0;
        }
        else if(target.x >= TileController.BoardSize.x)
        {
            target.x = TileController.BoardSize.x - 1;
        }
        //

        instance.StartCoroutine(instance.OrbitarMoveRoutine(c, TileController.Info(target).transform.position, orbitar));
        Destroy(orbitar, 1.5f);
        return orbitar;
    }

    private IEnumerator OrbitarMoveRoutine(Character c, Vector3 target, GameObject orbitar)
    {
        var startTime = Time.time;
        while (Time.time - startTime < 1.0f)
        {
            orbitar.transform.position = Vector3.MoveTowards(orbitar.transform.position, target, 0.5f);
            yield return new WaitForFixedUpdate();
        }
        foreach (var child in orbitar.GetComponentsInChildren<Transform>())
            child.gameObject.SetActive(true);

    }

    // Todo: add fire spawn and move here.
    public static void SpawnFireBlast(Character c, bool isControlledPlayer)
    {

        Debug.Log(c.transform.position);

        var fireblast = Instantiate(instance.m_FireBlastPrefab, instance.canvas.transform);

        if(isControlledPlayer)
        {
            fireblast.transform.position = new Vector2(c.transform.position.x + 1.5f, c.transform.position.y - 1.2f);
            
        }
        else if(!isControlledPlayer)
        {
            fireblast.transform.position = new Vector2(c.transform.position.x - 1.5f, c.transform.position.y - 1.2f);
            var scl = fireblast.transform.localScale;
            scl.x *= -1f;

            fireblast.transform.localScale = scl;
        }

        Destroy(fireblast, 0.8f);
    }

    public static void SpawnRecovery50(Vector2Int target)
    {
        var targetPosition = TileController.Info(target).transform.position;
        var prefab = Instantiate(instance.m_Recovery50Prefab, instance.canvas.transform);
        prefab.transform.position = new Vector2(targetPosition.x, targetPosition.y + 0.5f);

        Destroy(prefab, 0.5f);
    }

    //instantiate boulder gameoject and add it into in-game object list
    public static void SpawnBoulder(Vector2Int target, CharacterType characterType)
    {
        GameObject prefab = Instantiate(instance.m_BoulderPrefab, instance.m_BoulderParent);// instance.canvas.transform);
        prefab.transform.position = GetRealPosition(target);

        //24/10/2022
        prefab.GetComponent<Boulder>().inType = GameInType.Boulder;
        prefab.GetComponent<Boulder>().characterType = characterType;

        prefab.GetComponent<Boulder>().SetPos(target);
        for(int i = 0; i < instance.m_BoulderList.Count; i++)
        {
            if(prefab.GetComponent<Boulder>().currentPos == instance.m_BoulderList[i].GetComponent<Boulder>().currentPos)
            {
                Destroy(instance.m_BoulderList[i]);
                instance.m_BoulderList.RemoveAt(i);
            }
        }
        instance.m_BoulderList.Add(prefab);
        TileController.Info(target).boulder = prefab.GetComponent<Boulder>();

        //Destroy(prefab, 1.5f);
        //
    }

    public static Vector2 GetRealPosition(Vector2Int target)
    {
        var targetPosition = TileController.Info(target).transform.position;
        return new Vector2(targetPosition.x, targetPosition.y + 0.5f);
    }

    public static void SpawnSlapOff(Character c, Vector2Int target)
    {
        var targetPosition = TileController.Info(target).transform.position;
        var prefab = Instantiate(instance.m_SlapOffPrefab, instance.canvas.transform);
        prefab.transform.position = new Vector2(targetPosition.x + 0.5f, targetPosition.y);

        // todo: check for multi if that it is working
        prefab.transform.localScale = new Vector3(c == CharacterController.Player ? -1 : 1, 1, 1);

        Destroy(prefab, 0.5f);
    }

    public static void SpawnBubbleShield(Character c, Vector2Int target, int protectionLevel = 1)
    {
        var targetPosition = TileController.Info(target).transform.position;
        var prefab = Instantiate(instance.m_BubbleShieldPrefab, instance.canvas.transform);
        prefab.transform.position = new Vector2(targetPosition.x, targetPosition.y + 0.5f);
        prefab.transform.localScale = new Vector3(c == CharacterController.Player ? -1 : 1, 1, 1);

        prefab.GetComponent<MatchTransformPosition>().SetTargetTransform(c.transform, new Vector2(0f, 0.5f));
        c.SetProtection(protectionLevel, prefab);
    }

    public static void SpawnPopFire(Vector2Int target)
    {
        var adjacentsTile = TileController.GetAdjacentTiles(target);

        if(adjacentsTile != null)
        {
            // add also target tile in the list
            adjacentsTile.Add(target);

            foreach(var t in adjacentsTile)
            {
                var targetPosition = TileController.Info(t).transform.position;
                var prefab = Instantiate(instance.m_PopFire1Prefab, instance.canvas.transform);
                prefab.transform.position = new Vector2(targetPosition.x, targetPosition.y + 0.6f);
                Destroy(prefab, 0.5f);
            }
        }
    }

    /// <summary>
    /// Will spawn the weapon animation only , not the impact that goes with it.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="hasHitTarget"></param>
    public static void SpawnPeacemaker(Character c, Vector2Int target)
    {
        var targetPosition = TileController.Info(target).transform.position;
        var prefab = Instantiate(instance.m_PeacemakerPrefab, instance.canvas.transform);

        var scaleDirection = c == CharacterController.Player ? 1 : -1;
        prefab.transform.position = new Vector2(targetPosition.x + 2f * scaleDirection, targetPosition.y + 1.2f);
        prefab.transform.localScale = new Vector3(scaleDirection, 1, 1);

        Destroy(prefab, 2f);
    }

    /// <summary>
    /// Peacemaker shot impacts.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="isBackRow">Determine if shattered tile have to be spawned</param>
    public static void SpawnPeacemakerPopFire(Vector2Int target, bool isBackRow = false)
    {
        var targetPosition = TileController.Info(target).transform.position;
        var prefab = Instantiate(instance.m_PopFire2Prefab, instance.canvas.transform);
        prefab.transform.position = new Vector2(targetPosition.x, targetPosition.y + 0.5f);

        // earthquake panel
        if (isBackRow)
        {
            SpawnEarthquakeTile(target, true, 8f);
        }

        Destroy(prefab, 0.5f);
    }

    public static void SpawnEarthquakeTile(Vector2Int target, bool isBroken, float timeBeforeDestroy)
    {
        var targetPosition = TileController.Info(target).transform.position;
        var prefab = Instantiate(instance.m_EarthquakePrefab, instance.canvas.transform);
        prefab.transform.position = new Vector2(targetPosition.x, targetPosition.y);
        prefab.GetComponent<EarthquakeTile>().InitWithState(target, isBroken, timeBeforeDestroy);
    }
    #endregion
}
