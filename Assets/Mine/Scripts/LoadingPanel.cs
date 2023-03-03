using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    public static LoadingPanel instance;

    public int totalTask = 0;
    public int taskCompleted = 0;

    public static System.Action OnCompleteTasks;

    private void Awake()
    {
        instance = this;
        instance.gameObject.SetActive(false);
    }

    public void Show()
    {
        instance.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (taskCompleted == totalTask)
        {
            taskCompleted = 0;
            totalTask = 0;
            OnCompleteTasks?.Invoke();
            OnCompleteTasks = null;
            instance.gameObject.SetActive(false);
        }
    }
}
