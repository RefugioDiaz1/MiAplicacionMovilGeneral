#if ANDROID
using Android.App;
using Android.Content;
using Android.Provider;
using MiAppMovil.Interfaces;

namespace MiAppMovil.Platforms.Android
{
    public class DeviceIdProvider : IDeviceIdProvider
    {
        public string GetDeviceId()
        {
            var context = Android.App.Application.Context;
            return Settings.Secure.GetString(context.ContentResolver, Settings.Secure.AndroidId);
        }
    }
}
#endif
