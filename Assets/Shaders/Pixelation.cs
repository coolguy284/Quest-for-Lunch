using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Pixelation : MonoBehaviour
{
    public Material material;
    public Camera mainCamera;
    public float PIXEL_SIZE_IN_PIXELS = 5.0f;

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        src.filterMode = FilterMode.Point;
        this.transform.position = mainCamera.transform.position;
        this.transform.rotation = mainCamera.transform.rotation;
        var pixelsWidth = Screen.width / PIXEL_SIZE_IN_PIXELS;
        var pixelsHeight = Screen.height / PIXEL_SIZE_IN_PIXELS;
        material.SetFloat("_pixelsWidth", Mathf.Floor(pixelsWidth));
        material.SetFloat("_pixelsHeight", Mathf.Floor(pixelsHeight));
        Graphics.Blit(src, dest, material);
    }
}
