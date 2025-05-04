using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;

public class PressAnyKeyController : MonoBehaviour
{
    public TextMeshProUGUI pressText;
    public Image title;

    private bool hasPressedKey = false;
    private Tween blinkTween;

    public UnityEvent onStart;

    void Start()
    {
        // 初始透明度设置为1
        pressText.alpha = 1f;

        // 闪烁动画：无限淡入淡出
        blinkTween = pressText.DOFade(0f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    void Update()
    {
        if (!hasPressedKey && Input.anyKeyDown)
        {
            hasPressedKey = true;

            // 停止闪烁
            blinkTween.Kill();

            // 淡出当前文字，然后显示“1”
            pressText.DOFade(1f, 0.5f).OnComplete(() =>
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    pressText.DOFade(0f, 0.5f);
                    title.DOFade(0f, 0.5f);
                        
                    // 调用事件
                    onStart?.Invoke();
                });
            });
        }
    }
}