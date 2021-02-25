using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YeelightAPI;

namespace LightGrid
{
    static class YeelightHelper
    {
        public static DeviceGroup bulbs = new DeviceGroup();

        public static async Task InitializeYeelights()
        {
            DeviceLocator.MaxRetryCount = 2;

            // find bulbs on the network
            var devices = await DeviceLocator.DiscoverAsync();

            // once we collect devices on the network, add them to bulbs list,
            // connect to them, turn them on, set their brightness to max
            foreach (var device in devices)
            {
                bulbs.Add(device);

                // device.OnNotificationReceived += DeviceOnOnNotificationReceived;
                device.OnError += DeviceOnError;

                device.Connect();
                device.TurnOn();
                device.SetBrightness(100);
            }
        }


        /// <summary>
        /// Report any errors with a bulb.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DeviceOnError(object sender, UnhandledExceptionEventArgs e)
        {
            // Console.WriteLine($"[Yeelight] Error: {e.ExceptionObject}");
        }

        private static void DeviceOnOnNotificationReceived(object sender, NotificationReceivedEventArgs e)
        {
            // Console.WriteLine($"[Yeelight] Notification: {JsonConvert.SerializeObject(e.Result)}");
        }
    }
}
