using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                StartCoroutine(SpawnGameObjects());
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
        Instantiate(gameObjectToSpawn, mousePosition, Quaternion.identity);
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
}
