using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;

public class EnHealth : MonoBehaviour {
    float SPIKES_DAMAGE = 48.0f;
    float SPIKES_INVULN = 1.8f;
    [HideInInspector]
    public float DODGE_INVULN = 0.5f;

    GameObject Self;
    BoxCollider2D Self_BoxCollider;
    GridLayout GroundGridLayout;
    Tilemap GroundTileMap;
    Image HealthBarImage;
    TextMeshProUGUI DeadText;
    TextMeshProUGUI DebugText2;

    public float health = 100.0f;
    public float maxHealth = 100.0f;
    public bool alive = true;
    [HideInInspector]
    public float invulnTime = 0.0f;

    bool isPlayer = false;

    public void changeHealth(float amount) {
        if (!alive || invulnTime > 0.0f && amount < 0.0f) return;
        health = Mathf.Min(Mathf.Max(health + amount, 0.0f), maxHealth);
        if (isPlayer) HealthBarImage.fillAmount = health / maxHealth;
        if (health == 0.0f) {
            alive = false;
            if (isPlayer) {
                DeadText.enabled = true;
            } else {
                Destroy(Self);
            }
        } else if (!alive) {
            alive = true;
            if (isPlayer) {
                DeadText.enabled = false;
            }
        }
    }

    void Start() {
        Self = this.gameObject;
        Self_BoxCollider = GetComponent<BoxCollider2D>();
        GroundGridLayout = GameObject.Find("Grid").GetComponent<GridLayout>();
        GroundTileMap = GameObject.Find("Ground Tilemap").GetComponent<Tilemap>();
        HealthBarImage = GameObject.Find("Health Bar").GetComponent<Image>();
        DeadText = GameObject.Find("Dead Text").GetComponent<TextMeshProUGUI>();
        DebugText2 = GameObject.Find("Debug Text 2").GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        if (Time.timeScale == 0.0f) return;
        // update state variables
        isPlayer = GetComponent<EnMain>().isPlayer;

        // get tile at feet
        var celCoords = GroundGridLayout.WorldToCell(new Vector2(Self.transform.position.x, Self.transform.position.y - Self_BoxCollider.bounds.extents.y));
        var tile = GroundTileMap.GetTile(celCoords);
        var tileAt = tile ? tile.name : "None";
        
        if (alive) {
            // only do damage if vulnerable
            if (invulnTime == 0.0f) {
                // if tile is spikes do damage
                if (tileAt == "Spikes") {
                    changeHealth(-SPIKES_DAMAGE);
                    invulnTime = SPIKES_INVULN;
                }
            }
        }

        // reduce invuln time by time passed
        if (invulnTime > 0.0f) {
            invulnTime -= Time.deltaTime;
            if (invulnTime < 0.0f) invulnTime = 0.0f;
        }

        // debug text
        if (isPlayer) DebugText2.text = string.Format("TileCoord: {0}\nTileAt: {1}\nInvulnTime: {2:0.000}", celCoords, tileAt, invulnTime);
    }
}
