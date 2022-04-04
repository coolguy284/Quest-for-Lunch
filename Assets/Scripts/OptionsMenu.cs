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

    public void SetResolution(int resolutionIndex) {
        Resolution resolution = Screen.resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool fullScreen) {
        Screen.fullScreen = fullScreen;
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

    void Start() {
        resolutionDropdown.ClearOptions();
        List<string> resolutionOptions = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < Screen.resolutions.Length; i++) {
            resolutionOptions.Add(Screen.resolutions[i].width + " x " + Screen.resolutions[i].height);
            if (Screen.resolutions[i].width == Screen.currentResolution.width &&
                Screen.resolutions[i].height == Screen.currentResolution.height) {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        fullscreenToggle.isOn = Screen.fullScreen;
    }
}
