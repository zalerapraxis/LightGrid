﻿using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;

// sourced from https://gist.github.com/UweKeim/fb7f829b852c209557bc49c51ba14c8b

namespace LightGrid.Colors
{
    /// <summary>
    /// Provides color conversion functionality.
    /// </summary>
    /// <remarks>
    /// http://en.wikipedia.org/wiki/HSV_color_space
    /// http://www.easyrgb.com/math.php?MATH=M19#text19
    /// </remarks>
    internal static class ColorConverting
    {
        public static RgbColor ColorToRgb(
            Color color)
        {
            return new RgbColor(color.R, color.G, color.B, color.A);
        }

        public static Color RgbToColor(
            RgbColor rgb)
        {
            return Color.FromArgb((byte) rgb.Alpha, (byte) rgb.Red, (byte) rgb.Green, (byte) rgb.Blue);
        }

        public static Color HslToColor(HslColor hsl)
        {
            return RgbToColor(HslToRgb(hsl));
        }

        public static List<Color> GetGradientFromColor(Color color)
        {
            List<Color> colorList = new List<Color>();

            RgbColor rgbColor = new RgbColor(color.R, color.G, color.B, color.A);
            var hslColor = RgbToHsl(rgbColor);

            colorList.Add(HslToColor(new HslColor(hslColor.Hue - 10, hslColor.Saturation, hslColor.Light, hslColor.Alpha)));
            colorList.Add(HslToColor(new HslColor(hslColor.Hue - 5, hslColor.Saturation, hslColor.Light, hslColor.Alpha)));
            colorList.Add(HslToColor(new HslColor(hslColor.Hue, hslColor.Saturation, hslColor.Light, hslColor.Alpha)));
            colorList.Add(HslToColor(new HslColor(hslColor.Hue + 5, hslColor.Saturation, hslColor.Light, hslColor.Alpha)));
            colorList.Add(HslToColor(new HslColor(hslColor.Hue + 10, hslColor.Saturation, hslColor.Light, hslColor.Alpha)));

            return colorList;
        }

        public static HsbColor RgbToHsb(
            RgbColor rgb)
        {
            // _NOTE #1: Even though we're dealing with a very small range of
            // numbers, the accuracy of all calculations is fairly important.
            // For this reason, I've opted to use double data types instead
            // of float, which gives us a little bit extra precision (recall
            // that precision is the number of significant digits with which
            // the result is expressed).

            var r = rgb.Red / 255d;
            var g = rgb.Green / 255d;
            var b = rgb.Blue / 255d;

            var minValue = getMinimumValue(r, g, b);
            var maxValue = getMaximumValue(r, g, b);
            var delta = maxValue - minValue;

            double hue = 0;
            double saturation;
            var brightness = maxValue * 100;

            if (Math.Abs(maxValue - 0) < 0.0001 || Math.Abs(delta - 0) < 0.0001)
            {
                hue = 0;
                saturation = 0;
            }
            else
            {
                // _NOTE #2: FXCop insists that we avoid testing for floating 
                // point equality (CA1902). Instead, we'll perform a series of
                // tests with the help of Double.Epsilon that will provide 
                // a more accurate equality evaluation.

                if (Math.Abs(minValue - 0) < 0.0001)
                {
                    saturation = 100;
                }
                else
                {
                    saturation = (delta / maxValue) * 100;
                }

                if (Math.Abs(r - maxValue) < Double.Epsilon)
                {
                    hue = (g - b) / delta;
                }
                else if (Math.Abs(g - maxValue) < Double.Epsilon)
                {
                    hue = 2 + (b - r) / delta;
                }
                else if (Math.Abs(b - maxValue) < Double.Epsilon)
                {
                    hue = 4 + (r - g) / delta;
                }
            }

            hue *= 60;
            if (hue < 0)
            {
                hue += 360;
            }

            return new HsbColor(
                hue,
                saturation,
                brightness,
                rgb.Alpha);
        }

        public static HslColor RgbToHsl(
            RgbColor rgb)
        {
            var varR = (rgb.Red / 255.0); //Where RGB values = 0 ÷ 255
            var varG = (rgb.Green / 255.0);
            var varB = (rgb.Blue / 255.0);

            var varMin = getMinimumValue(varR, varG, varB); //Min. value of RGB
            var varMax = getMaximumValue(varR, varG, varB); //Max. value of RGB
            var delMax = varMax - varMin; //Delta RGB value

            double h;
            double s;
            var l = (varMax + varMin) / 2;

            if (Math.Abs(delMax - 0) < 0.0001) //This is a gray, no chroma...
            {
                h = 0; //HSL results = 0 ÷ 1
                s = 0;
                // UK:
                //				s = 1.0;
            }
            else //Chromatic data...
            {
                if (l < 0.5)
                {
                    s = delMax / (varMax + varMin);
                }
                else
                {
                    s = delMax / (2.0 - varMax - varMin);
                }

                var delR = (((varMax - varR) / 6.0) + (delMax / 2.0)) / delMax;
                var delG = (((varMax - varG) / 6.0) + (delMax / 2.0)) / delMax;
                var delB = (((varMax - varB) / 6.0) + (delMax / 2.0)) / delMax;

                if (Math.Abs(varR - varMax) < 0.0001)
                {
                    h = delB - delG;
                }
                else if (Math.Abs(varG - varMax) < 0.0001)
                {
                    h = (1.0 / 3.0) + delR - delB;
                }
                else if (Math.Abs(varB - varMax) < 0.0001)
                {
                    h = (2.0 / 3.0) + delG - delR;
                }
                else
                {
                    // Uwe Keim.
                    h = 0.0;
                }

                if (h < 0.0)
                {
                    h += 1.0;
                }
                if (h > 1.0)
                {
                    h -= 1.0;
                }
            }

            // --

            return new HslColor(
                h * 360.0,
                s * 100.0,
                l * 100.0,
                rgb.Alpha);
        }

