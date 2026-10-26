using Microsoft.Maui.Devices;

namespace slotsi_citas.Services.Api;

public static class ApiConfiguracion
{
    public const int Puerto = 5286;
    // IP local de la PC
    public const string IpLocalPc = "10.71.61.124";

    public static Uri UrlBase
    {
        get
        {
            string host;

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
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