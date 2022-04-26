using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTerrain : MonoBehaviour, ISaveable
{
    Terrain terrain;
    int hmWidth; // heightmap width
    int hmHeight; // heightmap height

    private void Start()
    {
        terrain = gameObject.GetComponent<Terrain>();
        hmWidth = terrain.terrainData.heightmapResolution;
        hmHeight = terrain.terrainData.heightmapResolution;
    }

    private void OnApplicationQuit()
    {
        float[,] heights = terrain.terrainData.GetHeights(0, 0, hmWidth, hmHeight);

        for (int i = 0; i < hmWidth; i++)
        {
            for (int j = 0; j < hmHeight; j++)
            {
                heights[i, j] = 0.5f;
            }
        }

        terrain.terrainData.SetHeights(0, 0, heights);
    }

    public byte[] SaveInfo()
    {
        return Helper.ObjectToByteArray(terrain.terrainData.GetHeights(0, 0, hmWidth, hmHeight));
    }

    public void LoadInfo(byte[] info)
    {
        float[,] heights = Helper.ByteArrayToObject(info) as float[,];
        Terrain.activeTerrain.terrainData.SetHeights(0, 0, heights);
    }
}
