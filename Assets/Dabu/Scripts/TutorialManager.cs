using System;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("步骤 UI prefab")]
    public GameObject[] tutorialSteps; // 每个步骤的提示框 prefab
    
    [Header("目标玩家")]
    public Transform player;           // 玩家 transform
    public Vector3 uiOffset = new Vector3(0, 2f, 0); // UI 偏移在角色头顶

    private int currentStep = 0;
    private GameObject currentUI;

    public static TutorialManager Instance { get; private set; }

    private void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        ShowStep(currentStep);
    }

    void Update()
    {
        if (currentStep >= tutorialSteps.Length) return;

        // UI 跟随玩家位置
        if (currentUI != null && player != null)
        {
            Vector3 worldPos = player.position + uiOffset;
            currentUI.transform.position = Camera.main.WorldToScreenPoint(worldPos);
        }
    }

    void ShowStep(int index)
    {
        if (index >= tutorialSteps.Length) return;

        currentUI = Instantiate(tutorialSteps[index], transform); // 创建 UI
    }

    public void TryAdvance(int stepId)
    {
        if (stepId != currentStep) return; // 只有顺序正确才允许前进

        if (currentUI != null)
            Destroy(currentUI);

        currentStep++;

        if (currentStep < 5)
        {
            ShowStep(currentStep);
        }
        else
        {
            Debug.Log("教程完成！");
            // 可扩展：触发事件、标记存档、隐藏UI等
        }
    }
}