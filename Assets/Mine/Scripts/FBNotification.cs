using Firebase.Extensions;
using LitJson;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

// Handler for UI buttons on the scene.  Also performs some
// necessary setup (initializing the firebase app, etc) on
// startup.
public class FBNotification : MonoBehaviour
{
    private string logText = "";
    const int kMaxLogSize = 16382;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;
    private string topic = "TestTopic";

    public static FBNotification instance;
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

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            DebugLog(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            DebugLog(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string errorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    errorCode = String.Format("Error.{0}: ",
                      ((Firebase.Messaging.Error)firebaseEx.ErrorCode).ToString());
                }
                DebugLog(errorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            DebugLog(operation + " completed");
            complete = true;

            if (operation == "SubscribeAsync")
            {
                //StartCoroutine(DeliverMessage(topic));
            }
            else if (operation == "RequestPermissionAsync")
            {
                if (!PlayerPrefs.HasKey("token"))
                {
                    DeleteToken();
                }
                else if(PlayerPrefs.GetString("token") == "")
                {
                    DeleteToken();
                }
            }
            else if(operation == "DeleteTokenAsync")
            {
                GetToken();
            }
        }
        return complete;
    }


    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    protected virtual void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Setup message event handlers.
    void InitializeFirebase()
    {
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic).ContinueWithOnMainThread(task => {
            LogTaskCompletion(task, "SubscribeAsync");
        });
        DebugLog("Firebase Messaging Initialized");

        // This will display the prompt to request permission to receive
        // notifications if the prompt has not already been displayed before. (If
        // the user already responded to the prompt, thier decision is cached by
        // the OS and can be changed in the OS settings).
        Firebase.Messaging.FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(
          task => {
              LogTaskCompletion(task, "RequestPermissionAsync");
          }
        );
        isFirebaseInitialized = true;
    }

    public virtual void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        DebugLog("Received a new message");
        var notification = e.Message.Notification;
        if (notification != null)
        {
            DebugLog("title: " + notification.Title);
            DebugLog("body: " + notification.Body);
            var android = notification.Android;
            if (android != null)
            {
                DebugLog("android channel_id: " + android.ChannelId);
            }
        }
        if (e.Message.From.Length > 0)
            DebugLog("from: " + e.Message.From);
        if (e.Message.Link != null)
        {
            DebugLog("link: " + e.Message.Link.ToString());
        }
        if (e.Message.Data.Count > 0)
        {
            DebugLog("data:");
            foreach (System.Collections.Generic.KeyValuePair<string, string> iter in
                     e.Message.Data)
            {
                DebugLog("  " + iter.Key + ": " + iter.Value);
            }
        }

        ServerManager.instance.ProcessMessage(notification.Title, notification.Body);
    }

    public virtual void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        DebugLog("Received Registration Token: " + token.Token);
        PlayerPrefs.SetString("token", token.Token);
    }

    public void ToggleTokenOnInit()
    {
        bool newValue = !Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled;
        Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = newValue;
        DebugLog("Set TokenRegistrationOnInitEnabled to " + newValue);
    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update()
    {
    }

    // End our messaging session when the program exits.
    public void OnDestroy()
    {
        Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
        Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
    }

    public IEnumerator DeliverMessage(string token)
    {
        WWWForm webForm = new WWWForm();

        webForm.AddField("recipient", token);
        webForm.AddField("title", "Alert");
        webForm.AddField("body", "Hi Notification");

        using (UnityWebRequest web = UnityWebRequest.Post(ServerManager.instance.m_Server + "SendNotification.php", webForm))
        {
            yield return web.SendWebRequest();

            if (web.error == null)
            {
            }
            else
            {
                // You can add follow up code here if the message is send successfully
            }
        }
    }


    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s)
    {
        print(s);
        logText += s + "\n";

        while (logText.Length > kMaxLogSize)
        {
            int index = logText.IndexOf("\n");
            logText = logText.Substring(index + 1);
        }
    }

    public void Subscribe()
    {
        {
            Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic).ContinueWithOnMainThread(
              task => {
                  LogTaskCompletion(task, "SubscribeAsync");
              }
            );
            DebugLog("Subscribed to " + topic);
        }
    }

    public void Unsubscribe()
    {
        {
            Firebase.Messaging.FirebaseMessaging.UnsubscribeAsync(topic).ContinueWithOnMainThread(
              task => {
                  LogTaskCompletion(task, "UnsubscribeAsync");
              }
            );
            DebugLog("Unsubscribed from " + topic);
        }
    }

    public void ToggleToken()
    {
        ToggleTokenOnInit();
    }
    public void GetToken()
    {
        {
            String token = "";
            Firebase.Messaging.FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(
              task => {
                  token = task.Result;
                  LogTaskCompletion(task, "GetTokenAsync");
              }
            );
            DebugLog("GetTokenAsync " + token);
        }
    }
    public void DeleteToken()
    {
        {
            Firebase.Messaging.FirebaseMessaging.DeleteTokenAsync().ContinueWithOnMainThread(
              task => {
                  LogTaskCompletion(task, "DeleteTokenAsync");
              }
            );
        }
    }
}
