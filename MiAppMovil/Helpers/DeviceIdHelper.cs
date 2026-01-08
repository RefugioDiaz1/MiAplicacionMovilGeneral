using Microsoft.Maui.Storage;

namespace MiAppMovil.Helpers
{
    public static class DeviceIdHelper
    {
        const string DeviceIdKey = "unique_device_id";

        public static string GetOrCreateDeviceId()
        {
            if (Preferences.ContainsKey(DeviceIdKey))
            {
                return Preferences.Get(DeviceIdKey, string.Empty);
            }
            else
            {
                var newId = Guid.NewGuid().ToString();
                Preferences.Set(DeviceIdKey, newId);
                return newId;
            }
        }
    }
}

