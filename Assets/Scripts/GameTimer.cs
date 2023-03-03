using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using Photon.Pun;

public class GameTimer : MonoBehaviour
{
    [SerializeField]
    private float maxTime = default;
    private float startTime = default;

    [SerializeField]
    private TMP_Text timerValue = null;

    public bool timerStarted = false;

    private void OnEnable()
    {
        timerStarted = false;
    }
    public void StartTimer()
    {
        GameController.PauseScreen.SetActive(false);
        GameController.ReconnectScreen.SetActive(false);
        TimeSpan timeSpan = TimeSpan.FromSeconds(maxTime);
        timerValue.text = timeSpan.ToString(@"mm\:ss");
        startTime = Time.time;
        startTime_Pun = PhotonNetwork.Time;
        timerStarted = true;
    }

    private void Update()
    {
        if (!timerStarted)
            return;

        if (GameController.instance.bEnd)
            return;

		if (Global.playmode == 0)
		{
            UpdateOfflineTimer();
		}
		else
		{
            UpdateOnlineTimer();
		}
        //TimeSpan timeSpan = TimeSpan.FromSeconds(maxTime - (Time.time - startTime));
        //timerValue.text = timeSpan.ToString(@"mm\:ss");

        //if (timeSpan.TotalSeconds < 1)
        //{
        //    //if (CharacterController.Player.health > CharacterController.Enemy.health)
        //    //    SceneManager.LoadScene("Win");
        //    //else
        //    //    SceneManager.LoadScene("Lose");
        //    Character myChar = Global.MyCT == CharacterType.Player ? CharacterController.Player : CharacterController.Enemy;
        //    Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;
        //    if (myChar.health > opponentChar.health)
        //        SceneManager.LoadScene("Win");
        //    else
        //        SceneManager.LoadScene("Lose");
    }

    void UpdateOfflineTimer()
	{
        TimeSpan timeSpan = TimeSpan.FromSeconds(maxTime - (Time.time - startTime));
        timerValue.text = timeSpan.ToString(@"mm\:ss");

        if (timeSpan.TotalSeconds < 1)
        {
            ////if (CharacterController.Player.health > CharacterController.Enemy.health)
            ////    SceneManager.LoadScene("Win");
            ////else
            ////    SceneManager.LoadScene("Lose");
            //Character myChar = Global.MyCT == CharacterType.Player ? CharacterController.Player : CharacterController.Enemy;
            //Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;
            //if (myChar.health > opponentChar.health)
            //    SceneManager.LoadScene("Win");
            //else
            //    SceneManager.LoadScene("Lose");
            GameController.instance.EndGame();
        }
    }

    double startTime_Pun = 0f;

    void UpdateOnlineTimer()
	{
		if (Global.IsPausePlay)
		{
            if(Global.playmode == 2)
            {
                if(GameController.WinScreen.activeSelf || GameController.LoseScreen.activeSelf)// || GameController.WinScreen.activeSelf)
                {

                }
                else
                {
                    GameController.ReconnectScreen.SetActive(true);
                }
            }
            else
            {
                GameController.PauseScreen.SetActive(true);
            }
            return;
		}

        if (Global.playmode == 2)
        {
            GameController.ReconnectScreen.SetActive(false);
        }
        else
        {
            GameController.PauseScreen.SetActive(false);
        }

        TimeSpan timeSpan = TimeSpan.FromSeconds(maxTime - (PhotonNetwork.Time - startTime_Pun - Global.LengthPaused));
        timerValue.text = timeSpan.ToString(@"mm\:ss");

        if (timeSpan.TotalSeconds < 1)
        {
            //Character myChar = Global.MyCT == CharacterType.Player ? CharacterController.Player : CharacterController.Enemy;
            //Character opponentChar = Global.MyCT == CharacterType.Player ? CharacterController.Enemy : CharacterController.Player;
            //if (myChar.health > opponentChar.health)
            //    SceneManager.LoadScene("Win");
            //else
            //    SceneManager.LoadScene("Lose");
            //PunManager._Instance.LeaveRoom();
            GameController.instance.EndGame();
        }
    }
}
