using Vertigo.Core.Enums;

namespace Vertigo.Core.Interfaces
{
    public interface IZoneConfig
    {
        int ZoneNumber { get; }
        ZoneType ZoneType { get; }
        IWheelConfig WheelConfig { get; }
        bool CanCashOut { get; }
    }
}
