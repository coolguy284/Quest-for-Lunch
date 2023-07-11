using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleButtons : MonoBehaviour {
    public GameObject MainMenu;
    public GameObject OptionsMenu;
    public GameObject TutorialMenu;
    
    public void StartGame() {
        SceneManager.LoadScene("Game");
    }
    
    public void ContinueGame() {
        SceneManager.LoadScene("Game");
    }
    
    public void QuitGame() {
        Application.Quit();
    }
    
    void Update() {
        if (Input.GetButtonDown("Cancel")) {
            if (OptionsMenu.activeSelf) {
                MainMenu.SetActive(true);
                OptionsMenu.SetActive(false);
            } else if (TutorialMenu.activeSelf) {
                MainMenu.SetActive(true);
                TutorialMenu.SetActive(false);
            }
        }
    }
}
