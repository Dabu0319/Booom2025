using UnityEngine;

namespace shark{

public class camera_follow : MonoBehaviour
{
    public GameObject player;  // 拖入你的PlayerCharacter
    public Vector3 offset = new Vector3(0f, 0f, -10f);  // Z轴-10让镜头保持在2D/3D游戏中的合理距离
    private void LateUpdate()
    {
        transform.position = player.transform.position + offset;
    }
}
}