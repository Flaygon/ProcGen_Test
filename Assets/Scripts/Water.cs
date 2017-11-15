using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public float waveStrength;

    public float textureScrollX;
    public float textureScrollY;

    private float scrollX;
    private float scrollY;

    public Texture2D waveTexture;

    public float planeOffsetX;
    public float planeOffsetZ;

    public float worldTilingX;
    public float worldTilingZ;

    public float density = 1000;

    private void Update()
    {
        scrollX += textureScrollX * Time.deltaTime;
        scrollY += textureScrollY * Time.deltaTime;
    }

    public float Sample(Vector3 position)
    {
        return waveTexture.GetPixelBilinear(planeOffsetX + scrollX + position.x / worldTilingX, planeOffsetZ + scrollY + position.z / worldTilingZ).r * waveStrength;
    }
}