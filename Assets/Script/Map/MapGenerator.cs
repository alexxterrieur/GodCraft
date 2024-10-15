using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class NoiseTile
{
    public Tile tile;
    [Range(0f, 1f)] public float noiseThreshold;
}

public class MapGenerator : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap groundTilemap;
    public Tilemap waterTilemap;

    [Header("Map Size")]
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;

    private float centerX;
    private float centerY;

    [Header("Noise Parameters")]
    [Range(1, 20)] public float noiseScale;
    [Range(1, 10)] public int numOctaves;
    [Range(0f, 1f)] public float persistence;
    [Range(1f, 3f)] public float lacunarity;
    [Range(0, 5000)] public int noiseOffsetX;
    [Range(0, 5000)] public int noiseOffsetY;

    [Range(0.1f, 5f)] public float initialAmplitude;
    [Range(0.1f, 5f)] public float initialFrequency;

    [Header("Tiles Based on Noise")]
    public List<NoiseTile> noiseTiles;

    [Header("Islandify")]
    public bool islandify = false;
    [Range(0.7f, 0.99f)] public float islandSize;

    void Start()
    {
        centerX = mapWidth / 2f;
        centerY = mapHeight / 2f;

        GenerateMap();
    }

    void GenerateMap()
    {
        int xOffset = mapWidth / 2;
        int yOffset = mapHeight / 2;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float noiseValue = islandify ? GetIslandNoiseValue(x, y) : GetPerlinNoiseWithOctaves(x, y);
                CreateTile(noiseValue, x - xOffset, y - yOffset);
            }
        }
    }

    float GetPerlinNoiseWithOctaves(int x, int y)
    {
        float total = 0f;
        float frequency = initialFrequency;
        float amplitude = initialAmplitude;
        float normalizedValue = 0f;

        for (int i = 0; i < numOctaves; i++)
        {
            float perlinValue = Mathf.PerlinNoise((x - noiseOffsetX) / (noiseScale * frequency), (y - noiseOffsetY) / (noiseScale * frequency)) * amplitude;
            total += perlinValue;

            normalizedValue += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total / normalizedValue;
    }

    float GetIslandNoiseValue(int x, int y)
    {
        float rawNoise = GetPerlinNoiseWithOctaves(x, y);

        float distanceFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
        float islandRadius = Mathf.Min(centerX, centerY) * islandSize;

        if (distanceFromCenter < islandRadius)
        {
            return rawNoise;
        }
        else
        {
            return 0;
        }
    }

    void CreateTile(float noiseValue, int x, int y)
    {
        Tile selectedTile = null;

        foreach (NoiseTile noiseTile in noiseTiles)
        {
            if (noiseValue < noiseTile.noiseThreshold)
            {
                selectedTile = noiseTile.tile;
                break;
            }
        }

        //Assign the tile to the correct tilemap
        if (selectedTile != null)
        {
            if (noiseValue < noiseTiles[2].noiseThreshold) //Place the 3 water levels in the water tilemap
            {
                waterTilemap.SetTile(new Vector3Int(x, y, 0), selectedTile);
            }
            else //Place the other levels in the ground tilemap
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), selectedTile);
            }
        }
    }
}