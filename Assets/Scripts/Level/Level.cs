using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;
using Cinemachine;

public class Level : MonoBehaviour {
    public TextAsset WeaponsJson;

    [System.Serializable]
    public class TWeaponStats {
        public string name;
        public string type;
        public string fires;
        public float damage;
        public float startup;
        public float active;
        public float cooldown;
        public float knockback;
        public float hitstun;
        public float invuln;
        public int[] extraInt;
    }

    [System.Serializable]
    public class WeaponStatsArr {
        public TWeaponStats[] weaponStats;
    }

    WeaponStatsArr baseWeaponsStats;

    [HideInInspector]
    public Dictionary<string, TWeaponStats> WeaponStats = new Dictionary<string, TWeaponStats>();

    public Sprite[] Sprites;
    public GameObject[] Projectiles;
    [HideInInspector]
    public Dictionary<string, Sprite> SpriteDict = new Dictionary<string, Sprite>();
    [HideInInspector]
    public Dictionary<string, GameObject> ProjectileDict = new Dictionary<string, GameObject>();
    public Tilemap GroundTilemap;
    public Tilemap PlatformTilemap;
    public GameObject EnemiesList;
    public GameObject ItemsList;
    public GameObject ProjectilesList;
    public GameObject EnemyPrefab;
    public GameObject[] Rooms;
    public GameObject ItemPrefab;
    public GameObject MainCamera;
    public GameObject DebugTexts;

    IEnumerator PlaceRoom(int roomId, int locX, int locY) {
        // place tiles
        float locXPos = locX / 2f;
        float locYPos = locY / 2f;
        var RoomGroundTilemap = Rooms[roomId].transform.Find("Grid/Ground Tilemap").gameObject.GetComponent<Tilemap>();
        var RoomPlatformTilemap = Rooms[roomId].transform.Find("Grid/Platform Tilemap").gameObject.GetComponent<Tilemap>();
        RoomGroundTilemap.CompressBounds();
        RoomPlatformTilemap.CompressBounds();
        var minX = System.Math.Min(RoomGroundTilemap.cellBounds.xMin, RoomPlatformTilemap.cellBounds.xMin);
        var maxX = System.Math.Max(RoomGroundTilemap.cellBounds.xMax, RoomPlatformTilemap.cellBounds.xMax);
        var minY = System.Math.Min(RoomGroundTilemap.cellBounds.yMin, RoomPlatformTilemap.cellBounds.yMin);
        var maxY = System.Math.Max(RoomGroundTilemap.cellBounds.yMax, RoomPlatformTilemap.cellBounds.yMax);
        for (int y = minY; y < maxY; y++) {
            for (int x = minX; x < maxX; x++) {
                var GroundTile = RoomGroundTilemap.GetTile(new Vector3Int(x, y, 0));
                var PlatformTile = RoomPlatformTilemap.GetTile(new Vector3Int(x, y, 0));
                GroundTilemap.SetTile(new Vector3Int(x + locX, y + locY, 0), GroundTile);
                PlatformTilemap.SetTile(new Vector3Int(x + locX, y + locY, 0), PlatformTile);
            }
        }

        // the tilemap collider does not update immediately so need to wait a frame before placing enemies
        yield return null;

        // place enemies
        var RoomEnemies = Rooms[roomId].transform.Find("Enemies").gameObject.transform;
        for (int i = 0; i < RoomEnemies.childCount; i++) {
            var enemy = RoomEnemies.GetChild(i).gameObject;
            var instantiatedEnemy = Instantiate(EnemyPrefab, new Vector3(locXPos, locYPos, 0.0f) + enemy.transform.position, Quaternion.identity);
            instantiatedEnemy.transform.parent = EnemiesList.transform;
            instantiatedEnemy.name = "Enemy " + i;
        }
    }

    void Start() {
        // load weapons json
        Assert.IsNotNull(WeaponsJson);
        baseWeaponsStats = JsonUtility.FromJson<WeaponStatsArr>(WeaponsJson.text);

        // parse weapons json
        foreach (var stat in baseWeaponsStats.weaponStats) {
            WeaponStats.Add(stat.name, stat);
        }

        // parse sprites dict
        foreach (var sprite in Sprites) {
            SpriteDict.Add(sprite.name, sprite);
        }

        // parse projectiles dict
        foreach (var prefab in Projectiles) {
            ProjectileDict.Add(prefab.name, prefab);
        }

        // activate debug text in editor mode
        if (Application.isEditor) {
            DebugTexts.SetActive(true);
        }

        // place test room
        StartCoroutine(PlaceRoom(0, -7, -35));
    }

    void Update() {
        if (WeaponStats.Count == 0) {
            // load weapons json
            Assert.IsNotNull(WeaponsJson);
            baseWeaponsStats = JsonUtility.FromJson<WeaponStatsArr>(WeaponsJson.text);

            // parse weapons json
            foreach (var stat in baseWeaponsStats.weaponStats) {
                WeaponStats.Add(stat.name, stat);
            }
        }

        if (SpriteDict.Count == 0) {
            // parse sprites dict
            foreach (var sprite in Sprites) {
                SpriteDict.Add(sprite.name, sprite);
            }
        }

        if (ProjectileDict.Count == 0) {
            // parse projectiles dict
            foreach (var prefab in Projectiles) {
                ProjectileDict.Add(prefab.name, prefab);
            }
        }

        MainCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize = 3.0f / Mathf.Max(MainCamera.GetComponent<Camera>().aspect, 1.77777777777f);
    }
}
