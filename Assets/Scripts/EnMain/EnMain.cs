using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EnMain : MonoBehaviour {
    public class Bounds {
        public float extentX = 0.3f;
        public float extentY = 0.6f;
        public float sizeX = 0.6f;
        public float sizeY = 1.2f;
        public float extentXBot = 0.2f;
        public float sizeXBot = 0.4f;
        public float sizeYBot = 1.1f;
        public float extraGap = 0.02f;
    }

    public Bounds bounds;
    public TextMeshProUGUI SusCoinsText;
    public bool isPlayer = true;
    [HideInInspector]
    public int susCoins = 0;
    [HideInInspector]
    public bool haltMotion = false;

    [HideInInspector]
    public GameObject Player;

    void Start() {
        bounds = new Bounds();
        Player = GameObject.Find("Player");
    }

    void Update() {
        if (bounds == null) bounds = new Bounds();
    }
}
