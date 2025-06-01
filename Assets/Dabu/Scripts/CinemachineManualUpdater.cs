using UnityEngine;
using Cinemachine;

public class CinemachineTimeStopUpdater : MonoBehaviour
{
    public static CinemachineTimeStopUpdater Instance { get; private set; }

    private CinemachineBrain brain;
    private bool isActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 保证单例
            return;
        }
        Instance = this;

        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    void Update()
    {
        // 如果启用了更新，即使 timeScale 为 0，也继续驱动相机
        if (isActive && brain != null)
        {
            brain.ManualUpdate();
        }
    }

    // 进入时停：开始手动更新 Cinemachine
    public void Enter()
    {
        isActive = true;
        if (brain != null)
        {
            brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;
        }
    }

    // 退出时停：还原 Cinemachine 的正常更新方式
    public void Exit()
    {
        isActive = false;
        if (brain != null)
        {
            brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate; // 或 SmartUpdate，根据你的默认值
        }
    }
}