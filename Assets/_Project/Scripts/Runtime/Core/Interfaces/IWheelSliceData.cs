namespace Vertigo.Core.Interfaces
{
    public interface IWheelSliceData
    {
        int SliceIndex { get; }
        IRewardData Reward { get; }
        int Amount { get; }
        float Weight { get; }
    }
}
