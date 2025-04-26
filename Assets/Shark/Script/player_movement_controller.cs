using System.Resources;
using UnityEngine;


namespace shark {

//#############################数据接口################################
//GetDirection():获取角色当前面朝方向,返回vector2
//GetSpeed():获取角色当前速度，返回float
//GetDashState():获取角色当前冲刺类型,返回int，0:未冲刺，1：普通冲刺，2：极限冲刺,3:极限冲刺后摇(攻击判定)
//GetBackwardJumpState():获取是否在后跳过程，返回bool，0：未后跳，1：后跳
//SetDashCooldownTimer(float value):将冲刺当前冲刺剩余冷却时间设置为value。调用该接口请注意需要对应修改该脚本内部逻辑！！！

public class PlayerMovementController : MonoBehaviour
{
    [Header("Basic Movement")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;


    [Header("Dash Settings")]
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 1f;
    //触发极限冲刺所需的按住空格时间
    [SerializeField] private float ultimateDashRequiremnetTimer = 0.3f; 
    [SerializeField] private float maxDashSpeed = 15.0f;
    [SerializeField] private float increaseSpeedPerSec = 5.0f;
    [SerializeField] private float attackRecoveryTimer = 0f;
    //maxAttackRecoveryTime应设置为攻击完整时长
    [SerializeField] private float maxAttackRecoveryTime = 1f;
    [SerializeField] private float backwardJumpTimer = 0f;
    [SerializeField] private float maxBackwardJumpTime = 0.2f;
    [SerializeField] private AnimationCurve dashSpeedCurve = 
        new AnimationCurve(new Keyframe(0, 1.5f), new Keyframe(0.5f, 1f));

    
    [Header("References")]
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private GameObject player;
    
    // 运行时状态
    private float currentSpeed = 0;
    private float extraSpeed = 0;
    private float moveSpeed;
    private Vector2 currentVelocity;
    private Vector2 dashDirection;
    private Vector2 playerDirection = Vector2.right;
    private float dashTimer = 0f;
    private float cooldownTimer = 0f;
    private float pressSpaceTimer = 0f;
    private bool isDashing = false;
    private bool isUltimateDashing = false;
    private bool isStartAttackRecory = false;
    private bool isBackwardJump = false;
    private bool directionLock = false;








    private void Update()
    {
        HandleDashInput();
        HandleUltimateDashInput();
        
    }

    private void FixedUpdate()
    {

        AttackRecovery();
        BackwardJump();

        UpdateSpeed();
        if (isDashing || isUltimateDashing)
        {
            UpdateDash();
            UltimateDashing();

        }
        else
        {
            UpdateNormalMovement();
        }
        
        UpdateTimers();
    }










    //实时更新移速
    private void UpdateSpeed(){
        moveSpeed = baseSpeed + extraSpeed;
    }


    //普通移动
    private void UpdateNormalMovement()
    {
        currentSpeed = moveSpeed;

        // 获取WASD输入
        Vector2 inputDirection = new Vector2(
            Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0,
            Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0
        ).normalized;

        if (inputDirection.magnitude > 0.1f)
        {
            playerDirection = inputDirection;
        }

        // 计算目标速度
        Vector2 targetVelocity = inputDirection * moveSpeed;

        // 平滑移动过渡
        currentVelocity = inputDirection.magnitude > 0.1f 
            ? Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime)
            : Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);

        playerRigidbody.linearVelocity = currentVelocity;

    }



    //根据输入判定冲刺类型
    private void HandleDashInput()
    {
        // 冷却期间不响应输入
        if (cooldownTimer > 0 || isDashing) return;

        if(Input.GetKey(KeyCode.Space) && isUltimateDashing == false){
            pressSpaceTimer += Time.deltaTime;
            if(pressSpaceTimer >= ultimateDashRequiremnetTimer){
                isUltimateDashing = true;
                StartUltimateDash();
            }
        }
        
        // 检测冲刺输入
        if (Input.GetKeyUp(KeyCode.Space) && isUltimateDashing == false)
        {
            StartDash();
            pressSpaceTimer = 0;
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

        //重置当前速度
        currentVelocity = Vector2.zero;

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

        currentSpeed = currentDashSpeed;
        
        // 应用速度
        playerRigidbody.linearVelocity = dashDirection * currentDashSpeed;
        
        // 检查冲刺是否结束
        if (dashTimer >= dashDuration)
        {
            EndDash();
        }
    }

    private void EndDash()
    {
        isDashing = false;
        cooldownTimer = dashCooldown;
        playerRigidbody.linearVelocity = Vector2.zero;
    }


    private void StartUltimateDash(){
        isUltimateDashing = true;
    }


    private void UltimateDashing(){
        if(isUltimateDashing == true && Input.GetKey(KeyCode.Space)){
            if(moveSpeed <= maxDashSpeed){
                extraSpeed += increaseSpeedPerSec * Time.fixedDeltaTime;
                currentSpeed = moveSpeed;
            }
            if(directionLock == false){
                // 确定冲刺方向（优先使用当前输入方向，若无输入则使用当前面向方向）
                Vector2 inputDirection = new Vector2(
                    Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0,
                    Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0
                );
                dashDirection = inputDirection.magnitude > 0.1f 
                    ? inputDirection.normalized 
                    : playerDirection;
                directionLock = true;
            }
            backwardJumpTimer = maxBackwardJumpTime;
            attackRecoveryTimer = maxAttackRecoveryTime;
            playerRigidbody.linearVelocity = dashDirection * moveSpeed;
        }

    }

    private void HandleUltimateDashInput(){
        if(isUltimateDashing == true){
            if(Input.GetKeyUp(KeyCode.Space)){
                isStartAttackRecory = true;
            }
        }
    }


    private void AttackRecovery(){
        if(isStartAttackRecory == true){
            isDashing = true;
            attackRecoveryTimer -= Time.fixedDeltaTime;
            if(isBackwardJump == true){
                isUltimateDashing = false;
                attackRecoveryTimer = -1;
            }else if(attackRecoveryTimer < 0){
                EndUltimateDash();

            }
        }
    }


    private void EndUltimateDash(){
        isDashing = false;
        isUltimateDashing = false;
        extraSpeed = 0;
        cooldownTimer = dashCooldown;
        playerRigidbody.linearVelocity = Vector2.zero;
        directionLock = false;
        pressSpaceTimer = 0;
        isStartAttackRecory = false;
        attackRecoveryTimer = maxAttackRecoveryTime;
    }


    private void BackwardJump(){

        if(isBackwardJump && backwardJumpTimer > 0){
            backwardJumpTimer -= Time.fixedDeltaTime;
            player.transform.position -= (Vector3)(playerDirection * 0.5f);
            print(playerDirection);
        }
        if(isBackwardJump && backwardJumpTimer <= 0){
            isBackwardJump = false;
            backwardJumpTimer = maxAttackRecoveryTime;
            EndUltimateDash();
            
        }
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

    public int GetDashState(){
        if(isStartAttackRecory){
            return 3;
        }else if(isUltimateDashing){
            return 2;
        }else if(isDashing){
            return 1;
        }else{
            return 0;
        }
    }

    public bool GetBackwardJumpState(){
        return isBackwardJump;
    }

    public void SetDashCooldownTimer(float value){
        cooldownTimer = value;
    }











    // #################测试用代码，用于数据可视化###################
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
