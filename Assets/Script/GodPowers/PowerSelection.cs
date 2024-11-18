using UnityEngine;
using UnityEngine.Tilemaps;

public class PowerSelection : MonoBehaviour
{
    [SerializeField] private SpawnLife spawnLife;
    [SerializeField] private TilemapEditor tilemapEditor;

    private void Start()
    {
        spawnLife = GetComponent<SpawnLife>();
        tilemapEditor = GetComponent<TilemapEditor>();
    }

    public void SelectGameObject(GameObject selectedGameObject)
    {
        if (tilemapEditor.selectedTile != null)
        {
            tilemapEditor.selectedTile = null;
        }

        spawnLife.selectedGameObjectToSpawn = selectedGameObject;
    }

    public void SelectTile(TileBase tile)
    {
        if (spawnLife.selectedGameObjectToSpawn != null)
        {
            spawnLife.selectedGameObjectToSpawn = null;
        }

        tilemapEditor.selectedTile = tile;
    }

    public void SetBrushSize(float size)
    {
        tilemapEditor.brushSize = Mathf.RoundToInt(size);
    }
}
