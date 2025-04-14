using UnityEngine;

public class Dabu10_EnemyTest : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Dabu10_PlayerControllerRotate playerController = other.GetComponent<Dabu10_PlayerControllerRotate>();

            if (playerController != null &&
                playerController.currentState == Dabu10_PlayerControllerRotate.PlayerState.TacticalDash)
            {
                Destroy(gameObject); // Enemy dies
            }
            else
            {
                // Optional: Handle player hit logic here
            }
        }
    }
}