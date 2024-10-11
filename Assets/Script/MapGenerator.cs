using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    Dictionary<int, Tile> tileset;

    [Header("Tiles")]
    [SerializeField] private Tile soilTile;
    [SerializeField] private Tile grassTile;
    [SerializeField] private Tile waterTile;
    [SerializeField] private Tile mountainTile;

    public Tilemap groundTilemap;
    public Tilemap waterTilemap;

    [Header("Map Size")]
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;

    private float centerX;
    private float centerY;

    List<List<int>> noiseGrid = new List<List<int>>();

    [Header("Noise Parameters")]
    [Range(1, 20)] public float noiseScale;
    [Range(1, 10)] public int numOctaves;
    [Range(0.0f, 1.0f)] public float persistence;
    [Range(1.0f, 3.0f)] public float lacunarity;

    [Header("Islandify")]
    public bool islandify = false;
    [Range(0.1f, 0.99f)] public float islandSize;

    int xOffset = 0; // <- +>
    int yOffset = 0; // v- +^

    void Start()
    {
        centerX = mapWidth / 2f;
        centerY = mapHeight / 2f;

        CreateTileset();
        GenerateMap();
    }

    void CreateTileset()
    {
        tileset = new Dictionary<int, Tile>();
        tileset.Add(0, soilTile);
        tileset.Add(1, grassTile);
        tileset.Add(2, waterTile);
        tileset.Add(3, mountainTile);
    }

    void GenerateMap()
    {
        int xOffset = mapWidth / 2;
        int yOffset = mapHeight / 2;

        for (int x = 0; x < mapWidth; x++)
        {
            noiseGrid.Add(new List<int>());

            for (int y = 0; y < mapHeight; y++)
            {
                //tile ID according to the noise
                int tileId = islandify ? GetIdForIsland(x, y) : GetIdUsingPerlin(x, y);
                noiseGrid[x].Add(tileId);

                CreateTile(tileId, x - xOffset, y - yOffset);
            }
        }
    }

    int GetIdUsingPerlin(int x, int y)
    {
        float perlinValue = GetPerlinNoiseWithOctaves(x, y);
        float clampPerlin = Mathf.Clamp01(perlinValue);
        float scaledPerlin = clampPerlin * tileset.Count;

        if (scaledPerlin == tileset.Count)
        {
            scaledPerlin = (tileset.Count - 1);
        }
        return Mathf.FloorToInt(scaledPerlin);
    }

    float GetPerlinNoiseWithOctaves(int x, int y)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float normalizedValue = 0;

        for (int i = 0; i < numOctaves; i++)
        {
            float perlinValue = Mathf.PerlinNoise((x - xOffset) / (noiseScale * frequency), (y - yOffset) / (noiseScale * frequency)) * amplitude;
            total += perlinValue;

            normalizedValue += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / normalizedValue;
    }

    int GetIdForIsland(int x, int y)
    {
        float centerX = mapWidth / 2f;
        float centerY = mapHeight / 2f;

        float distanceFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

        float islandRadius = Mathf.Min(centerX, centerY) * islandSize;

        if (distanceFromCenter < islandRadius)
        {
            return GetIdUsingPerlin(x, y);
        }
        else
        {
            return 2;
        }
    }

    void CreateTile(int tileId, int x, int y)
    {
        Tile tile = tileset[tileId];

        //check if is water or land
        if (tileId == 2)
        {
            waterTilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }
        else
        {
            groundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }
    }
}
