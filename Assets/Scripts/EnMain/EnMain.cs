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
        public float extraSizeGap = 0.04f;
    }
    
    public class Inputs {
        public float horizontal = 0.0f;
        public float vertical = 0.0f;
        public bool jump = false;
        public bool jumpRelease = false;
        bool _jumpDone = false;
        bool _jumpReleaseDone = false;
        public bool jumpHeld = false;
        public bool dodge = false;
        public bool attack1 = false;
        public bool attack2 = false;
        public bool attackTele = false;
        public bool pickupItem = false;
        public bool dropItem = false;
        
        public void Update() {
            if (jumpHeld) {
                _jumpReleaseDone = false;
                jumpRelease = false;
                if (!_jumpDone && !jump) {
                    jump = true;
                    _jumpDone = true;
                } else {
                    jump = false;
                }
            } else {
                _jumpDone = false;
                jump = false;
                if (!_jumpReleaseDone && !jumpRelease) {
                    jumpRelease = true;
                    _jumpReleaseDone = true;
                } else {
                    jumpRelease = false;
                }
            }
        }
    }
    
    public Bounds bounds = new Bounds();
    public Inputs inputs = new Inputs();
    public TextMeshProUGUI SusCoinsText;
    public bool isPlayer = true;
    [HideInInspector]
    public int susCoins = 0;
    [HideInInspector]
    public bool haltMotion = false;
    [HideInInspector]
    public GameObject Player;
    Level LevelComponent;
    [HideInInspector]
    public Dictionary<string, Sprite> SpriteDict;
    [HideInInspector]
    public Dictionary<string, GameObject> ProjectileDict;
    [HideInInspector]
    public GameObject ItemsList;
    [HideInInspector]
    public GameObject ProjectilesList;
    [HideInInspector]
    public GameObject ItemPrefab;
    [HideInInspector]
    public Dictionary<string, Level.TWeaponStats> WeaponStats;

    void Start() {
        Player = GameObject.Find("Player");
        LevelComponent = GameObject.Find("Main Level Script").GetComponent<Level>();
        SpriteDict = LevelComponent.SpriteDict;
        ProjectileDict = LevelComponent.ProjectileDict;
        ItemsList = LevelComponent.ItemsList;
        ProjectilesList = LevelComponent.ProjectilesList;
        ItemPrefab = LevelComponent.ItemPrefab;
        WeaponStats = LevelComponent.WeaponStats;
    }

    void Update() {
        if (Player == null) Player = GameObject.Find("Player");
        if (LevelComponent == null) LevelComponent = GameObject.Find("Main Level Script").GetComponent<Level>();
        if (SpriteDict == null) SpriteDict = LevelComponent.SpriteDict;
        if (ProjectileDict == null) ProjectileDict = LevelComponent.ProjectileDict;
        if (ItemsList == null) ItemsList = LevelComponent.ItemsList;
        if (ProjectilesList == null) ProjectilesList = LevelComponent.ProjectilesList;
        if (ItemPrefab == null) ItemPrefab = LevelComponent.ItemPrefab;
        if (WeaponStats == null) WeaponStats = LevelComponent.WeaponStats;
    }
}
