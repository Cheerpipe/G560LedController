using Device.Net;
using Hid.Net.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace G560Led
{
    public static class G560LedControllerLoader
    {
        private static IDevice[] _devices;

        private static G560LedController[] _G560LedController;
        public static G560LedController[] Devices { get => _G560LedController; }

        public static async Task InitDevice(int deviceIndex)
        {
            WindowsHidDeviceFactory.Register(null, null);
            List<FilterDeviceDefinition> deviceDefinitions = new List<FilterDeviceDefinition>();
            FilterDeviceDefinition d = new FilterDeviceDefinition { DeviceType = DeviceType.Hid, VendorId = 0x046D, ProductId = 0x0A78, Label = "Logitech G560 Speakers" };
            deviceDefinitions.Add(d);
            List<IDevice> devices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitions);
            _devices = devices.ToArray();
            _G560LedController = new G560LedController[_devices.Length];
            _G560LedController[deviceIndex] = new G560LedController(_devices[deviceIndex]);
        }
    }
}
