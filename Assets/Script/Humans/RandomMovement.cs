using UnityEngine;
using UnityEngine.AI;

public class RandomMovement : MonoBehaviour
{
    public NavMeshAgent agent; // R�f�rence vers le NavMeshAgent de l'agent
    public float wanderRadius = 10f; // Rayon de la zone dans laquelle l'agent peut choisir une nouvelle destination
    public float wanderTime = 5f; // Temps entre chaque changement de destination

    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Initialiser le timer
        timer = wanderTime;

        // S'assurer que l'agent est correctement configur� pour naviguer sur le NavMesh en 2D
        agent.updateRotation = false;  // Pas de rotation automatique pour rester en 2D
        agent.updateUpAxis = false;    // D�sactiver l'axe vertical pour un d�placement 2D
    }

    void Update()
    {
        // Compte le temps avant de choisir une nouvelle destination
        timer += Time.deltaTime;

        // Si le timer d�passe le temps d'attente, une nouvelle destination est choisie
        if (timer >= wanderTime)
        {
            Vector3 newPos = RandomNavmeshLocation(wanderRadius);
            agent.SetDestination(newPos); // D�placer l'agent vers la nouvelle destination
            timer = 0; // R�initialiser le timer
        }
    }

    // M�thode pour trouver une position al�atoire sur le NavMesh dans un rayon donn�
    public Vector3 RandomNavmeshLocation(float radius)
    {
        // G�n�rer une position al�atoire autour de l'agent
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;

        // Trouver la position la plus proche sur le NavMesh
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position; // On s'assure que la position est bien sur le NavMesh
        }

        return finalPosition;
    }
}