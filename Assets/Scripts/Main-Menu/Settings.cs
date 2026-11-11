using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class Settings : MonoBehaviour
{
    public AudioMixer gameAudio;
    public AudioMixer musicAudio;

    public void SetGameVolume(float gameVolume){

        gameAudio.SetFloat("gameVolume",gameVolume);

    }

    public void SetMusicVolume(float musicVolume){
        musicAudio.SetFloat("musicVolume",musicVolume);
    }

    public void setQuality( int qualityIndex){
        QualitySettings.SetQualityLevel(qualityIndex);

    }
}
