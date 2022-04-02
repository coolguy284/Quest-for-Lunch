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
    public float invulnTime = 0.0f;

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

        // get tile at feet
        var celCoords = GroundGridLayout.WorldToCell(new Vector2(Self.transform.position.x, Self.transform.position.y - Self_BoxCollider.bounds.extents.y));
        var tile = GroundTileMap.GetTile(celCoords);
        
        // if tile is spikes do damage
        if ((tile ? tile.name : "") == "Spikes") {
            changeHealth(-110.0f * Time.deltaTime);
        }

        // slow heal over time
        changeHealth(10.0f * Time.deltaTime);

        if (isPlayer) DebugText2.text = string.Format("TileCoord: {0}\nTileAt: {1}", celCoords, (tile ? tile.name : ""));
    }
}
