using System.Collections.Generic;
using UnityEngine;
using Vertigo.Core.Enums;
using Vertigo.Core.Interfaces;

namespace Vertigo.Wheel.Data
{
    [CreateAssetMenu(fileName = "WheelConfig_", menuName = "Vertigo/Wheel/Wheel Config")]
    public class WheelConfigSO : ScriptableObject, IWheelConfig
    {
        public const int DefaultSliceCount = 8;

        [Header("Wheel Appearance")]
        [SerializeField] private WheelType wheelType = WheelType.BronzeNormal;
        [SerializeField] private Sprite wheelBaseSprite;
        [SerializeField] private Sprite indicatorSprite;

        [Header("Wheel Slices (Strictly 8 slices)")]
        [SerializeField] private List<WheelSliceData> slices = new List<WheelSliceData>(DefaultSliceCount);

        public WheelType WheelType => wheelType;
        public Sprite WheelBaseSprite => wheelBaseSprite;
        public Sprite IndicatorSprite => indicatorSprite;
        public IReadOnlyList<IWheelSliceData> Slices => slices;
        public IReadOnlyList<WheelSliceData> ConcreteSlices => slices;
        public int SliceCount => slices != null ? slices.Count : 0;

        public bool HasBomb
        {
            get
            {
                if (slices == null) return false;
                for (int i = 0; i < slices.Count; i++)
                {
                    if (slices[i]?.Reward != null && slices[i].Reward.IsBomb)
                        return true;
                }
                return false;
            }
        }

        public WheelSliceData GetSlice(int index)
        {
            if (slices == null || index < 0 || index >= slices.Count)
                return null;
            return slices[index];
        }

        private void OnValidate()
        {
            if (slices == null)
            {
                slices = new List<WheelSliceData>();
            }

            while (slices.Count < DefaultSliceCount)
            {
                slices.Add(new WheelSliceData(slices.Count, null, 1));
            }

            for (int i = 0; i < slices.Count; i++)
            {
                if (slices[i] != null)
                {
                    slices[i].SetIndex(i);
                }
            }
        }
    }
}
