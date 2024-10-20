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

    public void SelectTile(TileBase tile)
    {
        selectedTile = tile;
    }

    public void SetBrushSize(float size)
    {
        brushSize = Mathf.RoundToInt(size);
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
                for (int x = -brushSize / 2; x <= brushSize / 2; x++)
                {
                    for (int y = -brushSize / 2; y <= brushSize / 2; y++)
                    {
                        Vector3Int positionToPaint = cellPosition + new Vector3Int(x, y, 0);
                        targetTilemap.SetTile(positionToPaint, selectedTile);

                        if (targetTilemap == waterTilemap)
                        {
                            UpdateWaterTile(positionToPaint, true);
                        }
                    }
                }
            }
        }

        StartNavMeshRebuild();
    }

    private void RemoveTilesInBrushArea(Vector3Int cellPosition)
    {
        for (int x = -brushSize / 2; x <= brushSize / 2; x++)
        {
            for (int y = -brushSize / 2; y <= brushSize / 2; y++)
            {
                Vector3Int positionToRemove = cellPosition + new Vector3Int(x, y, 0);
                RemoveTileIfExists(positionToRemove);
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

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
