using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Display current game's version
public class ShowVersion : MonoBehaviour
{
    Text TXT_Status;
    // Start is called before the first frame update
    void Start()
    {
        TXT_Status = GetComponent<Text>();

#if !UNITY_EDITOR && UNITY_ANDROID
        TXT_Status.text = GetVersionName() + "(" + GetVersionCode().ToString() + ")";
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#if !UNITY_EDITOR && UNITY_ANDROID

    //int vesioncode =  context().getPackageManager().getPackageInfo(context().getPackageName(), 0).versionCode;
    public static int GetVersionCode()
    {
        AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageMngr = context.Call<AndroidJavaObject>("getPackageManager");
        string packageName = context.Call<string>("getPackageName");
        AndroidJavaObject packageInfo = packageMngr.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
        return packageInfo.Get<int>("versionCode");
    }

    //int versionName =  context().getPackageManager().getPackageInfo(context().getPackageName(), 0).versionName;
    public static string GetVersionName()
    {
        AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageMngr = context.Call<AndroidJavaObject>("getPackageManager");
        string packageName = context.Call<string>("getPackageName");
        AndroidJavaObject packageInfo = packageMngr.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
        return packageInfo.Get<string>("versionName");
    }
#endif
}
