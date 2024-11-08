using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using NavMeshPlus.Components;

public class TilemapEditor : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap waterTilemap;

    public TileBase selectedTile;
    [Range(1, 10)] public int brushSize = 1;

    public NavMeshSurface navMeshSurface;
    private Coroutine navMeshRebuildCoroutine;
    private float navMeshRebuildDelay = 2f;

    private void Update()
    {
        if (!IsPointerOverUI() && Input.GetMouseButton(0))
        {
            if(selectedTile != null)
            {
                Vector3Int cellPosition = GetMouseCellPosition();
                PaintTile(cellPosition);
            }
        }
    }

    private void StartNavMeshRebuild()
    {
        if (navMeshRebuildCoroutine != null)
        {
            StopCoroutine(navMeshRebuildCoroutine);
        }
        navMeshRebuildCoroutine = StartCoroutine(WaitAndRebuildNavMesh());
    }

    private IEnumerator WaitAndRebuildNavMesh()
    {
        yield return new WaitForSeconds(navMeshRebuildDelay);

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
    }

    private Vector3Int GetMouseCellPosition()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = waterTilemap.WorldToCell(mouseWorldPosition);
        return cellPosition;
    }

    private void PaintTile(Vector3Int cellPosition)
    {
        RemoveTilesInBrushArea(cellPosition);

        if (selectedTile != null)
        {
            Tilemap targetTilemap = DetermineTargetTilemap(selectedTile);
            if (targetTilemap != null)
            {
                for (int x = -brushSize; x <= brushSize; x++)
                {
                    for (int y = -brushSize; y <= brushSize; y++)
                    {
                        if (IsWithinCircle(x, y, brushSize))
                        {
                            Vector3Int positionToPaint = cellPosition + new Vector3Int(x, y, 0);

                            //If painting water check and remove objects
                            if (selectedTile.name.Contains("Water"))
                            {
                                DestroyObjectsOnTile(positionToPaint);
                                UpdateWaterTile(positionToPaint, true);
                            }

                            targetTilemap.SetTile(positionToPaint, selectedTile);
                        }
                    }
                }
            }
        }
        StartNavMeshRebuild();
    }

    private void RemoveTilesInBrushArea(Vector3Int cellPosition)
    {
        for (int x = -brushSize; x <= brushSize; x++)
        {
            for (int y = -brushSize; y <= brushSize; y++)
            {
                //Only remove tiles within the circular brush area
                if (IsWithinCircle(x, y, brushSize))
                {
                    Vector3Int positionToRemove = cellPosition + new Vector3Int(x, y, 0);
                    RemoveTileIfExists(positionToRemove);
                }
            }
        }
    }

    private void RemoveTileIfExists(Vector3Int cellPosition)
    {
        if (waterTilemap.GetTile(cellPosition) != null)
        {
            waterTilemap.SetTile(cellPosition, null);
            UpdateWaterTile(cellPosition, false);
        }

        if (groundTilemap.GetTile(cellPosition) != null)
        {
            groundTilemap.SetTile(cellPosition, null);
        }
    }

    private void UpdateWaterTile(Vector3Int cellPosition, bool isWaterAdded)
    {
        Vector3 worldPosition = waterTilemap.CellToWorld(cellPosition);
        if (isWaterAdded)
        {
            WorldRessources.instance.RegisterWaterTile(worldPosition);
        }
        else
        {
            WorldRessources.instance.UnregisterWaterTile(worldPosition);
        }
    }

    private Tilemap DetermineTargetTilemap(TileBase tile)
    {
        if (tile.name.Contains("Water"))
        {
            return waterTilemap;
        }
        else
        {
            return groundTilemap;
        }
    }

    private void DestroyObjectsOnTile(Vector3Int cellPosition)
    {
        Vector3 worldPosition = waterTilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0);
        float destructionRadius = brushSize;

        //Destroy trees
        foreach (Transform child in VegetationGenerator.instance.treesParent)
        {
            if (Vector3.Distance(child.position, worldPosition) < destructionRadius)
            {
                ResourceParameters resourceParameters = child.GetComponent<ResourceParameters>();
                if (resourceParameters != null)
                {
                    WorldRessources.instance.UnregisterResource(resourceParameters);
                }
                Destroy(child.gameObject);
            }
        }

        //Destroy ores
        foreach (Transform child in VegetationGenerator.instance.oresParent)
        {
            if (Vector3.Distance(child.position, worldPosition) < destructionRadius)
            {
                ResourceParameters resourceParameters = child.GetComponent<ResourceParameters>();
                if (resourceParameters != null)
                {
                    WorldRessources.instance.UnregisterResource(resourceParameters);
                }
                Destroy(child.gameObject);
            }
        }

        //Find and damage all humans within range
        GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
        foreach (GameObject human in humans)
        {
            if (Vector3.Distance(human.transform.position, worldPosition) < destructionRadius)
            {
                LifeManager lifeManager = human.GetComponent<LifeManager>();
                if (lifeManager != null)
                {
                    lifeManager.TakeDamage(1000);
                }
            }
        }

        //Destroy houses in range
        VillageManager[] villages = FindObjectsOfType<VillageManager>();
        foreach (VillageManager village in villages)
        {
            for (int i = village.houses.Count - 1; i >= 0; i--)
            {
                GameObject house = village.houses[i];
                if (Vector3.Distance(house.transform.position, worldPosition) < destructionRadius)
                {
                    village.houses.RemoveAt(i);
                    Destroy(house);
                }
            }

            //Check if TownHall is within range and destroy the entire village if so
            if (Vector3.Distance(village.transform.position, worldPosition) < destructionRadius)
            {
                village.DestroyVillage();
                break;
            }
        }
    }


    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsWithinCircle(int x, int y, int radius)
    {
        return x * x + y * y <= radius * radius;
    }
}
