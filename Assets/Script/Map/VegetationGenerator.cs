using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public abstract class SpawnableObjectData
{
    public GameObject prefab;
    [Range(0f, 100f)] public float grassAndSoilChance;
    [Range(0f, 100f)] public float mountainChance;
    [Range(0f, 100f)] public float sandChance;
}

[System.Serializable]
public class TreeData : SpawnableObjectData
{

}

[System.Serializable]
public class OreData : SpawnableObjectData
{

}

public class VegetationGenerator : MonoBehaviour
{
    public static VegetationGenerator instance { get; private set; }

    [Header("Tilemap")]
    public Tilemap groundTilemap;

    [Header("Trees to Spawn")]
    public List<TreeData> trees;
    public Transform treesParent;

    [Header("Ores to Spawn")]
    public List<OreData> ores;
    public Transform oresParent;

    [Header("Spawn Parameters")]
    [Range(0f, 1f)] public float objectDensity;
    [Range(.1f, 5f)] public float noiseScale;

    [Header("Distance Between Objects")]
    [Range(0.5f, 2f)] public float minDistanceBetweenObjects;

    [Header("Map Size")]
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;
    private float centerX;
    private float centerY;

    [Header("Runtime Spawn Settings")]
    [SerializeField] private int runtimeTreeSpawnAmount;
    [SerializeField] private int runtimeOresSpawnAmount;
    [SerializeField] private float spawnIntervalInGameSeconds;

    private List<Vector3> placedObjects = new List<Vector3>();
    private TimeManager timeManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        centerX = mapWidth / 2f;
        centerY = mapHeight / 2f;
        timeManager = GameObject.FindWithTag("Managers").GetComponent<TimeManager>();

        StartCoroutine(RuntimeVegetationGeneration());
    }

    public void GenerateObjects()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x - (mapWidth / 2), y - (mapHeight / 2), 0);
                Tile tile = groundTilemap.GetTile<Tile>(tilePosition);

                if (tile != null && !IsWater(tile))
                {
                    //Get Perlin noise value to determine if an object should be placed
                    float noiseValue = Mathf.PerlinNoise((x / noiseScale), (y / noiseScale));

                    if (noiseValue < objectDensity)
                    {
                        //Place trees
                        PlaceObjectBasedOnTile(tile, tilePosition, trees);
                        //Place ores
                        PlaceObjectBasedOnTile(tile, tilePosition, ores);
                    }
                }
            }
        }
    }

    void PlaceObjectBasedOnTile<T>(Tile tile, Vector3Int position, List<T> objects) where T : SpawnableObjectData
    {
        foreach (T objectData in objects)
        {
            float spawnChance = 0f;

            //Determine spawn chance based on the type of tile
            if (IsGrassOrSoil(tile))
            {
                spawnChance = objectData.grassAndSoilChance;
            }
            else if (IsMountain(tile))
            {
                spawnChance = objectData.mountainChance;
            }
            else if (IsSand(tile))
            {
                spawnChance = objectData.sandChance;
            }

            //Check if the object should spawn based on the calculated chance
            if (Random.Range(0f, 100f) <= spawnChance)
            {
                PlaceObject(objectData.prefab, position, objectData is TreeData);
            }
        }
    }

    void PlaceObject(GameObject prefab, Vector3Int position, bool isTree)
    {
        Vector3 worldPosition = groundTilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0);

        if (CanPlaceObject(worldPosition))
        {
            GameObject obj = Instantiate(prefab, worldPosition, Quaternion.identity);
            if (isTree)
                obj.transform.parent = treesParent;
            else
                obj.transform.parent = oresParent;

            placedObjects.Add(worldPosition);  //Add to placed objects list
        }
    }

    public void DestroyAllObjects()
    {
        foreach (Transform child in treesParent)
        {
            ResourceParameters resourceParameters = child.GetComponent<ResourceParameters>();
            if (resourceParameters != null)
            {
                WorldRessources.instance.UnregisterResource(resourceParameters);
            }
            Destroy(child.gameObject);
        }

        foreach (Transform child in oresParent)
        {
            ResourceParameters resourceParameters = child.GetComponent<ResourceParameters>();
            if (resourceParameters != null)
            {
                WorldRessources.instance.UnregisterResource(resourceParameters);
            }
            Destroy(child.gameObject);
        }

        placedObjects.Clear();
    }

    IEnumerator RuntimeVegetationGeneration()
    {
        while (true)
        {
            float adjustedInterval = spawnIntervalInGameSeconds / timeManager.timeSpeed; //Adjust spawn interval based on timeSpeed
            yield return new WaitForSeconds(adjustedInterval);

            for (int i = 0; i < runtimeTreeSpawnAmount; i++)
            {
                Vector3Int randomTilePosition = GetRandomGroundTilePosition();
                Tile tile = groundTilemap.GetTile<Tile>(randomTilePosition);

                if (tile != null && !IsWater(tile))
                {
                    PlaceObjectBasedOnTile(tile, randomTilePosition, trees);
                }
            }

            for (int i = 0; i < runtimeOresSpawnAmount; i++)
            {
                Vector3Int randomTilePosition = GetRandomGroundTilePosition();
                Tile tile = groundTilemap.GetTile<Tile>(randomTilePosition);

                if (tile != null && !IsWater(tile))
                {
                    PlaceObjectBasedOnTile(tile, randomTilePosition, ores);
                }
            }
        }
    }


    Vector3Int GetRandomGroundTilePosition()
    {
        int x = Random.Range(-mapWidth / 2, mapWidth / 2);
        int y = Random.Range(-mapHeight / 2, mapHeight / 2);
        return new Vector3Int(x, y, 0);
    }

    bool IsGrassOrSoil(Tile tile)
    {
        return tile.name.Contains("Grass") || tile.name.Contains("Soil");
    }

    bool IsSand(Tile tile)
    {
        return tile.name.Contains("Sand");
    }

    bool IsMountain(Tile tile)
    {
        return tile.name.Contains("Mountain");
    }

    bool IsWater(Tile tile)
    {
        return tile.name.Contains("Water");
    }

    bool CanPlaceObject(Vector3 position)
    {
        foreach (Vector3 placedPosition in placedObjects)
        {
            //Avoid objects being too close to each other
            if (Vector3.Distance(position, placedPosition) < minDistanceBetweenObjects)
            {
                return false;
            }
        }
        return true;
    }
}
