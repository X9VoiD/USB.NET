namespace USB.NET.Platform.Windows.Enumerators.USB
{
    public enum ConnectionStatus : uint
    {
        NoDeviceConnected,
        DeviceConnected,
        DeviceFailedEnumeration,
        DeviceGeneralFailure,
        DeviceCausedOvercurrent,
        DeviceNotEnoughPower,
        DeviceNotEnoughBandwidth,
        DeviceHubNestedTooDeeply,
        DeviceInLegacyHub,
        DeviceEnumerating,
        DeviceReset
    }
}