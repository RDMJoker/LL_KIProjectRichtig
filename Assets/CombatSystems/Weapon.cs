﻿using KI;
using UnityEngine;

namespace CombatSystems
{
    public abstract class Weapon : MonoBehaviour
    {
        protected Agent weaponHolder;
        public abstract void DoDamage(IHitable _target);
    }
}