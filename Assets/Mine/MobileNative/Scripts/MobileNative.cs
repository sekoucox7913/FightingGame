using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

public class MobileNative {

#if UNITY_IOS
	[DllImport("__Internal")]
	static extern string MobileNative_getAppID();

	[DllImport("__Internal")]
	static extern string MobileNative_getAppVersion();

	[DllImport("__Internal")]
	static extern string MobileNative_getAppBuild();

	[DllImport("__Internal")]
	static extern string MobileNative_getAppName();

	[DllImport("__Internal")]
	static extern string MobileNative_getMetaData(string key);

	[DllImport("__Internal")]
	static extern void MobileNative_alert(string title, string message, string button1Label, string button2Label, string button3Label, string callbackGameObject, string callbackMethod);

	[DllImport("__Internal")]
	static extern void MobileNative_shareText(string text);

	[DllImport("__Internal")]
	static extern void MobileNative_shareImage(byte[] image, int length, string text);

	[DllImport ("__Internal")]
	static extern void MobileNative_showApp(string url);

	[DllImport ("__Internal")]
	static extern void MobileNative_showLoading();

	[DllImport ("__Internal")]
	static extern void MobileNative_hideLoading();
#elif UNITY_ANDROID
	static AndroidJavaClass _androidNative;
	static AndroidJavaClass androidNative {
		get {
			if (_androidNative == null) _androidNative = new AndroidJavaClass("mobilenative.MobileNative");
			return _androidNative;
		}
	}
#endif

	static Dictionary<string, object> _localizationDic;
	static Dictionary<string, object> localizationDic {
		get {
			if (_localizationDic == null) {
				_localizationDic = MobileNativeMiniJSON.Json.Deserialize(Resources.Load<TextAsset>("MobileNativeStrings").text) as Dictionary<string, object>;
			}
			return _localizationDic;
		}
	}

	static string GetLocalization(string key) {
		if (localizationDic != null) {
			string systemLanguage;
			if(Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified){
				systemLanguage = SystemLanguage.Chinese.ToString();
			}else{
				systemLanguage = Application.systemLanguage.ToString();
			}

			string lang = localizationDic.ContainsKey(systemLanguage) ? systemLanguage : SystemLanguage.English.ToString();

			var langDetail = localizationDic[lang] as Dictionary<string, object>;
			if (langDetail != null && langDetail.ContainsKey(key)) {
				return langDetail[key] as string;
			}
		}
		return "";
	}

	public static bool debugLog = false;

	static void print(string text) {
		if (debugLog)
		{
			//Debug.Log(text);
		}
	}


#region AppInfo

	// cache values since native calls are expensive
	static string _appBundleID;
	static string _appVersion;
	static string _appBuild;
	static string _appName;

	/// <summary>
	/// The application bundle id.
	/// </summary>
	/// <returns>The application bundle id.</returns>
	public static string appBundleID {
		get {
			if (_appBundleID == null) {
#if UNITY_EDITOR
				_appBundleID = UnityEditor.PlayerSettings.applicationIdentifier;
#elif UNITY_IOS
				_appBundleID =  MobileNative_getAppID();
#elif UNITY_ANDROID
				_appBundleID = androidNative.CallStatic<string>("getAppID");
#endif
			}
			return _appBundleID;
		}
	}

	/// <summary>
	/// The application version.
	/// </summary>
	/// <returns>The application version.</returns>
	public static string appVersion {
		get {
			if (_appVersion == null) {
#if UNITY_EDITOR
				_appVersion = UnityEditor.PlayerSettings.bundleVersion;
#elif UNITY_IOS
				_appVersion = MobileNative_getAppVersion();
#elif UNITY_ANDROID
				_appVersion = androidNative.CallStatic<string>("getAppVersion");
#endif
			}
			return _appVersion ?? "0.0";
		}
	}

	/// <summary>
	/// The application build/version code.
	/// </summary>
	/// <returns>The application build/version code.</returns>
	public static string appBuild {
		get {
			if (_appBuild == null) {
#if UNITY_EDITOR
				_appBuild = UnityEditor.PlayerSettings.bundleVersion;
#elif UNITY_IOS
				_appBuild = MobileNative_getAppBuild();
#elif UNITY_ANDROID
				_appBuild = androidNative.CallStatic<int>("getAppBuild").ToString();
#endif
			}
			return _appBuild ?? "0";
		}
	}

