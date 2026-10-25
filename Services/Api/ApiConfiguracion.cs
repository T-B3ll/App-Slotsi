using Microsoft.Maui.Devices;

namespace slotsi_citas.Services.Api;

public static class ApiConfiguracion
{
    public const int Puerto = 5286;
    public const string IpLocalPc = "192.168.56.1";

    public static Uri UrlBase
    {
        get
        {
            string host;

            // 1. Si estamos en Android...
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                // 10.0.2.2 es la dirección mágica que usa el EMULADOR de Android 
                // para llegar al localhost de la PC.
                // Si estás en un celular FÍSICO, usará tu IP real.
                if (DeviceInfo.DeviceType == DeviceType.Virtual)
                {
                    host = "10.0.2.2";
                }
                else
                {
                    host = IpLocalPc;
                }
            }
            // 2. Si es iOS
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                host = DeviceInfo.DeviceType == DeviceType.Virtual ? "localhost" : IpLocalPc;
            }
            // 3. Windows/Mac
            else
            {
                host = "localhost";
            }

            return new Uri($"http://{host}:{Puerto}/");
        }
    }
}