using Game.Managers;
using Injection;
using Game.UI.Hud;
using UnityEngine;
using Game.UI;
using Game.Core;
using System;
using System.Collections.Generic;
using Game.Player;
using Game.Config;
using Game.Level;
using Game.Modules;
using GameAnalyticsSDK;

namespace Game.States
{
    public sealed class GamePlayState : GameState
    {
		private const float _rotationY = 180f;
		private const float _showHudDelay = 2f;

		[Inject] private GameView _gameView;
		[Inject] private Injector _injector;
		[Inject] private Context _context;
		[Inject] private HudManager _hudManager;
		[Inject] private GameConfig _config;
		[Inject] private Timer _timer;

		private GameManager _gameManager;
		private LevelManager _levelManager;
		private AdMob _adMob;
		private LevelView _levelView;
		private int _startedLevel;
		
        public readonly List<Module> _levelModules;
		private float _showHudTime;
		private bool _isWin;

		public GamePlayState()
		{
			_levelModules = new List<Module>();
			_adMob=  new AdMob();
		}

		public override void Initialize()
        {
			_gameManager = new GameManager(_config);
			var level = _gameManager.Model.Level;
			var levelConfig = _config.LevelConfigs[level];
			var durationMax = _gameManager.Model.LevelDurationMax;
			_startedLevel = level;
            _levelManager = new LevelManager(levelConfig, level, durationMax);
            AnalyticsManager.Instance.OnLevelStart(_startedLevel);
            
			var prefab = _levelManager.Model.Prefab;
			_levelView = GameObject.Instantiate(prefab).GetComponent<LevelView>();

			_context.Install(_gameManager);
			_context.Install(_levelManager);
			_context.Install(_levelView);
			_context.ApplyInstall();

			var model = new PlayerModel(_config);
			var player = new PlayerController(_gameView.PlayerView, model, _context);
			player.View.Position = Vector3.zero;
			player.View.Rotation = Quaternion.Euler(0f, _rotationY, 0f);
			player.Idle();
			_gameManager.Player = player;

			_levelManager.Initialize();

			_gameView.Camera.SetPosition(_gameManager.Player.View.Position);
			_gameView.Camera.SetTarget(_gameManager.Player.View.RotateNode);
			_gameView.Camera.SetEnable(true);

			InitLevelModules();

			_hudManager.ShowAdditional<GamePlayHudMediator>();

			_gameView.Joystick.SetEnable(true);
			_gameView.MenuLight.SetActive(false);
			_gameView.LevelLight.SetActive(true);

			_levelManager.ON_LEVEL_END += OnLevelEnd;
			_gameView.Joystick.ON_FIRST_TOUCH += OnFirstTouch;
		}

        public override void Dispose()
        {
			_levelManager.ON_LEVEL_END -= OnLevelEnd;
			_gameView.Joystick.ON_FIRST_TOUCH -= OnFirstTouch;

			_hudManager.HideAdditional<GamePlayHudMediator>();

			_gameView.Joystick.SetEnable(false);
			_gameView.Camera.SetEnable(false);
			_gameView.Camera.SetTarget(null);

			_context.Uninstall(_gameManager);
			_context.Uninstall(_levelManager);
			_context.Uninstall(_levelView);

			DisposeLevelModules();

			_gameManager.Dispose();
			_levelManager.Dispose();

			GameObject.Destroy(_levelView.gameObject);
		}

        private void InitLevelModules()
		{
			AddModule<GroundsModule>();
            AddModule<WeaponsModule, WeaponsModuleView>(_gameView);
			AddModule<AbilitiesModule, AbilitiesModuleView>(_gameView);
			AddModule<EnemiesModule, EnemiesModuleView>(_levelView);
			AddModule<EffectsModule, EffectsModuleView>(_gameView);
			AddModule<CollectiblesModule, CollectiblesModuleView>(_gameView);
            AddModule<SkillsModule>();
			AddModule<UINotificationModule, UINotificationModuleView>(_gameView);
		}

		private void OnLevelEnd(bool isWin)
		{
			_levelManager.ON_LEVEL_END -= OnLevelEnd;

			_isWin = isWin;
			if (isWin)
				AnalyticsManager.Instance.OnLevelComplete(_startedLevel);
			else
				AnalyticsManager.Instance.OnLevelFailed(_startedLevel);

			if (_isWin)
				_gameManager.Player.Win();

			var elapsed = _levelManager.Model.Elapsed;
			var durationMax = _levelManager.Model.DurationMax;
			if (elapsed > durationMax)
			{
				_levelManager.Model.IsNewRecord = true;
				_levelManager.Model.DurationMax = elapsed;

				_gameManager.Model.LevelDurationMax = elapsed;
				_gameManager.Model.Save();
			}

			_showHudTime = _timer.Time + _showHudDelay;
			_timer.TICK += OnTickDelayShowHud;
			_adMob.ShowInterstitial();
		}

		private void OnTickDelayShowHud()
		{
			if(_timer.Time < _showHudTime)
				return;

			_timer.TICK -= OnTickDelayShowHud;

			if (_isWin)
				_hudManager.ShowSingle<VictoryHudMediator>();
			else
				_hudManager.ShowSingle<DefeatHudMediator>();
		}

		private void OnFirstTouch()
		{
			_gameView.Joystick.ON_FIRST_TOUCH -= OnFirstTouch;
			_gameView.BarsHolder.gameObject.SetActive(true);
        }

		private void AddModule<T>(params object[] args) where T : Module
		{
			var result = (T)Activator.CreateInstance(typeof(T), args);
			AddModule(result);
		}

		private void AddModule(Module result)
		{
			_levelModules.Add(result);
			_injector.Inject(result);
			result.Initialize();
		}

		private void AddModule<T, T1>(MonoBehaviour moduleView) where T : Module
		{
			var view = moduleView.GetComponent<T1>();

			if (null == view)
				return;

			var result = (T)Activator.CreateInstance(typeof(T), new object[] { view });

			_levelModules.Add(result);
			_injector.Inject(result);
			result.Initialize();
		}

		private void DisposeLevelModules()
		{
			foreach (var levelModule in _levelModules)
			{
				levelModule.Dispose();
			}
			_levelModules.Clear();
		}
	}
}