	/// <summary>
	/// The application name.
	/// </summary>
	/// <returns>The application name.</returns>
	public static string appName {
		get {
			if (_appName == null) {
#if UNITY_EDITOR
				_appName = UnityEditor.PlayerSettings.productName;
#elif UNITY_IOS
				_appName =  MobileNative_getAppName();
#elif UNITY_ANDROID
				_appName = androidNative.CallStatic<string>("getAppName");
#endif
			}
			return _appName;
		}
	}

	/// <summary>
	/// Meta data, an xml tag in AndroidManifest.xml on Android or a string value in Info.plist on iOS.
	/// </summary>
	/// <param name="key">The key to the meta data.</param>
	/// <returns>A meta data string.</returns>
	public static string GetMetaData(string key) {
		string data = null;
#if UNITY_EDITOR
		UnityEditor.PlayerSettings.GetPropertyOptionalString(key, ref data);
#elif UNITY_IOS
		data =  MobileNative_getMetaData(key);
#elif UNITY_ANDROID
		data = androidNative.CallStatic<string>("getMetaData", key);
#endif
		return data;
	}

#endregion


#region Alert

	/// <summary>
	/// Alerts/Prompts a message, with a title and up to 3 buttons.
	/// </summary>
	/// <param name="title">A title.</param>
	/// <param name="message">A message.</param>
	/// <param name="button1Label">The label for the first/only button. e.g. "OK".</param>
	/// <param name="button1ClickCallback">A System.Action that will be called when the first/only button was clicked.</param>
	/// <param name="button2Label">The label for the second button. e.g. "Later".</param>
	/// <param name="button2ClickCallback">A System.Action that will be called when the second button was clicked.</param>
	/// <param name="button3Label">The label for the third button. e.g. "Skip".</param>
	/// <param name="button3ClickCallback">A System.Action that will be called when the third button was clicked.</param>
	public static void Alert(
		string title, string message,
		string button1Label, System.Action button1ClickCallback = null,
		string button2Label = null, System.Action button2ClickCallback = null,
		string button3Label = null, System.Action button3ClickCallback = null
	) {
		print("MobileNative::Alert("+title+", "+message+")");

		MobileNativeBehaviour.instance.onClickButton1 = button1ClickCallback;
		MobileNativeBehaviour.instance.onClickButton2 = button2ClickCallback;
		MobileNativeBehaviour.instance.onClickButton3 = button3ClickCallback;
#if UNITY_EDITOR
		print(string.Format("{0}\n{1}\n{2} {3} {4}", title, message, button1Label, button2Label, button3Label));
		Debug.Log(string.Format("{0}\n{1}\n{2} {3} {4}", title, message, button1Label, button2Label, button3Label));
#elif UNITY_IOS
		if (button1Label != null && button2Label != null && button3Label == null) {
			string strTmp = button1Label;
			button1Label = button2Label;
			button2Label = strTmp;
			MobileNativeBehaviour.instance.onClickButton1 = button2ClickCallback;
			MobileNativeBehaviour.instance.onClickButton2 = button1ClickCallback;
		}
		MobileNative_alert(title, message, button1Label, button2Label, button3Label, MobileNativeBehaviour.instance.name, "OnClickAlert");
#elif UNITY_ANDROID
		androidNative.CallStatic("alert", title, message, button1Label, button2Label, button3Label, MobileNativeBehaviour.instance.name, "OnClickAlert");
#endif
	}

#endregion


#region Share

	/// <summary>
	/// Shares a text message.
	/// </summary>
	/// <param name="message">A message.</param>
	public static void ShareMessage(string message) {
#if UNITY_EDITOR
		print("MobileNative::ShareText("+message+")");
#elif UNITY_IOS
		MobileNative_shareText(message);
#elif UNITY_ANDROID
		androidNative.CallStatic("shareText", "", message);
#endif
	}

	/// <summary>
	/// Shares an image with an optional message. But due to a system limitation on Android, the message may not be recognized by all applications.
	/// </summary>
	/// <param name="image">An image.</param>
	/// <param name="message">A message.</param>
	public static void ShareImage(Texture2D image, string message = null) {
		byte[] bytes = image.EncodeToPNG();
#if UNITY_EDITOR
		print("MobileNative::ShareImage("+message+") "+bytes.Length+" bytes");
#elif UNITY_IOS
		MobileNative_shareImage(bytes, bytes.Length, message);
#elif UNITY_ANDROID
		androidNative.CallStatic("shareImage", bytes, "", message, false);
#endif
	}

	/// <summary>
	/// Shares a screenshot with an optional message. But due to a system limitation on Android, the message may not be recognized by all application.
	/// </summary>
	/// <param name="message">A message.</param>
	public static void ShareScreenshot(string message = null) {
		MobileNativeBehaviour.instance.StartCoroutine(CaptureScreenshot(message));
	}

