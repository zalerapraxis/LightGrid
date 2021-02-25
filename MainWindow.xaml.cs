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
using YeelightAPI.Models;

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
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var buttonBgColor = (button.Background as SolidColorBrush).Color;

            YeelightHelper.bulbs.SetRGBColor(buttonBgColor.R, buttonBgColor.G, buttonBgColor.B, 250);
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
        /// Refresh the color grid
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
            YeelightHelper.bulbs.SetColorTemperature(2700, 250);
        }

        private void BtnMusicMode_OnClick(object sender, RoutedEventArgs e)
        {
            if (!MusicMode.Enabled)
            {

                Task.Run((() => MusicMode.Start()));
            }
            else
            {

                Task.Run((() => MusicMode.Stop()));
            }
                
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
                    // generate random high-intensity color
                    // randomize the hue and saturation values, but the lightness values (for brightness) should always be max
                    color = HSLColor.ColorFromHSL(rng.NextDouble(), rng.NextDouble(), 255);

                // generate empty button, set its background to above color
                var btn = new Button();
                btn.Background = new SolidColorBrush(color);

                // favorite colors get gold borders
                if (favoriteColorExists)
                {
                    btn.BorderBrush = new SolidColorBrush(Colors.Gold);
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
