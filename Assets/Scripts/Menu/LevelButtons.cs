using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButtons : MonoBehaviour {
    public void PickLevel(int level) {
        SceneManager.LoadScene("Test Scn");
    }

    public void BackToMenu() {
        SceneManager.LoadScene("Title");
    }

    void Update() {
        if (Input.GetButtonDown("Cancel")) {
            SceneManager.LoadScene("Title");
        }
    }
}
