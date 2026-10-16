using UnityEngine;

public class ChairAnomaly : MonoBehaviour
{
    public Transform player;

    public float detectionRange = 10f;
    public float moveSpeed = 3f;
    public float lookThreshold = 0.8f;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        float distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange)
            return;

        if (!IsPlayerLookingAtMe())
        {
            MoveTowardPlayer();
        }
    }

    bool IsPlayerLookingAtMe()
    {
        Vector3 directionToObject =
            (transform.position - player.position).normalized;

        float dot =
            Vector3.Dot(player.forward, directionToObject);

        // Player isn't facing the object
        if (dot < lookThreshold)
            return false;

        RaycastHit hit;

        if (Physics.Raycast(
            player.position,
            directionToObject,
            out hit,
            detectionRange))
        {
            // Make sure the first thing the player sees is this object
            return hit.transform == transform;
        }

        return false;
    }

    void MoveTowardPlayer()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );
    }
}
