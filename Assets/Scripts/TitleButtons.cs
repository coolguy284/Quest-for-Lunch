using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleButtons : MonoBehaviour {
    public void StartGame() {
        SceneManager.LoadScene("Test Scn 1 -- Default 1");
    }
    
    public void QuitGame() {
        Application.Quit();
    }
}
