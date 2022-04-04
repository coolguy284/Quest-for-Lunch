using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleButtons : MonoBehaviour {
    public GameObject MainMenu;
    public GameObject OptionsMenu;

    public void StartGame() {
        SceneManager.LoadScene("Levels");
    }
    
    public void QuitGame() {
        Application.Quit();
    }

    void Update() {
        if (Input.GetButtonDown("Cancel")) {
            if (OptionsMenu.activeSelf) {
                MainMenu.SetActive(true);
                OptionsMenu.SetActive(false);
            }
        }
    }
}
