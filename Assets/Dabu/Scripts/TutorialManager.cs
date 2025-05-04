using System;
using System.Collections;
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
        

        //instantiate game prefab
        GameObject stepUI = Instantiate(tutorialSteps[index], player.position + uiOffset, Quaternion.identity);
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
            // 可扩展：触发事件、标记存档、隐藏UI等
        }
    }
}