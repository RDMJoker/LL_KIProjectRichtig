﻿using UnityEngine;

namespace KI
{
    public class AttackState : AnimationState
    {   
        static readonly int isWalking = Animator.StringToHash("isWalking");
        static readonly int isAttacking = Animator.StringToHash("isAttacking");
        public AttackState(Animator _animator) : base(_animator)
        {
        }

        public override void StateEnter()
        {
            animator.SetBool(isAttacking,true);
            animator.SetBool(isWalking, false);
        }

        public override void StateExit()
        {
            animator.SetBool(isAttacking,false);
        }
    }
}