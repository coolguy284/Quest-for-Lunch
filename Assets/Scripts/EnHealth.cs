using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using TMPro;

public class EnHealth : MonoBehaviour {
    GameObject Self;
    BoxCollider2D Self_BoxCollider;
    public GridLayout GroundGridLayout;
    public Tilemap GroundTileMap;
    public Image HealthBarImage;
    public TextMeshProUGUI DeadText;
    public TextMeshProUGUI DebugText2;

    public float health = 100.0f;
    public float maxHealth = 100.0f;
    public bool alive = true;
    float invulnTime = 0.0f;
    bool isInvuln = false;

    bool isPlayer = false;

    void changeHealth(float amount) {
        if (!alive) return;
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
        isPlayer = GetComponent<EnMain>().isPlayer;
    }

    void Update() {
        // update state variables
        isPlayer = GetComponent<EnMain>().isPlayer;
        isInvuln = invulnTime > 0.0f;

        // get tile at feet
        var celCoords = GroundGridLayout.WorldToCell(new Vector2(Self.transform.position.x, Self.transform.position.y - Self_BoxCollider.bounds.extents.y));
        var tile = GroundTileMap.GetTile(celCoords);
        var tileAt = tile ? tile.name : "None";
        
        if (alive) {
            // only do damage if vulnerable
            if (!isInvuln) {
                // if tile is spikes do damage
                if (tileAt == "Spikes") {
                    changeHealth(-48.0f);
                    invulnTime = 1.0f;
                }
            }

            // slow heal over time
            changeHealth(10.0f * Time.deltaTime);
        }

        if (isInvuln) {
            invulnTime -= Time.deltaTime;
            if (invulnTime < 0.0f) invulnTime = 0.0f;
        }

        if (isPlayer) DebugText2.text = string.Format("TileCoord: {0}\nTileAt: {1}\nInvulnTime: {2:0.000}", celCoords, tileAt, invulnTime);
    }
}