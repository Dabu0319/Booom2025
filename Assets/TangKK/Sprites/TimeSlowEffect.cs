using UnityEngine;

namespace TangKK
{
    public class TimeSlowEffect : MonoBehaviour
    {
        [Header("Time Slow Settings")]
        public float slowTimeScale = 0.1f;      // 慢动作速度（0.1 = 十分之一速度）
        public float slowDuration = 0.2f;       // 慢动作持续时间（真实时间）

        private bool isSlowing = false;

        public void TriggerTimeSlow()
        {
            if (!isSlowing)
            {
                StartCoroutine(DoTimeSlow());
            }
        }

        private System.Collections.IEnumerator DoTimeSlow()
        {
            isSlowing = true;

            // 记录当前时间缩放
            float originalTimeScale = Time.timeScale;
            float originalFixedDeltaTime = Time.fixedDeltaTime;

            // 设置慢动作
            Time.timeScale = slowTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            // 等待一段真实时间
            yield return new WaitForSecondsRealtime(slowDuration);

            // 恢复时间
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;

            isSlowing = false;
        }
    }
}