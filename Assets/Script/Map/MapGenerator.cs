using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

[System.Serializable]
public class NoiseTile
{
    public Tile tile;
    [Range(0f, 1f)] public float noiseThreshold;
}

public class MapGenerator : MonoBehaviour
{
    private VegetationGenerator vegetationGenerator; ///EVITER LA REF

    [Header("Tilemap")]
    public Tilemap groundTilemap;
    public Tilemap waterTilemap;

    [Header("Map Size")]
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;

    private float centerX;
    private float centerY;

    [Header("Noise Parameters")]
    [SerializeField, Range(1, 20)] private float noiseScale;
    [SerializeField, Range(1, 10)] private float numOctaves;
    [SerializeField, Range(0f, 1f)] private float persistence;
    [SerializeField, Range(1f, 3f)] private float lacunarity;
    [SerializeField, Range(0, 5000)] private float noiseOffsetX;
    [SerializeField, Range(0, 5000)] private float noiseOffsetY;
    [SerializeField, Range(0.1f, 5f)] private float initialAmplitude;
    [SerializeField, Range(0.1f, 5f)] private float initialFrequency;

    [Header("Islandify")]
    [SerializeField] private bool islandify = false;
    [Range(0.7f, 0.99f)] public float islandSize;

    [Header("Tiles Based on Noise")]
    public List<NoiseTile> noiseTiles;   

    [Header("NavMesh")]
    public NavMeshSurface navMeshSurface;

    private bool mapAlreadyGenerated = false;

    public float NoiseScale
    {
        get => noiseScale;
        set => noiseScale = Mathf.Clamp(value, 1, 20);
    }

    public float NumOctaves
    {
        get => numOctaves;
        set => numOctaves = Mathf.Clamp(Mathf.RoundToInt(value), 1, 10);
    }

    public float Persistence
    {
        get => persistence;
        set => persistence = Mathf.Clamp01(value);
    }

    public float Lacunarity
    {
        get => lacunarity;
        set => lacunarity = Mathf.Clamp(value, 1f, 3f);
    }

    public float NoiseOffsetX
    {
        get => noiseOffsetX;
        set => noiseOffsetX = Mathf.Clamp(Mathf.RoundToInt(value), 0, 5000);
    }

    public float NoiseOffsetY
    {
        get => noiseOffsetY;
        set => noiseOffsetX = Mathf.Clamp(Mathf.RoundToInt(value), 0, 5000);
    }

    public float InitialAmplitude
    {
        get => initialAmplitude;
        set => initialAmplitude = Mathf.Clamp(value, 0.1f, 5f);
    }

    public float InitialFrequency
    {
        get => initialFrequency;
        set => initialFrequency = Mathf.Clamp(value, 0.1f, 5f);
    }

    public bool Islandify
    {
        get => islandify;
        set => islandify = value;
    }


    void Start()
    {
        centerX = mapWidth / 2f;
        centerY = mapHeight / 2f;

        vegetationGenerator = GetComponent<VegetationGenerator>();
    }

    public void GenerateMap()
    {
        if(!mapAlreadyGenerated)
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

            mapAlreadyGenerated = true;
            navMeshSurface.BuildNavMesh();
            vegetationGenerator.GenerateObjects();
        }
        else
        {
            groundTilemap.ClearAllTiles();
            waterTilemap.ClearAllTiles();
            mapAlreadyGenerated = false;

            GenerateMap();
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

        // Return noise value in the island
        if (distanceFromCenter < islandRadius * 0.7f)
        {
            return rawNoise;
        }

        //Lower the noise value in the limit of the island to make it smooth
        else if (distanceFromCenter < islandRadius)
        {
            //Distance of the transition zone
            float distanceFactor = (distanceFromCenter - islandRadius * 0.7f) / (islandRadius * 0.3f);

            //Smooth interpolation
            float smoothFactor = Mathf.SmoothStep(1f, 0f, distanceFactor);

            return rawNoise * smoothFactor;
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
            Vector3Int tilePosition = new Vector3Int(x, y, 0);

            if (noiseValue < noiseTiles[2].noiseThreshold) //Place the 3 water levels in the water tilemap
            {
                waterTilemap.SetTile(tilePosition, selectedTile);

                // Register the water tile in WorldRessources
                WorldRessources.instance.RegisterWaterTile(waterTilemap.CellToWorld(tilePosition));
            }
            else //Place the other levels in the ground tilemap
            {
                groundTilemap.SetTile(tilePosition, selectedTile);
            }
        }
    }

}