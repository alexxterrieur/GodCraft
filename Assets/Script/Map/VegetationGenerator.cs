using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VegetationGenerator : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap groundTilemap;

    [Header("Tree Prefabs")]
    public List<GameObject> grassAndSoilTrees;
    public List<GameObject> sandTrees;

    [Header("Tree Generation Parameters")]
    [Range(0f, .35f)] public float vegetationDensity;
    [Range(.1f, 5f)] public float noiseScale;

    [Header("Distance Between Trees")]
    [Range(0.5f, 5f)] public float minDistanceBetweenTrees;

    [Header("Map Size")]
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;
    private float centerX;
    private float centerY;

    private List<Vector3> placedTrees = new List<Vector3>();

    //Call start function after 1 second so the map is spawn before
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        centerX = mapWidth / 2f;
        centerY = mapHeight / 2f;
        GenerateTrees();
    }

    void GenerateTrees()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                //Get tiles positions
                Vector3Int tilePosition = new Vector3Int(x - (mapWidth / 2), y - (mapHeight / 2), 0);
                Tile tile = groundTilemap.GetTile<Tile>(tilePosition);

                if (tile != null)
                {
                    //check if the noiseScale allows to place a tree on the tile
                    float noiseValue = Mathf.PerlinNoise((x / noiseScale), (y / noiseScale));

                    if (noiseValue < vegetationDensity)
                    {
                        if (IsGrassOrSoil(tile))
                        {
                            PlaceTree(grassAndSoilTrees, tilePosition);
                        }
                        else if (IsSand(tile))
                        {
                            PlaceTree(sandTrees, tilePosition);
                        }
                    }
                }
            }
        }
    }

    bool IsGrassOrSoil(Tile tile)
    {
        return tile.name.Contains("Grass") || tile.name.Contains("Soil");
    }

    bool IsSand(Tile tile)
    {
        return tile.name.Contains("Sand");
    }

    void PlaceTree(List<GameObject> treePrefabs, Vector3Int position)
    {
        if (treePrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, treePrefabs.Count);
            GameObject selectedTree = treePrefabs[randomIndex];

            //Get world position (+ offset to be well placed)
            Vector3 worldPosition = groundTilemap.CellToWorld(position) + new Vector3(0.5f, 0.5f, 0);

            if (CanPlaceTree(worldPosition))
            {
                Instantiate(selectedTree, worldPosition, Quaternion.identity);
                placedTrees.Add(worldPosition);
            }
        }
    }

    bool CanPlaceTree(Vector3 position)
    {
        foreach (Vector3 placedPosition in placedTrees)
        {
            //Avoid trees to be too close
            if (Vector3.Distance(position, placedPosition) < minDistanceBetweenTrees)
            {
                return false;
            }
        }
        return true;
    }
}
