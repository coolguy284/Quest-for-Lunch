using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour {
    public AudioMixer audioMixer;

    public void SetMainVol(float volume) {
        audioMixer.SetFloat("Main", volume);
    }
}
