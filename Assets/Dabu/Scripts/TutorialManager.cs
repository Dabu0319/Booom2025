using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("步骤 UI prefab")]
    public GameObject[] tutorialSteps; // 每个步骤的提示框 prefab
    
    [Header("目标玩家")]
    public Transform player;           // 玩家 transform
    public Vector3 uiOffset = new Vector3(0, 2f, 0); // UI 偏移在角色头顶

    public int currentStep = 0;
    //private GameObject currentUI;

    public static TutorialManager Instance { get; private set; }

    public GameObject Scarecrow;

    private List<Vector3> spawnedPositions = new List<Vector3>();
    public float minDistance = 0.5f; // 距离小于这个就算重叠
    public Vector3 offsetStep = new Vector3(0, -1f, 0); // 如果重叠，就向上偏移
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

    public void ShowScarecrow()
    {
        Scarecrow.SetActive(true);
    }
    
    public void ShowStepWithDelay(int index, float delay)
    {
       
        StartCoroutine(ShowStepWithDelayCoroutine(index, delay));
        
    }
    
    public IEnumerator ShowStepWithDelayCoroutine(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowStep(index);
    }

    public void ShowStep(int index)
    {
        Vector3 basePos = player.position + uiOffset;
        Vector3 finalPos = basePos;

        // 检查是否与已有 UI 太近，最多尝试偏移 10 次
        for (int i = 0; i < 10; i++)
        {
            bool tooClose = false;
            foreach (var pos in spawnedPositions)
            {
                if (Vector3.Distance(finalPos, pos) < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose) break;

            finalPos += offsetStep; // 向上偏移
        }

        // 创建并记录位置
        GameObject stepUI = Instantiate(tutorialSteps[index], finalPos, Quaternion.identity);
        spawnedPositions.Add(finalPos);
    }
    public void TryAdvance(int stepId)
    {
        if (stepId != currentStep) return; // 只有顺序正确才允许前进

        Debug.Log($"当前步骤：{currentStep}，尝试前进到步骤：{stepId}");

        currentStep++;

        if (currentStep < 5)
        {
            ShowStepWithDelay(currentStep,2f);
        }
        else
        {
            Debug.Log("教程完成！");
            ShowStep(5);
            LevelLoader.Instance.LoadNextLevelWithDelay(2f);
        }
    }
}