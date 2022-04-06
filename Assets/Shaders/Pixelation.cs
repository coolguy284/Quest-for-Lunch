using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pixelation : MonoBehaviour
{
    public Material material;
    public Camera mainCamera;
    public float PIXEL_SIZE_IN_PIXELS = 100.0f;

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        this.transform.position = mainCamera.transform.position;
        this.transform.rotation = mainCamera.transform.rotation;
        var pixelsWidth = Screen.width / PIXEL_SIZE_IN_PIXELS;
        var pixelsHeight = Screen.height / PIXEL_SIZE_IN_PIXELS;
        material.SetFloat("_pixelsWidth", pixelsWidth);
        material.SetFloat("_pixelsHeight", pixelsHeight);
        Graphics.Blit(src, dest, material);
    }
}
