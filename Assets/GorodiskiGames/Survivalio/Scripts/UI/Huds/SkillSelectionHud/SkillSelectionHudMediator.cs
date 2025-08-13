using Game.Ability;
using Game.Core.UI;
using Game.Managers;
using Game.Weapon;
using Injection;
using System.Collections.Generic;
using System.Linq;

namespace Game.UI.Hud
{
    public sealed class SkillSelectionHudMediator : Mediator<SkillSelectionHudView>
    {
        [Inject] private LevelManager _levelManager;
        [Inject] private HudManager _hudManager;
        [Inject] private GameManager _gameManager;

        private readonly Dictionary<SkillSelectionSlotView, WeaponModel> _weaponSlotMap;
        private readonly Dictionary<SkillSelectionSlotView, AbilityModel> _abilitySlotMap;

        public SkillSelectionHudMediator()
        {
            _weaponSlotMap = new Dictionary<SkillSelectionSlotView, WeaponModel>();
            _abilitySlotMap = new Dictionary<SkillSelectionSlotView, AbilityModel>();
        }

        protected override void Show()
        {
            _levelManager.Pause();

            var weaponConfigs = _levelManager.Model.WeaponConfigs;
            var slotIndex = 0;
            foreach (var config in weaponConfigs)
            {
                var serial = GameConstants.GetSerial();
                var index = config.Index;
                var existedWeapon = _levelManager.WeaponsMap.Keys.ToList().Find(w => w.Model.Index == index);
                var model = new WeaponModel(config, serial, 0);

                var isExisting = existedWeapon != null;
                if (isExisting)
                {
                    var count = existedWeapon.Model.Count;
                    count++;
                    model.Count = count;
                }

                var slot = _view.Slots.ToList().ElementAtOrDefault(slotIndex);
                if(slot == null)
                    continue;

                var type = SkillType.Weapon;
                slot.Initialize(type, model.Icon, model.Label, model.Count);

                slot.ON_CLICK += OnWeaponSlotClick;
                _weaponSlotMap[slot] = model;

                slotIndex++;
            }

            var abilityConfigs = _levelManager.Model.AbilityConfigs;
            foreach (var config in abilityConfigs)
            {
                var type = config.Type;
                var existedAbility = _levelManager.Abilities.Find(a => a.Model.Type == type);
                var model = new AbilityModel(config);

                var isExisting = existedAbility != null;
                if (isExisting)
                {
                    var count = existedAbility.Model.Count;
                    count++;
                    model.Count = count;
                }

                var slot = _view.Slots.ToList().ElementAtOrDefault(slotIndex);
                if (slot == null)
                    continue;

                var skillType = SkillType.Ability;
                slot.Initialize(skillType, model.Icon, model.Label, model.Count);

                slot.ON_CLICK += OnAbilitySlotClick;
                _abilitySlotMap[slot] = model;

                slotIndex++;
            }

            foreach (var icon in _view.WeaponIcons)
            {
                var index = _view.WeaponIcons.ToList().IndexOf(icon);
                var weapon = _levelManager.WeaponsMap.Keys.ToList().ElementAtOrDefault(index);

                var visibility = weapon != null;
                icon.gameObject.SetActive(visibility);

                if (!visibility)
                    continue;

                icon.sprite = weapon.Model.Icon;
            }

            foreach (var icon in _view.AbilityIcons)
            {
                var index = _view.AbilityIcons.ToList().IndexOf(icon);
                var ability = _levelManager.Abilities.ToList().ElementAtOrDefault(index);

                var visibility = ability != null;
                icon.gameObject.SetActive(visibility);

                if (!visibility)
                    continue;

                icon.sprite = ability.Model.Icon;
            }
        }

        protected override void Hide()
        {
            foreach (var slot in _weaponSlotMap.Keys)
            {
                slot.ON_CLICK -= OnWeaponSlotClick;
            }
            _weaponSlotMap.Clear();

            foreach (var slot in _abilitySlotMap.Keys)
            {
                slot.ON_CLICK -= OnAbilitySlotClick;
            }
            _abilitySlotMap.Clear();

            foreach (var icon in _view.WeaponIcons)
            {
                icon.sprite = null;
            }

            foreach (var icon in _view.AbilityIcons)
            {
                icon.sprite = null;
            }

            _levelManager.Unpause();
        }

        private void OnWeaponSlotClick(SkillSelectionSlotView slot)
        {
            var model = _weaponSlotMap[slot];
            _gameManager.FireWeaponSelected(model);

            HideHud();
        }

        private void OnAbilitySlotClick(SkillSelectionSlotView slot)
        {
            var model = _abilitySlotMap[slot];
            _gameManager.FireAbilitySelected(model);

            HideHud();
        }

        private void HideHud()
        {
            _hudManager.HideSingle();
        }
    }
}

