using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelStart : MonoBehaviour {
    public Tilemap GroundTilemap;
    public Tilemap PlatformTilemap;
    public GameObject EnemiesList;
    public GameObject EnemyPrefab;
    public GameObject Room_1;

    IEnumerator PlaceRoom(int roomId, int locX, int locY) {
        float locXPos = locX / 2f;
        float locYPos = locY / 2f;
        var RoomGroundTilemap = Room_1.transform.Find("Grid/Ground Tilemap").gameObject.GetComponent<Tilemap>();
        var RoomPlatformTilemap = Room_1.transform.Find("Grid/Platform Tilemap").gameObject.GetComponent<Tilemap>();
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
        var RoomEnemies = Room_1.transform.Find("Enemies").gameObject.transform;
        for (int i = 0; i < RoomEnemies.childCount; i++) {
            var enemy = RoomEnemies.GetChild(i).gameObject;
            var instantiatedEnemy = Instantiate(EnemyPrefab, new Vector3(locXPos, locYPos, 0.0f) + enemy.transform.position, Quaternion.identity);
            instantiatedEnemy.transform.parent = EnemiesList.transform;
            instantiatedEnemy.name = "Enemy " + i;
        }
    }

    void Start() {
        StartCoroutine(PlaceRoom(1, -7, -35));
    }
}
