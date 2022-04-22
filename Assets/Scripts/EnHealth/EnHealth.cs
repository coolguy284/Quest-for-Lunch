using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;

public class EnHealth : MonoBehaviour {
    GameObject Self;
    BoxCollider2D Self_BoxCollider;
    EnMain EnMainInst;
    GridLayout GroundGridLayout;
    Tilemap GroundTileMap;
    Image HealthBarImage;
    GameObject DeadText;
    TextMeshProUGUI HealthText;
    public TextMeshProUGUI DebugText2;

    float SPIKES_DAMAGE = 48.0f;
    float SPIKES_INVULN = 1.8f;
    [HideInInspector]
    public float DODGE_INVULN = 0.5f;

    public float health = 100.0f;
    public float maxHealth = 100.0f;
    public bool alive = true;
    [HideInInspector]
    public float hitInvulnTime = 0.0f;
    [HideInInspector]
    public float dodgeInvulnTime = 0.0f;
    [HideInInspector]
    public float pastDodgeInvulnTime = 0.0f;
    [HideInInspector]
    public float invulnTime {
        get {
            return Mathf.Max(hitInvulnTime, dodgeInvulnTime);
        }
    }

    bool isPlayer = false;

    public void changeHealth(float amount) {
        if (!alive || invulnTime > 0.0f && amount < 0.0f) return;
        health = Mathf.Min(Mathf.Max(health + amount, 0.0f), maxHealth);
        if (isPlayer) {
            HealthBarImage.fillAmount = health / maxHealth;
            HealthText.text = string.Format("{0:0}/{1:0} ({2:0.0}%)", health, maxHealth, health / maxHealth * 100.0f);
        }
        if (health == 0.0f) {
            alive = false;
            if (isPlayer) {
                DeadText.SetActive(true);
            } else {
                Destroy(Self);
            }
        } else if (!alive) {
            alive = true;
            if (isPlayer) {
                DeadText.SetActive(false);
            }
        }
    }

    void Start() {
        Self = this.gameObject;
        Self_BoxCollider = GetComponent<BoxCollider2D>();
        EnMainInst = GetComponent<EnMain>();
        GroundGridLayout = GameObject.Find("Grid").GetComponent<GridLayout>();
        GroundTileMap = GameObject.Find("Ground Tilemap").GetComponent<Tilemap>();
        HealthBarImage = GameObject.Find("Health Bar").GetComponent<Image>();
        DeadText = GameObject.Find("HUD").transform.Find("Dead Text").gameObject;
        HealthText = GameObject.Find("Health Text").GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        if (Time.timeScale == 0.0f) return;
        
        // update state variables
        if (EnMainInst == null) EnMainInst = GetComponent<EnMain>();
        isPlayer = EnMainInst.isPlayer;

        // get tile at feet
        var feetCoords = new Vector2(Self.transform.position.x, Self.transform.position.y - EnMainInst.bounds.extentY);
        var celCoords = GroundGridLayout.WorldToCell(feetCoords);
        var tile = GroundTileMap.GetTile(celCoords);
        var tileAt = tile ? tile.name : "None";
        
        if (alive) {
            // only do damage if vulnerable
            if (invulnTime == 0.0f) {
                // if tile is spikes and player is actually on top of them do damage
                // negative modulus my beloved
                if (tileAt == "Spikes" && (feetCoords.y % 0.5f + 0.5f) % 0.5f < 0.3f) {
                    changeHealth(-SPIKES_DAMAGE);
                    hitInvulnTime = SPIKES_INVULN;
                }
            }
        }

        // reduce invuln time by time passed
        if (hitInvulnTime > 0.0f) {
            hitInvulnTime -= Time.deltaTime;
            if (hitInvulnTime < 0.0f) hitInvulnTime = 0.0f;
        }

        if (dodgeInvulnTime > 0.0f) {
            dodgeInvulnTime -= Time.deltaTime;
            if (dodgeInvulnTime < 0.0f) dodgeInvulnTime = 0.0f;
        }

        // debug text
        if (isPlayer) DebugText2.text = string.Format("TileCoord: {0}\nTileAt: {1}\nHitInvulnTime: {2:0.000}\nDodgeInvulnTime: {3:0.000}", celCoords, tileAt, hitInvulnTime, dodgeInvulnTime);
    }

    void LateUpdate() {
        pastDodgeInvulnTime = dodgeInvulnTime;
    }
}
