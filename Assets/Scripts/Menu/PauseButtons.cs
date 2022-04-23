using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseButtons : MonoBehaviour {
    public GameObject PauseMenu;
    public GameObject PauseMainMenu;
    public GameObject OptionsMenu;
    public GameObject TutorialMenu;
    public Transform PauseSlots;
    [HideInInspector]
    public bool gamePaused = false;
    GameObject Player;

    public void Pause() {
        PauseMenu.SetActive(true);
        Player.GetComponent<EnItem>().DisplaySlots(PauseSlots);
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
        SceneManager.LoadScene("Title");
    }

    void Start() {
        Player = GameObject.Find("Player");
    }

    void Update() {
        if (Player == null) Player = GameObject.Find("Player");
        // press esc to return to title
        if (Input.GetButtonDown("Cancel")) {
            if (PauseMenu.activeSelf) {
                if (OptionsMenu.activeSelf) {
                    PauseMainMenu.SetActive(true);
                    OptionsMenu.SetActive(false);
                } else if (TutorialMenu.activeSelf) {
                    PauseMainMenu.SetActive(true);
                    TutorialMenu.SetActive(false);
                } else {
                    Resume();
                }
            } else {
                Pause();
            }
        }
    }
}
