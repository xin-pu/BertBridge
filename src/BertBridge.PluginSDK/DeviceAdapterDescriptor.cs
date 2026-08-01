namespace BertBridge.PluginSDK;

public sealed record DeviceAdapterDescriptor(
    Type AdapterType,
    string Name,
    string Version,
    string Vendor,
    string SupportedModels,
    string Source)
{
    public static DeviceAdapterDescriptor FromAdapterType(Type adapterType, string source)
    {
        if (!typeof(IDeviceAdapter).IsAssignableFrom(adapterType) || adapterType.IsAbstract)
            throw new ArgumentException("Adapter type must implement IDeviceAdapter.", nameof(adapterType));

        var registration = adapterType
            .GetCustomAttributes(typeof(AdapterRegistrationAttribute), inherit: false)
            .OfType<AdapterRegistrationAttribute>()
            .FirstOrDefault();

        return new DeviceAdapterDescriptor(
            adapterType,
            registration?.Name ?? adapterType.Name,
            registration?.Version ?? "0.0.0",
            registration?.Vendor ?? "Unknown",
            registration?.SupportedModels ?? string.Empty,
            source);
    }
}
