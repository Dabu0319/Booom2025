using UnityEngine;

public class SetSpriteOffset : StateMachineBehaviour
{
    public Vector2 offset;

    // 动画状态进入时调用
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Transform sprite = animator.transform;
        sprite.localPosition = offset;
    }
}