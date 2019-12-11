using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WWUtils.Audio;

public class ConvertTest : MonoBehaviour
{
    [SerializeField] private AudioSource audio;
    // Start is called before the first frame update
    void Start()
    {
        WAV wav = new WAV("Assets/Sound/Ring.wav");
        Debug.Log(wav);
        AudioClip audioClip = AudioClip.Create("testSound", wav.SampleCount, 1, wav.Frequency, false);
        audioClip.SetData(wav.LeftChannel, 0);
        audio.clip = audioClip;
        audio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
