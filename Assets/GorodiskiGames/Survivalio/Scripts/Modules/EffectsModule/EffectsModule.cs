using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Effect;
using Injection;
using UnityEngine;
using Game.Managers;

namespace Game.Modules
{
    public sealed class EffectsModule : Module<EffectsModuleView>
    {
        [Inject] private Timer _timer;
        [Inject] private GameManager _gameManager;

        private readonly List<EffectView> _effects;

        public EffectsModule(EffectsModuleView view) : base(view)
        {
            _effects = new List<EffectView>();
        }

        public override void Initialize()
        {
            _gameManager.ON_SPAWN_EFFECT += OnSpawnEffect;
            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _gameManager.ON_SPAWN_EFFECT -= OnSpawnEffect;
            _timer.TICK -= OnTick;

            foreach (var pool in _view.EffectsMap.Values)
            {
                pool.ReleaseAllInstances();
            }
            _effects.Clear();
        }

        private void OnSpawnEffect(EffectType type, Vector3 position, Vector3 direction)
        {
            var pool = _view.EffectsMap[type];

            var view = pool.Get<EffectView>();
            view.gameObject.SetActive(false);

            view.Init(type, position, direction);
            view.gameObject.SetActive(true);

            view.Play();

            view.ON_REMOVE += OnRemove;

            _effects.Add(view);
        }

        private void OnRemove(EffectView view)
        {
            view.ON_REMOVE -= OnRemove;

            _effects.Remove(view);

            var pool = _view.EffectsMap[view.Type];
            pool.Release(view);
        }

        private void OnTick()
        {
            foreach (var view in _effects.ToList())
            {
                view.Proceed();
            }
        }
    }
}

