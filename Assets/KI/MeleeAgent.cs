﻿using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace KI
{
    public class MeleeAgent : Agent
    {
        TargetComponent idleTargetComponent;
        [SerializeField] float idleDuration;
        [SerializeField] float searchRadius;
        [SerializeField] LayerMask layerMask;
        [SerializeField] float distanceThreshhold;
        Vector3 patrolRadiusCenter;
        StateMachine stateMachine;
        IdleState idleState;
        bool attackDone;

        protected override void Awake()
        {
            base.Awake();
            patrolRadiusCenter = transform.position;
            TargetComponent = new TargetComponent();
            idleTargetComponent = new TargetComponent();
            var idleTimer = new Timer(idleDuration);
            idleState = new IdleState(idleTimer, NavMeshAgent, Animator);
            State chaseState = new WalkToPointState(NavMeshAgent, TargetComponent, Animator);
            State returnToPointState = new WalkToPointState(NavMeshAgent, idleTargetComponent, Animator);
            State patrolState = new PatrolState(NavMeshAgent, idleTargetComponent, Animator, RecalculatePatrolPoint);
            State attackState = new AttackState(Animator);
            State rotateToPlayerState = new RotateToPlayerState(Animator, TargetComponent, NavMeshAgent);
            stateMachine = new StateMachine(idleState, gameObject);

            var anyToChase = new Transition(chaseState, () => FindTarget(searchRadius) || IsAggro);
            var idleToPatrol = new Transition(patrolState, () => idleState.IsTimerFinished == true);
            var movingToIdle = new Transition(idleState, () => NavMeshAgent.remainingDistance < NavMeshAgent.stoppingDistance);
            var chaseToReturn = new Transition(returnToPointState, () => FindTarget(searchRadius) == false);
            var toAttack = new Transition(attackState, () =>
            {
                if (!(DistanceToTarget <= AttackRange)) return false;
                attackDone = false;
                return true;
            });
            var attackToChase = new Transition(chaseState, () => DistanceToTarget >= AttackRange && attackDone);
            // var attackToAttack = new Transition(attackState, () => attackDone);
            var attackToReturn = new Transition(returnToPointState, () => FindTarget(searchRadius) == false && attackDone);
            var attackToRotate = new Transition(rotateToPlayerState, () =>
            {
                var dotProduct = Vector3.Dot(transform.forward, (TargetComponent.TargetPosition - transform.position).normalized);
                return dotProduct < 0.9f && attackDone;
            });
            var rotateToAttack = new Transition(attackState, () =>
            {
                var dotProduct = Vector3.Dot(transform.forward, (TargetComponent.TargetPosition - transform.position).normalized);
                return dotProduct > 0.9f;
            });
            var rotateToChase = new Transition(chaseState, () => DistanceToTarget >= AttackRange);

            idleState.AddTransition(anyToChase);
            idleState.AddTransition(idleToPatrol);

            chaseState.AddTransition(chaseToReturn);
            chaseState.AddTransition(toAttack);

            returnToPointState.AddTransition(movingToIdle);
            returnToPointState.AddTransition(anyToChase);

            patrolState.AddTransition(movingToIdle);
            patrolState.AddTransition(anyToChase);

            attackState.AddTransition(attackToReturn);
            attackState.AddTransition(attackToChase);
            // attackState.AddTransition(attackToAttack);
            attackState.AddTransition(attackToRotate);

            rotateToPlayerState.AddTransition(rotateToChase);
            rotateToPlayerState.AddTransition(rotateToAttack);
        }


        void FixedUpdate()
        {
            stateMachine.CheckSwapState();
        }

        protected override bool FindTarget(float _radius)
        {
            var overlap = Physics.OverlapSphere(this.transform.position, searchRadius, layerMask);
            if (overlap.Length > 0)
            {
                TargetComponent.SetTarget(overlap[0].transform);
                return true;
            }

            IsAggro = false;
            return false;
        }

        void RecalculatePatrolPoint()
        {
            Vector3 randomPoint;
            do
            {
                var unitSphere = Random.insideUnitSphere * 5f;
                randomPoint = new Vector3(unitSphere.x, 0, unitSphere.z);
                randomPoint += patrolRadiusCenter;
            } while (!NavMesh.SamplePosition(randomPoint, out _, NavMeshAgent.radius * 2, NavMeshAgent.areaMask) || Vector3.Distance(transform.position, randomPoint) < distanceThreshhold);

            idleTargetComponent.SetPoint(randomPoint);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, 5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position + Vector3.up, AttackRange);
        }
        
        

        void AttackStart()
        {
            attackDone = false;
        }

        void AttackDone()
        {
            attackDone = true;
        }
    }
}