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
    public Slider musicSlider;
    public Slider gameSlider;
    private float musicVol = 0.0f;
    private float gameVol = 0.0f;
    private int resIndex = 0;
    private int qualIndex = 0;
    Resolution[] resolutions;
    

    void Start(){
        gameVol = PlayerPrefs.GetFloat("gameVolume");
        musicVol = PlayerPrefs.GetFloat("musicVolume");
        gameAudio.SetFloat("gameVolume",gameVol);
        musicAudio.SetFloat("musicVolume",musicVol);

        gameAudio.SetFloat("gameVolume",gameVol);
        musicAudio.SetFloat("musicVolume",musicVol);
        resolutionDropdown.set
        musicSlider.value = musicVol;
        gameSlider.value = gameVol;

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
        PlayerPrefs.SetInt("resolutionIndex",resolutionIndex);

    }


    public void SetGameVolume(float gameVolume){
        gameVol = gameVolume;
        PlayerPrefs.SetFloat("gameVolume", gameVolume);
        gameAudio.SetFloat("gameVolume",gameVolume);
    }


    public void SetMusicVolume(float musicVolume){
        musicVol = musicVolume;
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        musicAudio.SetFloat("musicVolume",musicVolume);

    }


    public void setQuality( int qualityIndex){
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("qualityIndex",qualityIndex);
    }


    public void setFullscreen(bool isFullScreen){
        Screen.fullScreen = isFullScreen;
    }

}
