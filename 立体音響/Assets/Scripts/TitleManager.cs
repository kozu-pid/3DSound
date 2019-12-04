using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private Text ipAddressText;
    [SerializeField] private Text portNumText;

    public void MoveMain()
    {
        setIpAddressText();
        setPortNum();
        SceneManager.LoadScene("Main");
    }

    public void MoveClientTest()
    {
        setIpAddressText();
        setPortNum();
        SceneManager.LoadScene("ClientTest");
    }

    private void setIpAddressText()
    {
        if (!string.IsNullOrEmpty(ipAddressText.text))
        {
            DataManager.Instance.IpAddressText = ipAddressText.text;
        }
    }

    private void setPortNum()
    {
        if (!string.IsNullOrEmpty(portNumText.text))
        {
            DataManager.Instance.PortNum = int.Parse(portNumText.text);
        }
    }
}
