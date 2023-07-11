using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour {
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle debugToggle;
    public GameObject DebugTexts;
    
    Resolution[] resolutionsList;
    bool ignoreSetting = true;
    
    public void SetResolution(int resolutionIndex) {
        if (ignoreSetting) return;
        Resolution resolution = resolutionsList[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    
    public void SetFullscreen(bool fullScreen) {
        if (ignoreSetting) return;
        Screen.fullScreen = fullScreen;
        if (Screen.fullScreen) {
            ignoreSetting = true;
            updateResolutions();
            ignoreSetting = false;
        }
    }
    
    public void SetQuality(int qualityIndex) {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    
    public void SetMainVol(float volume) {
        audioMixer.SetFloat("Main", volume);
    }
    
    public void SetMusicVol(float volume) {
        audioMixer.SetFloat("Music", volume);
    }
    
    public void SetSFXVol(float volume) {
        audioMixer.SetFloat("SFX", volume);
    }
    
    void updateResolutions() {
        resolutionDropdown.ClearOptions();
        List<Resolution> newResolutionsList = new List<Resolution>();
        List<string> resolutionOptions = new List<string>();
        int currentResolutionIndex = 0;
        Resolution workingResolution;
        int screenWidth, screenHeight, screenResolution;
        int oldScreenWidth = 0, oldScreenHeight = 0, maxScreenResolution = 0;
        for (int i = 0; i < Screen.resolutions.Length; i++) {
            workingResolution = Screen.resolutions[i];
            screenWidth = workingResolution.width;
            screenHeight = workingResolution.height;
            screenResolution = workingResolution.refreshRate;
            if (screenWidth == oldScreenWidth && screenHeight == oldScreenHeight) {
                if (screenResolution > maxScreenResolution) {
                    newResolutionsList[newResolutionsList.Count - 1] = workingResolution;
                }
            } else {
                newResolutionsList.Add(workingResolution);
                resolutionOptions.Add(screenWidth + " x " + screenHeight);
                maxScreenResolution = 0;
            }
            oldScreenWidth = screenWidth;
            oldScreenHeight = screenHeight;
            if (screenWidth == Screen.currentResolution.width &&
                screenHeight == Screen.currentResolution.height) {
                currentResolutionIndex = i;
            }
        }
        resolutionsList = newResolutionsList.ToArray();
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    
    void Start() {
        ignoreSetting = true;
        updateResolutions();
        fullscreenToggle.isOn = Screen.fullScreen;
        ignoreSetting = false;
        if (DebugTexts != null && DebugTexts.activeSelf) {
            debugToggle.isOn = true;
        }
    }
}
