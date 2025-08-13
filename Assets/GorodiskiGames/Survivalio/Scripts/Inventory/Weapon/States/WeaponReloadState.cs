using Game.Core;
using Injection;
using UnityEngine;

namespace Game.Weapon.States
{
    public sealed class WeaponReloadState : WeaponState
    {
        [Inject] private Timer _timer;

        public override void Initialize()
        {
            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            _weapon.Model.FireRate -= Time.deltaTime;
            _weapon.Model.SetChanged();

            if(_weapon.Model.FireRate > 0f)
                return;

            _weapon.Ready();
        }
    }
}

