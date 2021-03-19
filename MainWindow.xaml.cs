using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LightGrid.Colors;
using YeelightAPI.Models;
using YeelightAPI.Models.ColorFlow;

namespace LightGrid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SettingsManager<UserSettings> settingsManager;
        private UserSettings userSettings;

        public MainWindow()
        {
            InitializeComponent();
            
            // load up user settings
            settingsManager = new SettingsManager<UserSettings>();
            userSettings = settingsManager.LoadSettings();

            Task.Run(async () => await YeelightHelper.InitializeYeelights());

            RefreshButtons();
        }

        /// <summary>
        /// Sets the color of the lights to the color of the given button
        /// </summary>
        private async void Btn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var buttonBgColor = (button.Background as SolidColorBrush).Color;

            var colors = ColorConverting.GetGradientFromColor(buttonBgColor);

            //YeelightHelper.SetRGBColor(buttonBgColor.R, buttonBgColor.G, buttonBgColor.B, 250);
            await YeelightHelper.StartRandomColorFlow(colors, 5000);
        }

        /// <summary>
        /// Adds/removes the background color of the given button to/from the user favorites list
        /// </summary>
        private void Btn_RightClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var buttonBgColor = button.Background as SolidColorBrush;

            var grid = button.Parent as UniformGrid;
            var buttonIndex = grid.Children.IndexOf(button);

            var favoriteColorExists = CheckIfFavoriteColorExists(buttonIndex);

            if (!favoriteColorExists)
            {
                // add color to favorites
                userSettings.favoriteColors.Add(new ColorValues
                {
                    R = buttonBgColor.Color.R,
                    G = buttonBgColor.Color.G,
                    B = buttonBgColor.Color.B
                });
            }
            else
            {
                // remove color from favorites
                userSettings.favoriteColors.RemoveAt(buttonIndex);
            }


            settingsManager.SaveSettings(userSettings);
        }

        /// <summary>
        /// Refresh the color grid, randomizing the available colors in the colorgrid
        /// </summary>
        private void BtnRefreshColors_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshButtons();
        }

        /// <summary>
        /// Turn the lights off and on
        /// </summary>
        private void BtnToggleOnOff_OnClick(object sender, RoutedEventArgs e)
        {
            YeelightHelper.bulbs.Toggle();
        }

        /// <summary>
        /// Set the lights to 2700K color temperature - aka default lighting
        /// </summary>
        private void BtnSet2700k_OnClick(object sender, RoutedEventArgs e)
        {
            YeelightHelper.SetColorTemperature(2700, 250);
        }

        /// <summary>
        /// Toggle music mode
        /// </summary>
        private void BtnMusicMode_OnClick(object sender, RoutedEventArgs e)
        {
            if (!MusicMode.Enabled)
            {
                btnMusicMode.Background = Brushes.Gold;
                Task.Run((() => MusicMode.Start()));
            }
            else
            {
                btnMusicMode.Background = new SolidColorBrush(Color.FromRgb(221, 221, 221));
                Task.Run((() => MusicMode.Stop()));
            }
        }

        /// <summary>
        /// Attempt to reconnect to the lights
        /// </summary>
        private async void BtnReconnectLights_OnClick(object sender, RoutedEventArgs e)
        {
            await YeelightHelper.InitializeYeelights();
            YeelightHelper.bulbs.TurnOn();
        }

        private async void btnColorFlow_Rainbow_OnClick(object sender, RoutedEventArgs e)
        {
            FluentFlow flow = await YeelightHelper.bulbs.Flow()
                .RgbColor(255, 0, 0, 100, 5000) // red
                // .RgbColor(255, 255, 0, 100, 5000) // yellow
                .RgbColor(0, 255, 0, 100, 5000) // green
                .RgbColor(0, 255, 255, 100, 5000) // aqua
                .RgbColor(0, 0, 255, 100, 5000) // blue
                .RgbColor(255, 0, 255, 100, 5000) // magenta
                .Play(ColorFlowEndAction.Keep);
        }
        


        /// <summary>
        /// Clear all buttons in grid & generate new ones with new colors.
        /// First entries in the grid will be colors marked as favorites by the user.
        /// </summary>
        private void RefreshButtons()
        {
            uniformGridOfColors.Children.Clear();

            // refresh settings in case of a manual settings edit
            userSettings = settingsManager.LoadSettings();

            // build buttons
            var rng = new Random();
            var i = 0;

            while (i <= 24)
            {
                Color color = new Color();
                bool favoriteColorExists = CheckIfFavoriteColorExists(i);

                if (favoriteColorExists)
                    // get color from favorites
                    color = GetFavoriteColor(i);
                else
                {
                    // generate random high-intensity color
                    var hsl = new HslColor(rng.Next(360), rng.Next(50, 100), 255, 255);
                    color = ColorConverting.HslToColor(hsl);
                }

                // generate empty button, set its background to above color
                var btn = new Button();
                btn.Background = new SolidColorBrush(color);

                // favorite colors get gold borders
                if (favoriteColorExists)
                {
                    btn.BorderBrush = new SolidColorBrush(System.Windows.Media.Colors.Gold);
                    btn.BorderThickness = new Thickness(2);
                }

                // bind mouseclicks to functions
                btn.Click += Btn_Click;
                btn.MouseRightButtonDown += Btn_RightClick;

                // add the button to the grid
                uniformGridOfColors.Children.Add(btn);

                i++;
            }
        }


        /// <summary>
        /// Checks the user favorites list to see if it contains a color at that index.
        /// </summary>
        /// <param name="i">The index of the user favorites list to check</param>
        /// <returns>boolean corresponding to whether or not a color exists at the given index in the user favorites list</returns>
        private bool CheckIfFavoriteColorExists(int i)
        {
            if (userSettings.favoriteColors.ElementAtOrDefault(i) != null)
                return true;

            return false;
        }

        /// <summary>
        /// Gets the Color object of the color at the given index in the user favorites list
        /// </summary>
        /// <param name="i">The index of the user favorites list to grab the color values from</param>
        /// <returns>a Color object using the colors from the user favorites list at the given index</returns>
        private Color GetFavoriteColor(int i)
        {
            var favoriteColor = userSettings.favoriteColors[i];

            var color = Color.FromRgb((byte)favoriteColor.R, (byte)favoriteColor.G, (byte)favoriteColor.B);

            return color;
        }
    }
}
