using System;
using System.Linq;
using CombatSystems;
using LL_Unity_Utils.Misc;
using LL_Unity_Utils.Scriptables;
using Scriptables.Lists;
using Scriptables.VFXScriptables;
using UnityEngine;
using UnityEngine.AI;
using VFXScripts;

namespace KI
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(HealthSystem))]
    public abstract class Agent : MonoBehaviour, IHitable
    {
        [SerializeField] public float AttackRange;
        [SerializeField] public float AttackDamage;
        [SerializeField] public float SpellDamage;
        [SerializeField] [Min(0.01f)] public float AttackSpeed;
        [SerializeField] protected bool StateMachineDebugMode;
        [SerializeField] protected float StunThreshhold;
        [SerializeField] public float StunDuration;
        [SerializeField] bool debugHealthSystem;
        [SerializeField] VFXSpawner hitEffectSpawner;
        [SerializeField] ColorGradientList hitEffectColorGradients;
        protected HealthSystem HealthSystem;
        protected TargetComponent TargetComponent;
        protected NavMeshAgent NavMeshAgent;
        protected Animator Animator;
        CapsuleCollider capsuleCollider;
        BoxCollider hitCollider;
        protected bool IsStunned;
        

        public bool IsDead => HealthSystem.IsDead;
        public bool IsCasting;

        protected float DistanceToTarget => Vector3.Distance(transform.position, TargetComponent.TargetPosition);


        protected virtual void Awake()
        {
            NavMeshAgent = GetComponent<NavMeshAgent>();
            Animator = GetComponent<Animator>();
            HealthSystem = GetComponent<HealthSystem>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            hitCollider = GetComponent<BoxCollider>();

        }

        protected abstract bool FindTarget(float _radius);

        public virtual void TakeDamage(float _value, GameObject _hitter)
        {
            HealthSystem.ReduceCurrentHP(_value);
            if (HealthSystem.IsDead) return;
            if (debugHealthSystem) Debug.Log($"Aua! Ich habe noch {HealthSystem.CurrentHP} von maximal {HealthSystem.MaxHP} Leben!");
        }

        public virtual void OnHit(Agent _attackingAgent, float _damage, EDamageType _damageType)
        {
            IsStunned = HealthSystem.CheckStunned(_damage, StunThreshhold);
            hitEffectSpawner.Spawn(transform.position, out var spawnedObject);
            var colorGradient = GetColorByDamageType(_damageType);
            spawnedObject.GetComponent<VFXColorChanger>().ChangeColor(colorGradient);
            TakeDamage(_damage, _attackingAgent.gameObject);
        }

        public void OnHit(float _damage, EDamageType _damageType)
        {
            OnHit(this, _damage, _damageType);
        }

        ColorGradient GetColorByDamageType(EDamageType _damageType)
        {
            foreach (var colorGradient in hitEffectColorGradients.colorGradients.Where(colorGradient => colorGradient.DamageType == _damageType))
            {
                return colorGradient;
            }
            throw new NotImplementedException();
        }

        public virtual void OnDeath()
        {
            if (debugHealthSystem) Debug.Log("Ich bin tot :( ");
            NavMeshAgent.enabled = false;
            hitCollider.enabled = false;
            capsuleCollider.enabled = false;
        }
    }
}