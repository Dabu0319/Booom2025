using UnityEngine;


namespace shark{

public class player_position_info : MonoBehaviour
{
    [Header("Position Info")]
    [SerializeField] private Vector3 position;
    [SerializeField] Vector2 currentDirect;
    
    [Header("References")]
    [SerializeField] private MovementController movementController;
    [SerializeField] private Transform characterTransform;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInfo();
    }

    private void UpdateInfo(){
        if (movementController == null){
            return;
        }
        position = characterTransform.position;
        currentDirect = movementController.GetDirection();
    }

    public Vector2 GetDirection(){
        return currentDirect;
    }

    public Vector2 GetPosition(){
        return position;
    }
}
}
