using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class SkillSelectionHudView : BaseHud
    {
        [SerializeField] private SkillSelectionSlotView[] _slots;
        [SerializeField] private Image[] _weaponIcons;
        [SerializeField] private Image[] _abilityIcons;
        [SerializeField] private RectTransform _content;
        [SerializeField] private GridLayoutGroup _layoutGroup;

        public SkillSelectionSlotView[] Slots => _slots;
        public Image[] WeaponIcons => _weaponIcons;
        public Image[] AbilityIcons => _abilityIcons;
        public RectTransform Content => _content;
        public GridLayoutGroup LayoutGroup => _layoutGroup;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}

