using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URPTimeStopEffect : MonoBehaviour
{
    

    // URP组件
    private Volume volume;
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private ChromaticAberration chromatic;

    private bool isActive = false;
    private float currentIntensity = 0f;
    private float timer = 0f;

    private void Awake()
    {
        volume = GetComponent<Volume>();
        
        // 确保所有需要的Volume组件存在
        if (!volume.sharedProfile.TryGet(out colorAdjustments))
        {
            colorAdjustments = volume.sharedProfile.Add<ColorAdjustments>(true);
        }
        if (!volume.sharedProfile.TryGet(out vignette))
        {
            vignette = volume.sharedProfile.Add<Vignette>(true);
        }
        if (!volume.sharedProfile.TryGet(out chromatic))
        {
            chromatic = volume.sharedProfile.Add<ChromaticAberration>(true);
        }
    }


    private void Start()
    {
        colorAdjustments.contrast.value = 0f;
        colorAdjustments.hueShift.value = 0f;
        vignette.intensity.value = 0f;
        chromatic.intensity.value = 0f;
    }


    private void Update()
    {
        if(isActive && timer < 1f){
            timer += 10*Time.deltaTime;
        }else if(isActive == false && timer > 0f){
            timer -= 10*Time.deltaTime;
        }
        currentIntensity = timer;
        if(isActive){
            UpdateEffects(1);
        }else if(isActive == false){
            UpdateEffects(0);
        }

    }

    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }

    public void Toggle()
    {
        isActive = !isActive;
    }

    private void UpdateEffects(float t)
    {
        // 颜色调整
        if (colorAdjustments != null)
        {
            colorAdjustments.active = true;
            colorAdjustments.contrast.value = Mathf.Lerp(0, 20f, t);
            colorAdjustments.hueShift.value = Mathf.Lerp(0, -50f, t);
        }


        // 暗角效果
        if (vignette != null)
        {
            vignette.active = true;
            vignette.intensity.value = Mathf.Lerp(0f, 0.4f, t);
        }


        if (chromatic != null)
        {
            chromatic.active = true;
            chromatic.intensity.value = Mathf.Lerp(0f, 1f, t);
        }
    }


}
