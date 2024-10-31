using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SpawnLife : MonoBehaviour
{
    [SerializeField] private GameObject gameObjectToSpawn;
    [SerializeField] private float spawnBuffer;

    private bool canSpawn = true;
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
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        GameObject agent = Instantiate(gameObjectToSpawn, mousePosition, Quaternion.identity);
        agent.GetComponent<NavMeshAgent>().updateRotation = false;

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

    public void SelectGameObject(GameObject selectedGameObject)
    {
        gameObjectToSpawn  = selectedGameObject;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
