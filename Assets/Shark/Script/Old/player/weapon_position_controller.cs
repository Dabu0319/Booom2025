using UnityEngine;

namespace shark{

public class weapon_position_controller : MonoBehaviour
{
    [Header("Position Info")]
    [SerializeField] private Vector3 position;
    [SerializeField] private Vector2 currentDirect;


    [Header("References")]
    [SerializeField] private player_position_info playerPosition;
    [SerializeField] private Transform weaponTransform;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInfo();
        UpdateWeaponTransform();
    }

    private void UpdateInfo(){
        if (playerPosition == null){
            return;
        }
        position = playerPosition.GetPosition();
        currentDirect = playerPosition.GetDirection();
    }

    private void UpdateWeaponTransform(){
        weaponTransform.position = position;

        // 计算方向的角度（弧度→度）
        float angle = Mathf.Atan2(currentDirect.y, currentDirect.x) * Mathf.Rad2Deg;
        // 应用旋转
        weaponTransform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

}
}
