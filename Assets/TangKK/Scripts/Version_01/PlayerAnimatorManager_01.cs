using UnityEngine;
using shark;

namespace TangKK
{
    /// <summary>
    /// 玩家动画管理器 - 支持时停状态下的动画播放
    /// 修改版本：在时停期间也能正常更新动画状态
    /// </summary>
    public class PlayerAnimatorManager_01 : MonoBehaviour
    {
        #region 组件引用
        private PlayerMovementController playerMovement;
        private Animator animator;
        private Rigidbody2D rb;
        #endregion

        #region 动画状态变量
        [Header("🎭 动画状态")]
        public bool isAttacking = false;
        public bool canAttack = true;
        public bool isRunning = false;
        public bool isBackJump = false;
        public bool isDash = false;
        public bool isDie = false;
        public bool isOpening = false;
        #endregion

        #region 配置参数
        [Header("🔄 旋转控制")]
        [SerializeField] private float turnSpeed = 10f;
        
        [Header("⏱️ 时停动画控制")]
        [Tooltip("是否在时停状态下更新动画")]
        [SerializeField] private bool updateAnimationDuringTimeStop = true;
        
        [Tooltip("时停状态下的动画速度倍率")]
        [SerializeField] private float timeStopAnimationSpeed = 1f;
        
        [Header("🎯 检测阈值")]
        [SerializeField] private float runningSpeedThreshold = 0.05f;
        #endregion

        #region 动画参数哈希（性能优化）
        private static class AnimatorHashes
        {
            public static readonly int IsAttacking = Animator.StringToHash("isAttacking");
            public static readonly int IsRunning = Animator.StringToHash("isRunning");
            public static readonly int IsBackJump = Animator.StringToHash("isBackJump");
            public static readonly int IsDash = Animator.StringToHash("isDash");
            public static readonly int IsDie = Animator.StringToHash("isDie");
            public static readonly int Opening = Animator.StringToHash("Opening");
        }
        #endregion

        #region Unity 生命周期
        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            if (!AreComponentsValid()) return;
            
            UpdateAnimationStates();
            UpdateAnimatorSpeed();
        }

        private void LateUpdate()
        {
            if (playerMovement == null) return;
            
            HandleRotation();
        }
        #endregion

        #region 初始化
        private void InitializeComponents()
        {
            playerMovement = GetComponentInParent<PlayerMovementController>();
            animator = GetComponent<Animator>();
            rb = GetComponentInParent<Rigidbody2D>();

            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (playerMovement == null) 
                Debug.LogError("[PlayerAnimator] 未找到 PlayerMovementController", this);
            if (animator == null) 
                Debug.LogError("[PlayerAnimator] 未找到 Animator", this);
            if (rb == null) 
                Debug.LogError("[PlayerAnimator] 未找到 Rigidbody2D", this);
        }

        private bool AreComponentsValid()
        {
            return playerMovement != null && animator != null && rb != null;
        }
        #endregion

        #region 动画状态更新
        private void UpdateAnimationStates()
        {
            // 更新各种动画状态
            UpdateAttackState();
            UpdateMovementState();
            UpdateDashState();
            UpdateJumpState();
            
            // 应用到Animator
            ApplyAnimatorParameters();
        }

        private void UpdateAttackState()
        {
            isAttacking = playerMovement.GetisStartAttackRecory() && canAttack && !isBackJump;
        }

        private void UpdateMovementState()
        {
            // 在时停状态下，需要特别处理速度检测
            float currentSpeed = GetEffectiveSpeed();
            isRunning = currentSpeed > runningSpeedThreshold;
        }

        private void UpdateDashState()
        {
            isDash = playerMovement.GetDashState() == 2;
        }

        private void UpdateJumpState()
        {
            isBackJump = playerMovement.GetisBackJump();
        }

