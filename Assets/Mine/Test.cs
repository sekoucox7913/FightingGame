using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public void LeaveButtonClick()
    {
        ServerManager.instance.LeaveApplication();
    }
    public void ResumeButtonClick()
    {
        ServerManager.instance.ResumeApplication();
    }
}
