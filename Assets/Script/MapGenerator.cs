using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    Dictionary<int, GameObject> tileset;
    Dictionary<int, GameObject> tileGroups;

    [Header("Tiles")]
    [SerializeField] private GameObject prefabSoil;
    [SerializeField] private GameObject prefabGrass;
    [SerializeField] private GameObject prefabWater;
    [SerializeField] private GameObject prefabMountains;

    [Header("Map Size")]
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;

    List<List<int>> noiseGrid = new List<List<int>>();
    List<List<GameObject>> tileGrid = new List<List<GameObject>>();

    [Header("Noise Parameters")]
    [Range(4, 20)] public float magnification = 7.0f;

    [Header("Islandify")]
    public bool islandify = false;
    [Range(0.1f, 0.99f)] public float islandSize;

    int xOffset = 0; // <- +>
    int yOffset = 0; // v- +^

    void Start()
    {
        CreateTileset();
        CreateTileGroups();
        GenerateMap();
    }

    void CreateTileset()
    {
        tileset = new Dictionary<int, GameObject>();
        tileset.Add(0, prefabSoil);
        tileset.Add(1, prefabGrass);
        tileset.Add(2, prefabWater);
        tileset.Add(3, prefabMountains);
    }

    void CreateTileGroups()
    {
        tileGroups = new Dictionary<int, GameObject>();
        foreach (KeyValuePair<int, GameObject> prefabPair in tileset)
        {
            GameObject tileGroup = new GameObject(prefabPair.Value.name);
            tileGroup.transform.parent = gameObject.transform;
            tileGroup.transform.localPosition = new Vector3(0, 0, 0);
            tileGroups.Add(prefabPair.Key, tileGroup);
        }
    }

    void GenerateMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            noiseGrid.Add(new List<int>());
            tileGrid.Add(new List<GameObject>());

            for (int y = 0; y < mapHeight; y++)
            {
                int tileId = islandify ? GetIdForIsland(x, y) : GetIdUsingPerlin(x, y);
                noiseGrid[x].Add(tileId);
                CreateTile(tileId, x, y);
            }
        }
    }

    int GetIdUsingPerlin(int x, int y)
    {
        float rawPerlin = Mathf.PerlinNoise((x - xOffset) / magnification, (y - yOffset) / magnification);
        float clampPerlin = Mathf.Clamp01(rawPerlin);
        float scaledPerlin = clampPerlin * tileset.Count;

        if (scaledPerlin == tileset.Count)
        {
            scaledPerlin = (tileset.Count - 1);
        }
        return Mathf.FloorToInt(scaledPerlin);
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

    void CreateTile(int tile_id, int x, int y)
    {
        GameObject tilePrefab = tileset[tile_id];
        GameObject tileGroup = tileGroups[tile_id];
        GameObject tile = Instantiate(tilePrefab, tileGroup.transform);

        tile.name = string.Format("tile_x{0}_y{1}", x, y);
        tile.transform.localPosition = new Vector3(x, y, 0);

        tileGrid[x].Add(tile);
    }
}
