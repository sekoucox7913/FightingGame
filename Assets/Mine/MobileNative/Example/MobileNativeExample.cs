using UnityEngine;
using System.Collections;

public class MobileNativeExample : MonoBehaviour {

#if UNITY_IOS
	const string appUrl = "https://itunes.apple.com/app/chrome/id535886823";
#elif UNITY_ANDROID
	const string appUrl = "market://details?id=com.android.chrome";
#else
	const string appUrl = "";
#endif
	
	void Start() {
		print("BundleID: "+MobileNative.appBundleID);
		print("Version: "+MobileNative.appVersion);
		print("Build: "+MobileNative.appBuild);
		print("Name: "+MobileNative.appName);
	}

	void OnGUI() {
		if (GUI.Button(new Rect(40, 40, 160, 90), "Show App")) {
			MobileNative.ShowApp(appUrl);
		}
		
		if (GUI.Button(new Rect(40, 140, 160, 90), "Share Message")) {
			MobileNative.ShareMessage("Message @"+System.DateTime.Now);
		}
		
		if (GUI.Button(new Rect(40, 240, 160, 90), "Share Screenshot")) {
			MobileNative.ShareScreenshot("Screenshot @"+System.DateTime.Now);
		}
		
		if (GUI.Button(new Rect(240, 40, 160, 90), "Alert")) {
			MobileNative.Alert("Alert", System.DateTime.Now.ToString(), "OK");
		}
		
		if (GUI.Button(new Rect(240, 140, 160, 90), "2 Button Alert")) {
			MobileNative.Alert("2 Button Alert", System.DateTime.Now.ToString(),
				"OK", () => {print("ok2");},
				"CANCEL", () => {print("cancel2");}
			);
		}
		
		if (GUI.Button(new Rect(240, 240, 160, 90), "3 Button Alert")) {
			MobileNative.Alert("3 Button Alert", System.DateTime.Now.ToString(),
				"OK", () => {print("ok3");},
				"CANCEL", () => {print("cancel3");},
				"OTHER", () => {print("other3");}
			);
		}
		
		if (GUI.Button(new Rect(440, 40, 160, 90), "Upgrade Test")) {
			if (MobileNative.UpgradeTest()) {
				print("Prompt for upgrading.");
			} else {
				print("No new version available.");
			}
		}
		
		if (GUI.Button(new Rect(440, 140, 160, 90), "Custom Upgrade")) {
			MobileNative.UpgradeTest(newVersion: "99.0", url: appUrl);
		}

		if (GUI.Button (new Rect (240, 440, 160, 90), "Show Loading")) {
			MobileNative.ShowLoading();
			StartCoroutine(HideLoading());
		}
	}

	IEnumerator HideLoading() {
		yield return new WaitForSeconds (2);
		MobileNative.HideLoading();
	}
}
