using UnityEngine;
using shark;

namespace TangKK{
public class PlayerDirectionRotator : MonoBehaviour
{
    private PlayerMovementController playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovementController>();
    }

    private void Update()
    {
        if (playerMovement == null) return;

        Vector2 direction = playerMovement.GetDirection();

        if (direction != Vector2.zero)
        {
            // 让角色的 "up" 始终朝向 playerDirection
            transform.up = direction.normalized;
        }
    }
}
}