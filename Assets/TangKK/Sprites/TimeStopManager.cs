using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace TangKK{

public class TimeStopManager : MonoBehaviour
{
    [Header("能量 & 时停设置")]
    public int parryCountToTrigger = 3;       // 需要的弹反次数
    public float timeStopDuration = 2f;       // 时停持续时间（秒）

    private int currentParryCount = 0;
    private bool isTimeStopped = false;

    private List<Rigidbody2D> enemyBodies = new List<Rigidbody2D>();
    private List<MonoBehaviour> enemyBehaviours = new List<MonoBehaviour>();
    private List<Animator> enemyAnimators = new List<Animator>();  // ✅ 动画列表

    public static TimeStopManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddParryPoint()
    {
        if (isTimeStopped) return;

        currentParryCount++;
        Debug.Log($"弹反能量：{currentParryCount}");

        if (currentParryCount >= parryCountToTrigger)
        {
            StartCoroutine(DoTimeStop());
        }
    }

    private IEnumerator DoTimeStop()
    {
        Debug.Log("⚡ 触发时停！");
        isTimeStopped = true;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyBodies.Clear();
        enemyBehaviours.Clear();
        enemyAnimators.Clear();  // ✅ 清空动画列表

        foreach (var enemy in enemies)
        {
            // ✅ 冻结刚体
            var rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true;
                enemyBodies.Add(rb);
            }

            // ✅ 暂停脚本
            var scripts = enemy.GetComponents<MonoBehaviour>();
            foreach (var s in scripts)
            {
                if (s.enabled && s != this)
                {
                    s.enabled = false;
                    enemyBehaviours.Add(s);
                }
            }

            // ✅ 暂停 Animator
            var animator = enemy.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
                enemyAnimators.Add(animator);
            }
        }

        // ✅ 等待时停结束
        yield return new WaitForSecondsRealtime(timeStopDuration);

        // ✅ 恢复刚体
        foreach (var rb in enemyBodies)
        {
            rb.isKinematic = false;
        }

        // ✅ 恢复脚本
        foreach (var script in enemyBehaviours)
        {
            script.enabled = true;
        }

        // ✅ 恢复 Animator
        foreach (var animator in enemyAnimators)
        {
            animator.enabled = true;
        }

        currentParryCount = 0;
        isTimeStopped = false;

        Debug.Log("⌛ 时停结束");
    }
}
}