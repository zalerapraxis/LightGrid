using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using YeelightAPI;
using YeelightAPI.Models;
using YeelightAPI.Models.ColorFlow;

namespace LightGrid
{
    static class YeelightHelper
    {
        public static DeviceGroup bulbs = new DeviceGroup();

        private static bool RandomColorFlowRunning = false;
        private static Timer ColorFlowTimer = new Timer();

        /// <summary>
        /// Locate all Yeelight bulbs in the network & connect to them
        /// </summary>
        /// <returns></returns>
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
        /// Set the RGB color for all bulbs. Stops any ongoing colorflow timers.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="smooth">How fast to perform the color transition, in milliseconds</param>
        public static void SetRGBColor(int r, int g, int b, int? smooth = null)
        {
            if (RandomColorFlowRunning)
            {
                RandomColorFlowRunning = false;
                ColorFlowTimer.Stop();
            }

            bulbs.SetRGBColor(r, g, b, smooth);
        }

        /// <summary>
        /// Sets the color temperature for all bulbs. Stops any ongoing colorflow timers.
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="smooth"></param>
        public static void SetColorTemperature(int temperature, int? smooth = null)
        {
            if (RandomColorFlowRunning)
            {
                RandomColorFlowRunning = false;
                ColorFlowTimer.Stop();
            }

            bulbs.SetColorTemperature(temperature, smooth);

        }

        /// <summary>
        /// Create a color flow from a list of colors, with the order of colors being random
        /// </summary>
        /// <param name="colors">List of Color objects to use in the flow</param>
        /// <param name="duration">The length of time the transition to this color should take, in milliseconds</param>
        /// <returns></returns>
        public static async Task StartRandomColorFlow(List<Color> colors, int colorDuration)
        {
            RandomColorFlowRunning = true;

            ColorFlowTimer.Elapsed += (Sender, e) => ColorFlowTimerOnElapsed(Sender, e, colors, colorDuration);
            ColorFlowTimer.Start();
        }

        private static async void ColorFlowTimerOnElapsed(object sender, ElapsedEventArgs e, List<Color> colors, int colorDuration)
        {
            var totalDuration = colors.Count * colorDuration;
            ColorFlowTimer.Interval = totalDuration;

            var flow = new ColorFlow(0, ColorFlowEndAction.Keep);

            colors = colors.OrderBy(a => new Random().Next()).ToList();

            foreach (var color in colors)
            {
                flow.Add(new ColorFlowRGBExpression(color.R, color.G, color.B, 100, colorDuration));
            }

            await bulbs.StartColorFlow(flow);
        }


        /// <summary>
        /// Report any errors with a bulb
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