	static IEnumerator CaptureScreenshot(string text = null) {
		yield return new WaitForEndOfFrame();

		var screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		screenshot.ReadPixels(new Rect(0, 0, screenshot.width, screenshot.height), 0, 0, false);
		screenshot.Apply();

		yield return 0;

		ShareImage(screenshot, text);
	}

	/// <summary>
	/// Shares an animated GIF with an optional message. But due to a system limitation on Android, the message may not be recognized by all applications.
	/// </summary>
	/// <param name="image">An image.</param>
	/// <param name="message">A message.</param>
	public static void ShareAnimatedGIF(byte[] image, string message = null) {
#if UNITY_EDITOR
		print("MobileNative::ShareAnimatedGIF("+message+") "+image.Length+" bytes");
#elif UNITY_IOS
		MobileNative_shareImage(image, image.Length, message);
#elif UNITY_ANDROID
		androidNative.CallStatic("shareImage", image, "", message, true);
#endif
	}

#endregion


#region ShowApp

	/// <summary>
	/// Shows an App Store page of an application in a popup on iOS, or application details page on Android.
	/// </summary>
	/// <param name="url">A url of the application in App Store or an Android market.</param>
	public static void ShowApp(string url) {
#if UNITY_EDITOR
		print("MobileNative::ShowApp("+url+")");
#elif UNITY_IOS
		MobileNative_showApp(url);
#elif UNITY_ANDROID
		androidNative.CallStatic("showApp", url);
#endif
	}

#endregion


#region Upgrade

	/// <summary>
	/// Prompts for upgrading to a new version of the application.
	/// If <paramref name="newVersion"/> was omitted, will try to pull from App Store/Google Play. Polled infomation will be cached for a day.
	/// </summary>
	/// <param name="title">A title for the prompt.</param>
	/// <param name="message">A message for the prompt.</param>
	/// <param name="confirmLabel">A label for the comfirm button.</param>
	/// <param name="cancelLabel">A label for the cancel button.</param>
	/// <param name="newVersion">A version to upgrade to.</param>
	/// <param name="url">A url of the application in App Store or an Android market.</param>
	/// <returns>Whether new version available, thus prompt showed.</returns>
	public static bool UpgradeTest(
		string title = null, string message = null, string confirmLabel = null, string cancelLabel = null,
		string newVersion = null, string url = null
	) {
		if (newVersion == null) AppInfo.GetAppInfo();
		if (newVersion == "") newVersion = null;

		newVersion   = newVersion   ?? AppInfo.version;
		url          = url          ?? AppInfo.url;
		title        = title        ?? GetLocalization("UpgradeTitle");
		message      = message      ?? string.Format(GetLocalization("UpgradeMessage"), newVersion);
		confirmLabel = confirmLabel ?? GetLocalization("UpgradeConfirmLabel");
		cancelLabel  = cancelLabel  ?? GetLocalization("UpgradeCancelLabel");

		System.Version v1 = null;
		System.Version v2 = null;
		try {
			v1 = new System.Version(appVersion);
			v2 = new System.Version(newVersion);
		} catch (System.Exception ex) {
			//Debug.LogWarning("Errpr parsing app version: "+ex);
		}

		print("MobileNative::UpgradeTest(appVersion:"+appVersion+" newVersion:"+newVersion+" needUpgrade:"+(v1 < v2)+")");
		if (v1 < v2) {
			Alert(title, message, confirmLabel, () => ShowApp(url), cancelLabel);
			return true;
		}
		return false;
	}

#endregion


#region Loading

	/// <summary>
	/// Shows a loading animation.
	/// </summary>
	public static void ShowLoading() {
#if UNITY_EDITOR
		print("MobileNative::ShowLoading()");
#elif UNITY_IOS
		MobileNative_showLoading();
#elif UNITY_ANDROID
		androidNative.CallStatic("showLoading");
#endif
	}

	/// <summary>
	/// Hides the loading animation.
	/// </summary>
	public static void HideLoading() {
#if UNITY_EDITOR
		print("MobileNative::HideLoading()");
#elif UNITY_IOS
		MobileNative_hideLoading();
#elif UNITY_ANDROID
		androidNative.CallStatic("hideLoading");
#endif
	}

#endregion
}

public class MobileNativeBehaviour : MonoBehaviour {

	static MobileNativeBehaviour _instance;
	public static MobileNativeBehaviour instance {
		get {
			if (_instance == null) {
				_instance = Object.FindObjectOfType<MobileNativeBehaviour>();
				if (_instance == null) {
					var go = new GameObject(typeof(MobileNative).Name);
					_instance = go.AddComponent<MobileNativeBehaviour>();
				}
			}
			return _instance;
		}
	}


