using Microsoft.Maui.Devices;

namespace slotsi_citas.Services.Api;

public static class ApiConfiguracion
{
    public const int Puerto = 5286;
    public const string IpLocalPc = "172.16.137.93";

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
  
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                host = DeviceInfo.DeviceType == DeviceType.Virtual ? "localhost" : IpLocalPc;
            }
           
            else
            {
                host = "localhost";
            }

            return new Uri($"http://{host}:{Puerto}/");
        }
    }
}