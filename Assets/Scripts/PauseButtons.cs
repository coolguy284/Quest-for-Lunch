using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseButtons : MonoBehaviour {
    public GameObject PauseMenu;
    public GameObject PauseMainMenu;
    public GameObject OptionsMenu;
    [HideInInspector]
    public bool gamePaused = false;

    public void Pause() {
        PauseMenu.SetActive(true);
        Time.timeScale = 0.0f;
        gamePaused = true;
    }

    public void Resume() {
        PauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
        gamePaused = false;
    }

    public void QuitGame() {
        if (PauseMenu.activeSelf) {
            Resume();
        }
        SceneManager.LoadScene("Levels");
    }

    void Update() {
        // press esc to return to title
        if (Input.GetButtonDown("Cancel")) {
            if (PauseMenu.activeSelf) {
                if (OptionsMenu.activeSelf) {
                    PauseMainMenu.SetActive(true);
                    OptionsMenu.SetActive(false);
                } else {
                    Resume();
                }
            } else {
                Pause();
            }
        }
    }
}