	public System.Action onClickButton1;
	public System.Action onClickButton2;
	public System.Action onClickButton3;

	void OnClickAlert(string buttonName) {
		switch (buttonName) {
			case "1":
				if (onClickButton1 != null) onClickButton1();
				break;
			case "2":
				if (onClickButton2 != null) onClickButton2();
				break;
			case "3":
				if (onClickButton3 != null) onClickButton3();
				break;
		}
	}
}

class AppInfo {
#if UNITY_IOS
	const string RESULTS_KEY = "results";
	const string APP_URL_ID_KEY = "trackId";
	const string VERSION_KEY = "version";
	const string WHATS_NEW_KEY = "releaseNotes";
	const string appInfoURLFormat = "http://itunes.apple.com/lookup?bundleId={0}";
	const string appURLFormat = "itms-apps://itunes.apple.com/app/id{0}";
#elif UNITY_ANDROID
	const string RE_VERSION = "<div class=\"content\" itemprop=\"softwareVersion\">(.+?)</div>";
	const string RE_WHATS_NEW = "<div class=\"recent-change\">(.+?)</div>";
	const string appInfoURLFormat = "https://play.google.com/store/apps/details?id={0}";
	const string appURLFormat = "market://details?id={0}";
#endif
	const string UPDATE_DATE_KEY = "app_update_date";


	public static string url {
		get { return PlayerPrefs.GetString("app_url", ""); }
		set { PlayerPrefs.SetString("app_url", value); }
	}
	public static string version {
		set { PlayerPrefs.SetString("app_version", value); }
		get { return PlayerPrefs.GetString("app_version", "0.0"); }
	}
	public static string whatsNew {
		set { PlayerPrefs.SetString("app_whatsNew", value); }
		get { return PlayerPrefs.GetString("app_whatsNew", ""); }
	}

	static Coroutine updateCoroutine;
	public static void GetAppInfo() {
#if UNITY_IOS || UNITY_ANDROID
		long binary = 0;
		long.TryParse(PlayerPrefs.GetString(UPDATE_DATE_KEY), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out binary);
		System.DateTime updateDate = System.DateTime.FromBinary(binary);

		if (updateDate < System.DateTime.Today && updateCoroutine == null) updateCoroutine = MobileNativeBehaviour.instance.StartCoroutine(DoGetAppInfo());
#endif
	}

#if UNITY_IOS || UNITY_ANDROID
	static IEnumerator DoGetAppInfo() {
		var www = new WWW(string.Format(appInfoURLFormat, MobileNative.appBundleID));
		yield return www;

		if (!string.IsNullOrEmpty(www.error)) {
			//Debug.LogWarning("Error fetching app info: "+www.error);
		} else {
			Parse(www.text);
		}

		updateCoroutine = null;
	}

	static void Parse(string info) {
#if UNITY_IOS
		try {
			var data = MobileNativeMiniJSON.Json.Deserialize(info) as Dictionary<string, object>;
			if (data != null) {
				var results = data[RESULTS_KEY] as List<object>;
				if (results != null && results.Count > 0) {
					var result = results[0] as Dictionary<string, object>;

					if (result != null) {
						url = string.Format(appURLFormat, result[APP_URL_ID_KEY]);
						version = result[VERSION_KEY] as string;
						whatsNew = result[WHATS_NEW_KEY] as string;

						PlayerPrefs.SetString(UPDATE_DATE_KEY, System.DateTime.Today.ToBinary().ToString("x"));
					}
				}
			}
		} catch (System.Exception ex) {
			//Debug.LogWarning("Error parsing json: "+ex);
		}
#elif UNITY_ANDROID
		url = string.Format(appURLFormat, MobileNative.appBundleID);

		var ver = new System.Text.RegularExpressions.Regex(RE_VERSION).Match(info).Groups[1].ToString().Trim();
		if (!string.IsNullOrEmpty(ver)) version = ver;

		var changes = "";
		foreach (System.Text.RegularExpressions.Match match in new System.Text.RegularExpressions.Regex(RE_WHATS_NEW).Matches(info)) changes += match.Groups[1] + "\n";
		if (!string.IsNullOrEmpty(changes)) whatsNew = changes;

		if (!string.IsNullOrEmpty(ver)) PlayerPrefs.SetString(UPDATE_DATE_KEY, System.DateTime.Today.ToBinary().ToString("x"));
#endif
	}
#endif
}
