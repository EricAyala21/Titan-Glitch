using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    public AudioMixer gameAudio;
    public AudioMixer musicAudio;
    public TMP_Dropdown resolutionDropdown;

    Resolution[] resolutions;
    
    void Start(){
        
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        HashSet<string> uniqueRes = new HashSet<string>();
        int currentResolutionIndex = 0;

        for(int i =0; i < resolutions.Length;i++ )
        {
            string option = resolutions[i].width + "x" +resolutions[i].height;
            if(uniqueRes.Add(option)){
                options.Add(option);
            }
            if(resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height){
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);

        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    public void SetResolution(int resolutionIndex){
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width,resolution.height,Screen.fullScreen);
    }
    public void SetGameVolume(float gameVolume){

        gameAudio.SetFloat("gameVolume",gameVolume);

    }

    public void SetMusicVolume(float musicVolume){
        musicAudio.SetFloat("musicVolume",musicVolume);
    }

    public void setQuality( int qualityIndex){
        QualitySettings.SetQualityLevel(qualityIndex);

    }

    public void setFullscreen(bool isFullScreen){
        Screen.fullScreen = isFullScreen;

    }
}
