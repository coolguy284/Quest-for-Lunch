using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnMain : MonoBehaviour {
    public bool isPlayer = true;

    [HideInInspector]
    public GameObject Player;

    void Start() {
        Player = GameObject.Find("Player");
    }

    void Update() {
        // press esc to return to title
        if (isPlayer && Input.GetButtonDown("Cancel")) {
            SceneManager.LoadScene("Title");
        }
    }
}
