using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class DataManager : SingletonManager<DataManager>
{
    [SerializeField] private string ipAddressText;
    [SerializeField] private int portNum;

    private void Start()
    {
        ipAddressText = "10.2.229.133";
        portNum = 10005;
    }

    public string IpAddressText
    {
        get { return ipAddressText; }
        set { ipAddressText = value; }
    }

    public int PortNum
    {
        get { return portNum; }
        set { portNum = value; }
    }
}