        private float GetEffectiveSpeed()
        {
            // 如果在时停状态下，需要从PlayerMovementController获取"虚拟速度"
            if (IsInTimeStop() && playerMovement.GetTimeFreezeState())
            {
                // 在时停状态下，通过检查输入来判断是否在"移动"
                Vector2 input = new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")
                );
                return input.magnitude > 0.1f ? 1f : 0f; // 模拟速度
            }
            
            return rb.linearVelocity.magnitude;
        }

        private void ApplyAnimatorParameters()
        {
            // 使用哈希值提高性能
            animator.SetBool(AnimatorHashes.IsAttacking, isAttacking);
            animator.SetBool(AnimatorHashes.IsRunning, isRunning);
            animator.SetBool(AnimatorHashes.IsBackJump, isBackJump);
            animator.SetBool(AnimatorHashes.IsDash, isDash);
            animator.SetBool(AnimatorHashes.IsDie, isDie);
            animator.SetBool(AnimatorHashes.Opening, isOpening);
        }

        private void UpdateAnimatorSpeed()
        {
            if (animator == null) return;
            
            if (IsInTimeStop() && updateAnimationDuringTimeStop)
            {
                // 在时停状态下，设置动画器使用unscaled time
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.speed = timeStopAnimationSpeed;
            }
            else
            {
                // 正常状态下，使用默认设置
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.speed = 1f;
            }
        }

        private bool IsInTimeStop()
        {
            return Time.timeScale <= 0f;
        }
        #endregion

        #region 旋转控制
        private void HandleRotation()
        {
            Vector2 direction = playerMovement.GetDirection();
            if (direction != Vector2.zero)
            {
                RotateTowardsDirection(direction);
            }
        }

        /// <summary>
        /// 旋转到指定方向（支持时停状态）
        /// </summary>
        public void RotateTowardsDirection(Vector2 direction)
        {
            if (direction == Vector2.zero) return;

            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction.normalized);
            
            // 在时停状态下也能正常旋转
            float deltaTime = GetEffectiveDeltaTime();
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * deltaTime
            );
        }

        private float GetEffectiveDeltaTime()
        {
            // 根据时停状态选择合适的deltaTime
            return IsInTimeStop() ? Time.unscaledDeltaTime : Time.deltaTime;
        }
        #endregion

        #region 攻击控制
        /// <summary>
        /// 中断攻击
        /// </summary>
        public void InterruptAttack()
        {
            canAttack = false;
            isAttacking = false;
            
            if (animator != null)
            {
                animator.SetBool(AnimatorHashes.IsAttacking, false);
            }
            
            Debug.Log("[PlayerAnimator] 🛑 攻击已中断");
        }

        /// <summary>
        /// 重置攻击状态
        /// </summary>
        public void ResetAttack()
        {
            canAttack = true;
            Debug.Log("[PlayerAnimator] 🔄 攻击状态已重置");
        }

        /// <summary>
        /// 强制设置攻击状态
        /// </summary>
        public void ForceSetAttackState(bool attacking)
        {
            isAttacking = attacking;
            if (animator != null)
            {
                animator.SetBool(AnimatorHashes.IsAttacking, attacking);
            }
        }
        #endregion

        #region 生命状态控制
        /// <summary>
        /// 设置死亡状态
        /// </summary>
        public void SetDeathState(bool dead)
        {
            isDie = dead;
            Debug.Log($"[PlayerAnimator] 💀 死亡状态: {dead}");
        }

        /// <summary>
        /// 设置开场状态
        /// </summary>
        public void SetOpeningState(bool opening)
        {
            isOpening = opening;
            Debug.Log($"[PlayerAnimator] 🎬 开场状态: {opening}");
        }
        #endregion

        #region 时停动画控制
        /// <summary>
        /// 启用时停状态下的动画更新
        /// </summary>
        public void EnableTimeStopAnimation()
        {
            updateAnimationDuringTimeStop = true;
            UpdateAnimatorSpeed();
            Debug.Log("[PlayerAnimator] ✅ 时停动画已启用");
        }

        /// <summary>
        /// 禁用时停状态下的动画更新
        /// </summary>
        public void DisableTimeStopAnimation()
        {
            updateAnimationDuringTimeStop = false;
            UpdateAnimatorSpeed();
            Debug.Log("[PlayerAnimator] ❌ 时停动画已禁用");
        }

        /// <summary>
        /// 设置时停状态下的动画速度
        /// </summary>
        public void SetTimeStopAnimationSpeed(float speed)
        {
            timeStopAnimationSpeed = Mathf.Max(0f, speed);
            if (IsInTimeStop())
            {
                UpdateAnimatorSpeed();
            }
        }

        /// <summary>
        /// 在时停状态下播放指定动画
        /// </summary>
        public void PlayAnimationDuringTimeStop(string animationName)
        {
            if (!IsInTimeStop() || animator == null) return;
            
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.Play(animationName);
        }

        /// <summary>
        /// 在时停状态下播放指定动画（使用哈希）
        /// </summary>
        public void PlayAnimationDuringTimeStop(int animationHash)
        {
            if (!IsInTimeStop() || animator == null) return;
            
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.Play(animationHash);
        }
        #endregion

        #region 公共接口
        /// <summary>
        /// 获取当前是否处于时停状态
        /// </summary>
        public bool IsInTimeFreezeState => IsInTimeStop();

        /// <summary>
        /// 设置旋转速度
        /// </summary>
        public void SetTurnSpeed(float speed)
        {
            turnSpeed = Mathf.Max(0f, speed);
        }

        /// <summary>
        /// 设置跑步检测阈值
        /// </summary>
        public void SetRunningSpeedThreshold(float threshold)
        {
            runningSpeedThreshold = Mathf.Max(0f, threshold);
        }

        /// <summary>
        /// 强制刷新动画状态
        /// </summary>
        public void RefreshAnimationStates()
        {
            UpdateAnimationStates();
        }

        /// <summary>
        /// 获取当前动画状态信息
        /// </summary>
        public string GetCurrentAnimationInfo()
        {
            return $"攻击:{isAttacking}, 跑步:{isRunning}, 冲刺:{isDash}, 后跳:{isBackJump}, 死亡:{isDie}, 开场:{isOpening}";
        }
        #endregion

        #region 调试功能
        private void OnValidate()
        {
            turnSpeed = Mathf.Max(0f, turnSpeed);
            timeStopAnimationSpeed = Mathf.Max(0f, timeStopAnimationSpeed);
            runningSpeedThreshold = Mathf.Max(0f, runningSpeedThreshold);
        }

        /// <summary>
        /// 在Scene视图中显示调试信息
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (playerMovement == null) return;
            
            // 显示朝向方向
            Vector2 direction = playerMovement.GetDirection();
            if (direction != Vector2.zero)
            {
                Gizmos.color = Color.yellow;
                Vector3 directionWorld = new Vector3(direction.x, direction.y, 0f) * 2f;
                Gizmos.DrawLine(transform.position, transform.position + directionWorld);
                Gizmos.DrawSphere(transform.position + directionWorld, 0.1f);
            }
            
            // 显示时停状态
            if (IsInTimeStop())
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
            }
        }

        /// <summary>
        /// 显示调试UI
        /// </summary>
        private void OnGUI()
        {
            if (!Application.isEditor || !IsInTimeStop()) return;
            
            GUILayout.BeginArea(new Rect(10, 200, 300, 200));
            GUILayout.Label("=== 动画调试信息 (时停中) ===");
            GUILayout.Label($"攻击中: {isAttacking}");
            GUILayout.Label($"跑步中: {isRunning}");
            GUILayout.Label($"冲刺中: {isDash}");
            GUILayout.Label($"后跳中: {isBackJump}");
            GUILayout.Label($"动画器更新模式: {animator?.updateMode}");
            GUILayout.Label($"动画器速度: {animator?.speed:F2}");
            GUILayout.EndArea();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void DebugLogAnimationState()
        {
            Debug.Log($"[PlayerAnimator] 动画状态 - {GetCurrentAnimationInfo()}");
        }
        #endregion
    }
}