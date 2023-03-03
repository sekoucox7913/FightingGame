using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectStatus : MonoBehaviour
{
    Text TXT_Status;
    // Start is called before the first frame update
    void Start()
    {
        TXT_Status = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        TXT_Status.text = "Connection Status: " + Photon.Pun.PhotonNetwork.NetworkClientState;
    }
}