        public static RgbColor HsbToRgb(
            HsbColor hsb)
        {
            double red = 0, green = 0, blue = 0;

            double h = hsb.Hue;
            var s = ((double)hsb.Saturation) / 100;
            var b = ((double)hsb.Brightness) / 100;

            if (Math.Abs(s - 0) < 0.0001)
            {
                red = b;
                green = b;
                blue = b;
            }
            else
            {
                // the color wheel has six sectors.

                var sectorPosition = h / 60;
                var sectorNumber = (int)Math.Floor(sectorPosition);
                var fractionalSector = sectorPosition - sectorNumber;

                var p = b * (1 - s);
                var q = b * (1 - (s * fractionalSector));
                var t = b * (1 - (s * (1 - fractionalSector)));

                // Assign the fractional colors to r, g, and b
                // based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        red = b;
                        green = t;
                        blue = p;
                        break;

                    case 1:
                        red = q;
                        green = b;
                        blue = p;
                        break;

                    case 2:
                        red = p;
                        green = b;
                        blue = t;
                        break;

                    case 3:
                        red = p;
                        green = q;
                        blue = b;
                        break;

                    case 4:
                        red = t;
                        green = p;
                        blue = b;
                        break;

                    case 5:
                        red = b;
                        green = p;
                        blue = q;
                        break;
                }
            }

            var nRed = (int)Math.Round(red * 255);
            var nGreen = (int)Math.Round(green * 255);
            var nBlue = (int)Math.Round(blue * 255);

            return new RgbColor(nRed, nGreen, nBlue, hsb.Alpha);
        }

        public static RgbColor HslToRgb(
            HslColor hsl)
        {
            double red, green, blue;

            var h = hsl.PreciseHue / 360.0;
            var s = hsl.PreciseSaturation / 100.0;
            var l = hsl.PreciseLight / 100.0;

            if (Math.Abs(s - 0.0) < 0.0001)
            {
                red = l;
                green = l;
                blue = l;
            }
            else
            {
                double var2;

                if (l < 0.5)
                {
                    var2 = l * (1.0 + s);
                }
                else
                {
                    var2 = (l + s) - (s * l);
                }

                var var1 = 2.0 * l - var2;

                red = hue2Rgb(var1, var2, h + (1.0 / 3.0));
                green = hue2Rgb(var1, var2, h);
                blue = hue2Rgb(var1, var2, h - (1.0 / 3.0));
            }

            // --

            var nRed = (int)Math.Round(red * 255.0);
            var nGreen = (int)Math.Round(green * 255.0);
            var nBlue = (int)Math.Round(blue * 255.0);

            return new RgbColor(nRed, nGreen, nBlue, hsl.Alpha);
        }

        private static double hue2Rgb(
            double v1,
            double v2,
            double vH)
        {
            if (vH < 0.0)
            {
                vH += 1.0;
            }
            if (vH > 1.0)
            {
                vH -= 1.0;
            }
            if ((6.0 * vH) < 1.0)
            {
                return (v1 + (v2 - v1) * 6.0 * vH);
            }
            if ((2.0 * vH) < 1.0)
            {
                return (v2);
            }
            if ((3.0 * vH) < 2.0)
            {
                return (v1 + (v2 - v1) * ((2.0 / 3.0) - vH) * 6.0);
            }

            return (v1);
        }

        /// <summary>
        /// Determines the maximum value of all of the numbers provided in the
        /// variable argument list.
        /// </summary>
        private static double getMaximumValue(
            params double[] values)
        {
            var maxValue = values[0];

            if (values.Length >= 2)
            {
                for (var i = 1; i < values.Length; i++)
                {
                    var num = values[i];
                    maxValue = Math.Max(maxValue, num);
                }
            }

            return maxValue;
        }

        /// <summary>
        /// Determines the minimum value of all of the numbers provided in the
        /// variable argument list.
        /// </summary>
        private static double getMinimumValue(
            params double[] values)
        {
            var minValue = values[0];

            if (values.Length >= 2)
            {
                for (var i = 1; i < values.Length; i++)
                {
                    var num = values[i];
                    minValue = Math.Min(minValue, num);
                }
            }

            return minValue;
        }
    }
}
