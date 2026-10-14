using System.Collections.Generic;
using UnityEngine;
using Vertigo.Core.Enums;

namespace Vertigo.Core.Interfaces
{
    public interface IWheelConfig
    {
        WheelType WheelType { get; }
        Sprite WheelBaseSprite { get; }
        Sprite IndicatorSprite { get; }
        IReadOnlyList<IWheelSliceData> Slices { get; }
        int SliceCount { get; }
        bool HasBomb { get; }
    }
}
