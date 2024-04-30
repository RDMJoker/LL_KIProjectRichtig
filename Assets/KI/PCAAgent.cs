﻿using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace KI
{
    public class PCAAgent : EnemyAgent
    {
        StateMachine stateMachine;
        IdleState idleState;

        protected override void Awake()
        {
            base.Awake();
            PatrolRadiusCenter = transform.position;
            TargetComponent = new TargetComponent();
            IdleTargetComponent = new TargetComponent();
            var idleTimer = new Timer(IdleDuration);
            idleState = new IdleState(idleTimer, NavMeshAgent, Animator);
            State chaseState = new WalkToPointState(NavMeshAgent, TargetComponent, Animator);
            State returnToPointState = new WalkToPointState(NavMeshAgent, IdleTargetComponent, Animator);
            State patrolState = new PatrolState(NavMeshAgent, IdleTargetComponent, Animator, RecalculatePatrolPoint);
            State attackState = new AttackState(Animator, this);
            // State rotateToPlayerState = new RotateToPlayerState(Animator, TargetComponent, NavMeshAgent);
            stateMachine = new StateMachine(idleState, gameObject);

            var anyToChase = new Transition(chaseState, () => FindTarget(SearchRadius) || IsAggro);
            var idleToPatrol = new Transition(patrolState, () => idleState.IsTimerFinished == true);
            var movingToIdle = new Transition(idleState, () => NavMeshAgent.remainingDistance < NavMeshAgent.stoppingDistance);
            var chaseToReturn = new Transition(returnToPointState, () => FindTarget(SearchRadius) == false);
            var toAttack = new Transition(attackState, () =>
            {
                if (!(DistanceToTarget < AttackRange)) return false;
                NavMeshAgent.ResetPath();
                AttackDone = false;
                return true;
            });
            var attackToChase = new Transition(chaseState, () => DistanceToTarget >= AttackRange && AttackDone);
            var attackToReturn = new Transition(returnToPointState, () => FindTarget(SearchRadius) == false && AttackDone);

            // var attackToRotate = new Transition(rotateToPlayerState, () =>
            // {
            //     var dotProduct = Vector3.Dot(transform.forward, (TargetComponent.TargetPosition - transform.position).normalized);
            //     return dotProduct < 0.98f && AttackDone;
            // });
            // var rotateToAttack = new Transition(attackState, () =>
            // {
            //     var dotProduct = Vector3.Dot(transform.forward, (TargetComponent.TargetPosition - transform.position).normalized);
            //     return dotProduct > 0.98f;
            // });
            // var rotateToChase = new Transition(chaseState, () => DistanceToTarget >= AttackRange);

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
            // attackState.AddTransition(attackToRotate);

            // rotateToPlayerState.AddTransition(rotateToChase);
            // rotateToPlayerState.AddTransition(rotateToAttack);
        }


        void FixedUpdate()
        {
            stateMachine.CheckSwapState();
        }

        protected override bool FindTarget(float _radius)
        {
            var overlap = Physics.OverlapSphere(transform.position, SearchRadius, DetectionMask);
            if (overlap.Length > 0)
            {
                bool obstruction = Physics.Raycast(transform.position + (transform.up * 0.75f), (overlap[0].transform.position - transform.position).normalized, SearchRadius, DetectionObstructionMask);
                if (obstruction) return false;
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
                var unitSphere = Random.insideUnitSphere * PatrolRange;
                randomPoint = new Vector3(unitSphere.x, 0, unitSphere.z);
                randomPoint += PatrolRadiusCenter;
            } while (!NavMesh.SamplePosition(randomPoint, out _, NavMeshAgent.radius * 2, NavMeshAgent.areaMask) || Vector3.Distance(transform.position, randomPoint) < PatrolPointDistanceThreshhold);

            IdleTargetComponent.SetPoint(randomPoint);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, SearchRadius);
            // Gizmos.color = Color.yellow;
            // Gizmos.DrawSphere(transform.position + Vector3.up, AttackRange);
        }


        void AttackStart()
        {
            NavMeshAgent.transform.LookAt(TargetComponent.TargetPosition);
            AttackDone = false;
        }

        void SetAttackDone()
        {
            AttackDone = true;
        }
    }
}