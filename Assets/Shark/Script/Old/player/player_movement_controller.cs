using UnityEngine;

namespace shark{

public class MovementController : MonoBehaviour
{
    [Header("Basic Movement")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float extraSpeedDuration = 5f;
    [SerializeField] private float extraSpeedDecayRate = 1f;
    //###################测试停止行动后移速变化用
    [SerializeField] private int stopMode = 0;


    [Header("Dash Settings")]
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private AnimationCurve dashSpeedCurve = 
        new AnimationCurve(new Keyframe(0, 1.5f), new Keyframe(0.5f, 1f));

    [SerializeField] private bool resetVelocityOnDash = true;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRigidbody;
    
    // 运行时状态
    private float currentSpeed = 0;
    private float extraSpeed = 0;
    private float moveSpeed;
    private Vector2 currentVelocity;
    private Vector2 dashDirection;
    private Vector2 playerDirection = Vector2.right;
    private float dashTimer = 0f;
    private float cooldownTimer = 0f;
    private float extraSpeedTimer = 0f;
    private bool isDashing = false;
    private bool isMoving = false;
    private bool perfecrtDash = false;

    
    // 公开属性
    public bool IsDashing => isDashing;
    public float DashCooldownProgress => Mathf.Clamp01(cooldownTimer / dashCooldown);

    private void Update()
    {
        HandleDashInput();
    }

    private void FixedUpdate()
    {
        UpdateSpeed();
        ExtraSpeedDecay();
        StopMovingDecay();
        if (isDashing)
        {
            UpdateDash();
        }
        else
        {
            UpdateNormalMovement();
        }
        
        UpdateTimers();
    }

    private void UpdateSpeed(){
        moveSpeed = baseSpeed + extraSpeed;
    }

    private void UpdateNormalMovement()
    {
        //###################测试用代码#####################
        currentSpeed = moveSpeed;

        // 获取WASD输入
        Vector2 inputDirection = new Vector2(
            Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0,
            Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0
        ).normalized;

        if (inputDirection.magnitude > 0.1f)
        {
            playerDirection = inputDirection;
            //###################测试停下时衰减模式用#################  
            isMoving = true;
        }else{
            isMoving = false;
        }
        
        // 计算目标速度
        Vector2 targetVelocity = inputDirection * moveSpeed;

        // 平滑移动过渡
        currentVelocity = inputDirection.magnitude > 0.1f 
            ? Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime)
            : Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);

        playerRigidbody.linearVelocity = currentVelocity;
    }

    //实现一段时间后额外移速开始衰减
    private void ExtraSpeedDecay(){
        if(extraSpeedTimer > 0){
            extraSpeedTimer -= Time.deltaTime;
        }
        if(extraSpeed > 0 && extraSpeedTimer <= 0){
            extraSpeed -= extraSpeedDecayRate * Time.deltaTime;
        }
        if(extraSpeed < 0){
            extraSpeed = 0;
        }
    }

    //实现停止移动后的移速变化，0无变化，1立刻开始衰减，2额外移速立刻归零
    private void StopMovingDecay()
    {
        if(isMoving == false && isDashing == false)
        {
            if(stopMode == 0){

            }else if(stopMode == 1){
                extraSpeed -= extraSpeedDecayRate * Time.deltaTime;
                if(extraSpeedTimer <= 0)
                {
                    extraSpeedTimer = 0.2f;
                }
            }else if(stopMode == 2){
                extraSpeed = 0;
            }            
        }
    }


    private void HandleDashInput()
    {
        // 冷却期间不响应输入
        if (cooldownTimer > 0 || isDashing) return;
        
        // 检测冲刺输入
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = 0f;
        
        // 确定冲刺方向（优先使用当前输入方向，若无输入则使用当前面向方向）
        Vector2 inputDirection = new Vector2(
            Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0,
            Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0
        );



        dashDirection = inputDirection.magnitude > 0.1f 
            ? inputDirection.normalized 
            : playerDirection;

        // 如果需要，重置当前速度
        if (resetVelocityOnDash)
        {
            currentVelocity = Vector2.zero;
        }
    }

    private void UpdateDash()
    {
        dashTimer += Time.fixedDeltaTime;
        
        // 计算当前冲刺进度（0-1）
        float dashProgress = Mathf.Clamp01(dashTimer / dashDuration);
        
        // 从曲线获取速度乘数
        float speedMultiplier = dashSpeedCurve.Evaluate(dashProgress);
        
        // 计算当前冲刺速度
        float currentDashSpeed = moveSpeed * speedMultiplier;

        //#####################测试用代码########################
        currentSpeed = currentDashSpeed;
        
        // 应用速度
        playerRigidbody.linearVelocity = dashDirection * currentDashSpeed;
        
        // 检查冲刺是否结束
        if (dashTimer >= dashDuration)
        {
            EndDash();
            if(perfecrtDash == true){
                cooldownTimer = 0.1f;
            }
            perfecrtDash = false;
        }
    }

    private void EndDash()
    {
        isDashing = false;
        cooldownTimer = dashCooldown;
        playerRigidbody.linearVelocity = Vector2.zero;
    }

    //完美冲刺时触发
    public void OnSuccessfulPenetration()
    {
        perfecrtDash = true;
        extraSpeed += 1;
        extraSpeedTimer = extraSpeedDuration;
    }

    //冲刺被打断时触发
    public void OnFailDash()
    {
        extraSpeed = 0;
    }


    private void UpdateTimers()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.fixedDeltaTime;
        }
    }

    public Vector2 GetDirection(){
        return playerDirection;
    }

    public float GetSpeed(){
        return currentSpeed;
    }

    public bool GetDashState(){
        return isDashing;
    }

    public void SetDashCooldownTimer(float value){
        cooldownTimer = value;
    }



    // ################测试用代码，用于数据可视化###################
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Speed: {currentSpeed:F1}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Dash State: {(isDashing ? "Dashing" : "Ready")}");
        if (cooldownTimer > 0)
        {
            GUI.Label(new Rect(10, 50, 300, 20), $"Cooldown: {cooldownTimer:F1}s");
        }
        GUI.Label(new Rect(10, 70, 300, 20), $"Direction: {playerDirection:F1}");
    }



}
}

