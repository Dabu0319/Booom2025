using UnityEngine;
using Cinemachine;

public class CinemachineManualUpdater : MonoBehaviour
{
    private CinemachineBrain brain;

    void Awake()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    void Update()
    {
        if (brain != null)
        {
            brain.ManualUpdate(); // 用 unscaled time 驱动相机
        }
    }
}