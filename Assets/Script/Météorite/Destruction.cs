using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destruction : MonoBehaviour
{
    [SerializeField] private float destructionRadius;
    [SerializeField] private Transform explosionDispay;
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeMagnitude;
    [SerializeField] private float animationSpeed;

    private CameraController cameraController;
    private Animator animator;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        animator.speed = animationSpeed;
    }

    public void Explode()
    {
        explosionDispay.localScale = new Vector3(destructionRadius, destructionRadius, destructionRadius);
        explosionDispay.gameObject.SetActive(true);

        Vector3 impactPoint = transform.position;
        DestroyObjectsInRadius(impactPoint, destructionRadius);

        if (cameraController != null)
        {
            StartCoroutine(cameraController.CameraShake(shakeDuration, shakeMagnitude));
        }

        //Destroy meteorite and then the entire gameObject (explosion display)
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(DestroyExplosionDisplay());
    }

    IEnumerator DestroyExplosionDisplay()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject.transform.parent.gameObject);
    }

    private void DestroyObjectsInRadius(Vector3 center, float radius)
    {
        //Destroy trees
        foreach (Transform tree in VegetationGenerator.instance.treesParent)
        {
            if (Vector3.Distance(tree.position, center) <= radius)
            {
                ResourceParameters resourceParameters = tree.GetComponent<ResourceParameters>();
                if (resourceParameters != null)
                {
                    WorldRessources.instance.UnregisterResource(resourceParameters);
                }
                Destroy(tree.gameObject);
            }
        }

        //Destroy ores
        foreach (Transform ore in VegetationGenerator.instance.oresParent)
        {
            if (Vector3.Distance(ore.position, center) <= radius)
            {
                ResourceParameters resourceParameters = ore.GetComponent<ResourceParameters>();
                if (resourceParameters != null)
                {
                    WorldRessources.instance.UnregisterResource(resourceParameters);
                }
                Destroy(ore.gameObject);
            }
        }

        //Destroy humans in range
        GameObject[] humans = GameObject.FindGameObjectsWithTag("Human");
        foreach (GameObject human in humans)
        {
            if (Vector3.Distance(human.transform.position, center) <= radius)
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
                if (Vector3.Distance(house.transform.position, center) <= radius)
                {
                    village.houses.RemoveAt(i);
                    Destroy(house);
                }
            }

            //If TownHall is within range, destroy the entire village
            if (Vector3.Distance(village.transform.position, center) <= radius)
            {
                village.DestroyVillage();
            }
        }
    }
}
