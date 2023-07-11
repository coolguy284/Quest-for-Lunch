using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class Pixelation : MonoBehaviour {
    public Material material;
    Material materialInst;
    public Camera mainCamera;
    public float PIXEL_SIZE = 0.0625f;
    public bool snapToWorld = true;
    
    Vector3 cameraWorldLeftCorner;
    Vector3 cameraWorldRightCorner;
    Vector3 cameraWorldCenter;
    Vector3 cameraWorldSize;
    float pixelWidthInv;
    float pixelHeightInv;
    float pixelXOffset;
    float pixelYOffset;
    
    void Awake() {
        materialInst = new Material(material);
    }
    
    void OnPreRender() {
        cameraWorldLeftCorner = mainCamera.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f));
        cameraWorldRightCorner = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0.0f));
        cameraWorldCenter = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0.0f));
        cameraWorldSize = cameraWorldRightCorner - cameraWorldLeftCorner;
        this.transform.position = new Vector3(
            Mathf.Floor(cameraWorldCenter.x / PIXEL_SIZE) * PIXEL_SIZE,
            Mathf.Floor(cameraWorldCenter.y / PIXEL_SIZE) * PIXEL_SIZE,
            mainCamera.transform.position.z
        );
        this.transform.rotation = mainCamera.transform.rotation;
        pixelWidthInv = cameraWorldSize.x / PIXEL_SIZE;
        pixelHeightInv = cameraWorldSize.y / PIXEL_SIZE;
        pixelXOffset = -(cameraWorldLeftCorner.x - Mathf.Floor(cameraWorldLeftCorner.x / PIXEL_SIZE) * PIXEL_SIZE) / cameraWorldSize.x;
        pixelYOffset = -(cameraWorldLeftCorner.y - Mathf.Floor(cameraWorldLeftCorner.y / PIXEL_SIZE) * PIXEL_SIZE) / cameraWorldSize.y;
        if (Application.isPlaying && SceneManager.GetActiveScene().name == "Render Test") {
            mainCamera.transform.position += new Vector3(0.0004f, 0.0f, 0.0f);
        }
    }
    
    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        src.filterMode = FilterMode.Point;
        materialInst.SetFloat("_pixelWidthInv", pixelWidthInv);
        materialInst.SetFloat("_pixelHeightInv", pixelHeightInv);
        if (snapToWorld) {
            materialInst.SetFloat("_pixelXOffset", pixelXOffset);
            materialInst.SetFloat("_pixelYOffset", pixelYOffset);
        } else {
            materialInst.SetFloat("_pixelXOffset", 0.0f);
            materialInst.SetFloat("_pixelYOffset", 0.0f);
        }
        Graphics.Blit(src, dest, materialInst);
    }
    
    void OnDrawGizmos() {
        return;
    }
}
