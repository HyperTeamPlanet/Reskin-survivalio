using System;
using System.Collections.Generic;
using Game.Config;
using Game.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Player
{
    public sealed class PlayerView : UnitView
    {
        [SerializeField] private Image _health;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private SkinnedMeshRenderer _full;
        [SerializeField] private SkinnedMeshRenderer _head;
        [SerializeField] private SkinnedMeshRenderer _helmet;
        [SerializeField] private SkinnedMeshRenderer _vest;
        [SerializeField] private SkinnedMeshRenderer _uniform;
        [SerializeField] private SkinnedMeshRenderer _gloves;
        [SerializeField] private SkinnedMeshRenderer _shoes;

        private Dictionary<ClothElementType, SkinnedMeshRenderer> _renderers;
        protected override void OnModelChanged(UnitModel model)
        {
            var playerModel = model as PlayerModel;

            var health = playerModel.GetAttribute(UnitAttributeType.Health);
            _health.fillAmount = (float)health / playerModel.HealthNominal;
            _healthText.text = health.ToString();

            bool hasAllClothMeshes = playerModel.HasAllClothMeshes;
            if(!hasAllClothMeshes)
            {
                _full.sharedMesh = playerModel.FullSkinnedMesh;
                _head.sharedMesh = null;
                return;
            }

            _renderers = new Dictionary<ClothElementType, SkinnedMeshRenderer>()
            {
                {ClothElementType.Helmet, _helmet},
                {ClothElementType.Vest, _vest},
                {ClothElementType.Uniform, _uniform},
                {ClothElementType.Gloves, _gloves},
                {ClothElementType.Shoes, _shoes}
            };

            foreach (var skinnedMeshRenderer in _renderers)
            {
                skinnedMeshRenderer.Value.sharedMesh = playerModel.ClothMeshMap[skinnedMeshRenderer.Key];
                var material = playerModel.ClothMaterialMap[skinnedMeshRenderer.Key];
                if (material != null)
                    skinnedMeshRenderer.Value.material = playerModel.ClothMaterialMap[skinnedMeshRenderer.Key];
            }
        }
    }
}

