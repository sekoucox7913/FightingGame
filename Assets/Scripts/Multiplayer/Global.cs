using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Define global variables
public class Global : MonoBehaviour
{
    public static string UserName = ""; //Player name
    public static CharacterType MyCT = CharacterType.Player; // Current charcter type
    public static int playmode = 0; //0 : PVE, 1: PVP, 2: Tournament

    public static bool IsPausePlay = false;
    public static double TimePaused = 0f; 
    public static double LengthPaused = 0f;

    public static bool IsMaster = true;
    public static bool IsGameStarted = false;
}