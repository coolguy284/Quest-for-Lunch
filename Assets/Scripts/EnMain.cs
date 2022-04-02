using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnMain : MonoBehaviour {
    public bool isPlayer = true;

    public GameObject Player;

    void Update() {
        // press esc to return to title
        if (isPlayer && Input.GetButtonDown("Cancel")) {
            SceneManager.LoadScene("Title");
        }
    }
}
