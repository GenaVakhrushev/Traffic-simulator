using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainEditor : MonoBehaviour
{
    public Terrain Terrain;
    public int Size = 2;
    public Sprite Form;
    public float ChangingSpeed = 5;
    public Material terrainMaterial;

    private int hmWidth; // heightmap width
    private int hmHeight; // heightmap height
    private float baseChangingSpeed = 0.001f;

    //0 - up, 1 - down, 2 - flat
    private int mode = 0;

    private void Start()
    {
        hmWidth = Terrain.terrainData.heightmapResolution;
        hmHeight = Terrain.terrainData.heightmapResolution;
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        //проверка на нажатие на UI
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        bool hitTerrain = Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("Terrain"));

        DrawBrush(hit.point);

        if (Input.GetMouseButton(0) && !isOverUI)
        {            
            if (hitTerrain)
            {
                ChangeTerrain(hit.point);
            }
        }
    }

    //отобразить кисть на земле
    private void DrawBrush(Vector3 coord)
    {
        terrainMaterial.SetVector("_Position", coord);
        terrainMaterial.SetFloat("_Radius", Size * 2);
    }

    private void ChangeTerrain(Vector3 pos)
    {
        //координаты точки на земле
        Vector2Int terrainPos = GetTerrainCoordinates(pos);
        //высоты земли
        float[,] heights = Terrain.terrainData.GetHeights(0, 0, hmWidth, hmHeight);

        //проход по всем точкам земли на определённой области
        for (int x = terrainPos.x - Size; x < terrainPos.x + Size; x++)
        {
            for (int y = terrainPos.y - Size; y < terrainPos.y + Size; y++)
            {
                //выход за пределы
                if (x < 0 || x >= hmWidth || y < 0 || y >= hmHeight)
                    continue;

                //получить пиксель кисти
                int texX = (int)((x - terrainPos.x + Size) / (2.0f * Size) * Form.texture.width);
                int texY = (int)((y - terrainPos.y + Size) / (2.0f * Size) * Form.texture.height);
                Color color = Form.texture.GetPixel(texX, texY);

                float changingValue = baseChangingSpeed * ChangingSpeed * Time.deltaTime * (1 - color.a);

                //изменить высоту в точке
                if (mode == 0)
                    heights[x, y] += changingValue;
                else if (mode == 1)
                    heights[x, y] -= changingValue;
                else if (mode == 2)
                    heights[x, y] = 0.5f;
            }
        }
                
        Terrain.terrainData.SetHeights(0, 0, heights);
    }

    //получить координаты точки на земле по глобальным координатам
    private Vector2Int GetTerrainCoordinates(Vector3 pos)
    {
        Vector2Int result = new Vector2Int();

        Vector3 terPosition = Terrain.transform.position;
        result.x = (int)((pos.z - terPosition.z) / Terrain.terrainData.size.z * hmHeight);
        result.y = (int)((pos.x - terPosition.x) / Terrain.terrainData.size.x * hmWidth);
       
        return result;
    }

    public void SetForm(Sprite newSprite)
    {
        Form = newSprite;
        terrainMaterial.SetTexture("_DrawTexture", newSprite.texture);
    }

    public void SetMode(int newMode)
    {
        mode = newMode;
    }
}
