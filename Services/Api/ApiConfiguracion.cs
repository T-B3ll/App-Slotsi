namespace slotsi_citas.Services.Api;

public static class ApiConfiguracion
{
    public const int Puerto = 5286;
    public const string IpLocalPc = "172.16.137.31";

    public static Uri UrlBase
    {
        get
        {
            bool esDispositivoFisico = DeviceInfo.DeviceType == DeviceType.Physical &&
                                       (DeviceInfo.Platform == DevicePlatform.Android ||
                                        DeviceInfo.Platform == DevicePlatform.iOS);
            string host;
            if (esDispositivoFisico)
                host = IpLocalPc;
            else
                host = "localhost";

            return new Uri($"http://{host}:{Puerto}/");
        }
    }
}