using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SpawnLife : MonoBehaviour
{
    public GameObject selectedGameObjectToSpawn;
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
        if (selectedGameObjectToSpawn != null)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            GameObject agent = Instantiate(selectedGameObjectToSpawn, mousePosition, Quaternion.identity);
            agent.GetComponent<NavMeshAgent>().updateRotation = false;
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

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
