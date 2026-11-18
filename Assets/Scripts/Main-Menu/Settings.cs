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
    public TMP_Dropdown qualityDropdown;
    public Slider musicSlider;
    public Slider gameSlider;
    public Toggle fullScreenToggle;
    private float musicVol = 0.0f;
    private float gameVol = 0.0f;
    private int resIndex = 0;
    private int qualIndex = 0;
    Resolution[] resolutions;
    List<Resolution> cleanSet = new List<Resolution>();

    void Start(){
        //saves the volume levels based off of the previous values
        gameVol = PlayerPrefs.GetFloat("gameVolume");
        musicVol = PlayerPrefs.GetFloat("musicVolume");
        gameAudio.SetFloat("gameVolume",gameVol);
        musicAudio.SetFloat("musicVolume",musicVol);


        gameAudio.SetFloat("gameVolume",gameVol);
        musicAudio.SetFloat("musicVolume",musicVol);
        musicSlider.value = musicVol;
        gameSlider.value = gameVol;

        //populates the list then saves the previous value
        getResolutions();
        if(cleanSet.Count > 0){
            resIndex = PlayerPrefs.GetInt("resolutionIndex");

            if(resIndex >= 0 && resIndex < cleanSet.Count){
                Screen.SetResolution(cleanSet[resIndex].width,cleanSet[resIndex].height,Screen.fullScreen);
                resolutionDropdown.SetValueWithoutNotify(resIndex);
                resolutionDropdown.RefreshShownValue();
            }
            
        }
        //Sets up the quality index based off of the previous value
        qualIndex = PlayerPrefs.GetInt("qualityIndex");
        QualitySettings.SetQualityLevel(qualIndex);
        qualityDropdown.SetValueWithoutNotify(qualIndex);
        qualityDropdown.RefreshShownValue();

        if(PlayerPrefs.GetInt("fullscreenBool") == 1){
            Screen.fullScreen = true;
            fullScreenToggle.isOn = true;

        }else{
            Screen.fullScreen = false;
            fullScreenToggle.isOn = false;
        }
    }


    public void getResolutions(){
        cleanSet.Clear();
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
                cleanSet.Add(resolutions[i]);
                if(cleanSet[cleanSet.Count -1].width == Screen.currentResolution.width && cleanSet[cleanSet.Count -1].height == Screen.currentResolution.height){
                    currentResolutionIndex = cleanSet.Count -1;
                }
            }


        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
        resolutionDropdown.RefreshShownValue();
    }


    public void SetResolution(int resolutionIndex){
        Resolution resolution = cleanSet[resolutionIndex];
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
        qualityDropdown.SetValueWithoutNotify(qualityIndex);
        qualityDropdown.RefreshShownValue();
    }


    public void setFullscreen(bool isFullScreen){
        Screen.fullScreen = isFullScreen;
        if(isFullScreen == false){
            PlayerPrefs.SetInt("fullscreenBool",0);
        }else{
            PlayerPrefs.SetInt("fullscreenBool",1);
        }

    }

}
