using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class SpawnLife : MonoBehaviour
{
    public GameObject selectedGameObjectToSpawn;
    [SerializeField] private float spawnBuffer;
    [SerializeField] private Tilemap waterTilemap;

    private bool canSpawn = true;
    [SerializeField] private bool canSpawnOnWater;
    private bool isClicking;

    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Started)
        {
            if (!isClicking)
            {
                isClicking = true;
                if (!IsPointerOverUI())
                {
                    StartCoroutine(SpawnGameObjects());
                }                
            }
        }

        if (ctx.phase == InputActionPhase.Canceled)
        {
            isClicking = false;
        }
    }

    private void SpawnGameObject()
    {
        if (selectedGameObjectToSpawn != null)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            if(selectedGameObjectToSpawn.name.Contains("Human"))
            {
                canSpawnOnWater = false;
            }
            else
                canSpawnOnWater = true;

            if(!canSpawnOnWater)
            {
                if (IsOnWaterTile(mousePosition))
                {
                    Debug.Log("Cannot spawn on water");
                    return;
                }
            }            

            GameObject gameObjectToSpawn = Instantiate(selectedGameObjectToSpawn, mousePosition, Quaternion.identity);
            NavMeshAgent agent = GetComponent<NavMeshAgent>();

            if(agent != null )
                agent.updateRotation = false;
        }
    }

    private IEnumerator SpawnGameObjects()
    {
        while (isClicking)
        {
            if (canSpawn)
            {
                SpawnGameObject();
                canSpawn = false;
                yield return new WaitForSeconds(spawnBuffer);
                canSpawn = true;
            }
            yield return null;
        }
    }

    private bool IsOnWaterTile(Vector3 position)
    {
        if (waterTilemap == null)
        {
            Debug.LogError("Water Tilemap is not assigned!");
            return false;
        }

        Vector3Int tilePosition = waterTilemap.WorldToCell(position);
        TileBase tile = waterTilemap.GetTile(tilePosition);

        return tile != null;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
