using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRecording : MonoBehaviour
{
    AudioClip myclip;
    AudioSource audioSource;
    string micName = "null"; //マイクデバイスの名前
    const int samplingFrequency = 44100; //サンプリング周波数
    const int maxTime_s = 300; //最大録音時間[s]

    private void Start()
    {
        //マイクデバイスを探す
        micName = "Built-in Microphone";
        Debug.Log("recording start");
        // deviceName: "null" -> デフォルトのマイクを指定
        myclip = Microphone.Start(deviceName: micName, loop: false, lengthSec: maxTime_s, frequency: samplingFrequency);
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                break;
            }
        }
        Debug.Log("recording stoped");
        Microphone.End(deviceName: micName);
    }

    private void Update()
    {
        
    }

    public void StartButton()
    {
        Debug.Log("recording start");
        // deviceName: "null" -> デフォルトのマイクを指定
        myclip = Microphone.Start(deviceName: micName, loop: false, lengthSec: maxTime_s, frequency: samplingFrequency);
    }

    public void EndButton()
    {
        if (Microphone.IsRecording(deviceName: micName) == true)
        { 
            Debug.Log("recording stoped");
            Microphone.End(deviceName: micName);
        }
        else
        {
            Debug.Log("not recording");
        }
    }

    public void PlayButton()
    {
        Debug.Log("play");
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = myclip;
        audioSource.Play();
    }
}
