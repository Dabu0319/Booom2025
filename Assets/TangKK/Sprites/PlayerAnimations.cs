using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D.Animation;

namespace TangKK{
public class PlayerAnimations : MonoBehaviour
{
    private PlayerAttack playerAttack;
    private PlayerMove playerMove;
    private Animator anim;

    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
        playerMove = GetComponent<PlayerMove>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        SetAnimation();
    }

    public void SetAnimation()
    {
        // 设置动画参数
        anim.SetBool("isMoving", playerMove.isMoving);
        anim.SetBool("isAttack", playerAttack.isAttack);
    }
}
}