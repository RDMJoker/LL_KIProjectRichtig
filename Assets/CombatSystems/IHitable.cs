﻿using KI;
using UnityEngine;

namespace CombatSystems
{
    public interface IHitable
    {
        public void TakeDamage(float _value, GameObject _hitter);
        public void OnHit(Agent _attackingAgent);
        public void OnDeath();
    }
}