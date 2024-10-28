using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldRessources : MonoBehaviour
{
    public Tilemap waterTilemap;
    public Tilemap groundTilemap;
    public float gridSize = 10f;

    private Dictionary<Vector2Int, List<TreeParameters>> treeGrid = new Dictionary<Vector2Int, List<TreeParameters>>();
    private Dictionary<Vector2Int, Vector3> waterGrid = new Dictionary<Vector2Int, Vector3>();
    private Dictionary<Vector2Int, List<ResourceParameters>> resourceGrid = new Dictionary<Vector2Int, List<ResourceParameters>>();

    public static WorldRessources instance;

    private void Awake()
    {
        instance = this;
    }

    // Register Tree resource
    public void RegisterTree(TreeParameters tree)
    {
        Vector2Int gridPosition = GetGridPosition(tree.transform.position);
        if (!treeGrid.ContainsKey(gridPosition))
        {
            treeGrid[gridPosition] = new List<TreeParameters>();
        }
        treeGrid[gridPosition].Add(tree);
    }

    public void UnregisterTree(TreeParameters tree)
    {
        Vector2Int gridPosition = GetGridPosition(tree.transform.position);
        if (treeGrid.ContainsKey(gridPosition))
        {
            treeGrid[gridPosition].Remove(tree);
            if (treeGrid[gridPosition].Count == 0)
            {
                treeGrid.Remove(gridPosition);
            }
        }
    }

    // Register and unregister water tiles
    public void RegisterWaterTile(Vector3 waterPosition)
    {
        Vector2Int gridPosition = GetGridPosition(waterPosition);
        if (!waterGrid.ContainsKey(gridPosition))
        {
            waterGrid[gridPosition] = waterPosition;
        }
    }

    public void UnregisterWaterTile(Vector3 waterPosition)
    {
        Vector2Int gridPosition = GetGridPosition(waterPosition);
        if (waterGrid.ContainsKey(gridPosition))
        {
            waterGrid.Remove(gridPosition);
        }
    }

    // Register and unregister generic resources
    public void RegisterResource(ResourceParameters resource)
    {
        Vector2Int gridPosition = GetGridPosition(resource.transform.position);
        if (!resourceGrid.ContainsKey(gridPosition))
        {
            resourceGrid[gridPosition] = new List<ResourceParameters>();
        }
        resourceGrid[gridPosition].Add(resource);
    }

    public void UnregisterResource(ResourceParameters resource)
    {
        Vector2Int gridPosition = GetGridPosition(resource.transform.position);
        if (resourceGrid.ContainsKey(gridPosition))
        {
            resourceGrid[gridPosition].Remove(resource);
            if (resourceGrid[gridPosition].Count == 0)
            {
                resourceGrid.Remove(gridPosition);
            }
        }
    }

    // Find nearest tree providing food
    public TreeParameters FindNearestFoodTree(Vector3 position)
    {
        Vector2Int gridPosition = GetGridPosition(position);

        List<TreeParameters> nearbyTrees = new List<TreeParameters>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int gridToCheck = new Vector2Int(gridPosition.x + x, gridPosition.y + y);
                if (treeGrid.ContainsKey(gridToCheck))
                {
                    nearbyTrees.AddRange(treeGrid[gridToCheck]);
                }
            }
        }

        TreeParameters nearestTree = null;
        float nearestDistance = Mathf.Infinity;

        foreach (TreeParameters tree in nearbyTrees)
        {
            if (tree.canGiveFood && !tree.foodHarvested)
            {
                float distance = Vector3.Distance(position, tree.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTree = tree;
                }
            }
        }

        return nearestTree;
    }

    // Find nearest water source
    public Vector3 FindNearestWater(Vector3 position)
    {
        Vector2Int gridPosition = GetGridPosition(position);
        Vector3 nearestWater = Vector3.zero;
        float nearestDistance = Mathf.Infinity;

        int searchRadius = 1;

        // Gradually expand the search
        while (nearestWater == Vector3.zero && searchRadius <= 10)
        {
            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int y = -searchRadius; y <= searchRadius; y++)
                {
                    Vector2Int gridToCheck = new Vector2Int(gridPosition.x + x, gridPosition.y + y);
                    if (waterGrid.ContainsKey(gridToCheck))
                    {
                        Vector3 waterPosition = waterGrid[gridToCheck];
                        float distance = Vector3.Distance(position, waterPosition);
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestWater = waterPosition;
                        }
                    }
                }
            }

            searchRadius++;
        }

        if (nearestWater == Vector3.zero)
        {
            Debug.LogWarning("No water found within search radius");
        }

        return nearestWater;
    }

    // Find nearest resource of a specific type
    public ResourceParameters FindNearestResource(Vector3 position, string resourceType)
    {
        Vector2Int gridPosition = GetGridPosition(position);

        List<ResourceParameters> nearbyResources = new List<ResourceParameters>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int gridToCheck = new Vector2Int(gridPosition.x + x, gridPosition.y + y);
                if (resourceGrid.ContainsKey(gridToCheck))
                {
                    foreach (var resource in resourceGrid[gridToCheck])
                    {
                        if (resource.resourceName == resourceType)
                        {
                            nearbyResources.Add(resource);
                        }
                    }
                }
            }
        }

        ResourceParameters nearestResource = null;
        float nearestDistance = Mathf.Infinity;

        foreach (ResourceParameters resource in nearbyResources)
        {
            float distance = Vector3.Distance(position, resource.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestResource = resource;
            }
        }

        return nearestResource;
    }

    private Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x / gridSize), Mathf.FloorToInt(worldPosition.y / gridSize));
    }
}
