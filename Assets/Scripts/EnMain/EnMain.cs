using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EnMain : MonoBehaviour {
    public TextMeshProUGUI SusCoinsText;
    public bool isPlayer = true;
    [HideInInspector]
    public int susCoins = 0;
    [HideInInspector]
    public bool haltMotion = false;

    [HideInInspector]
    public GameObject Player;

    void Start() {
        Player = GameObject.Find("Player");
    }
}
