using System;
using System.Windows.Media;

// sourced from https://gist.github.com/UweKeim/fb7f829b852c209557bc49c51ba14c8b

namespace LightGrid.Colors
{
    /// <summary>
    /// Represents a HSV (=HSB) color space.
    /// http://en.wikipedia.org/wiki/HSV_color_space
    /// </summary>
    public sealed class HsbColor
    {
        public HsbColor(
            double hue,
            double saturation,
            double brightness,
            int alpha)
        {
            PreciseHue = hue;
            PreciseSaturation = saturation;
            PreciseBrightness = brightness;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets or sets the hue. Values from 0 to 360.
        /// </summary>
        //// // [UsedImplicitly]
        public double PreciseHue { get; set; }

        /// <summary>
        /// Gets or sets the saturation. Values from 0 to 100.
        /// </summary>
        //// // [UsedImplicitly]
        public double PreciseSaturation { get; set; }

        /// <summary>
        /// Gets or sets the brightness. Values from 0 to 100.
        /// </summary>
        //// // [UsedImplicitly]
        public double PreciseBrightness { get; set; }

        //// // [UsedImplicitly]
        public int Hue
        {
            get => (int)PreciseHue;
            set => PreciseHue = value;
        }

        //// // [UsedImplicitly]
        public int Saturation
        {
            get => (int)PreciseSaturation;
            set => PreciseSaturation = value;
        }

        //// // [UsedImplicitly]
        public int Brightness
        {
            get => (int)PreciseBrightness;
            set => PreciseBrightness = value;
        }

        /// <summary>
        /// Gets or sets the alpha. Values from 0 to 255.
        /// </summary>
        //// // [UsedImplicitly]
        public int Alpha { get; set; }

        //// // [UsedImplicitly]
        public static HsbColor FromColor(
            Color color)
        {
            return ColorConverting.ColorToRgb(color).ToHsbColor();
        }

        //// // [UsedImplicitly]
        public static HsbColor FromRgbColor(
            RgbColor color)
        {
            return color.ToHsbColor();
        }

        //// // [UsedImplicitly]
        public static HsbColor FromHsbColor(
            HsbColor color)
        {
            return new HsbColor(color.PreciseHue, color.PreciseSaturation, color.PreciseBrightness, color.Alpha);
        }

        //// // [UsedImplicitly]
        public static HsbColor FromHslColor(
            HslColor color)
        {
            return FromRgbColor(color.ToRgbColor());
        }

        //// // [UsedImplicitly]
        public override string ToString()
        {
            return $@"Hue: {Hue}; saturation: {Saturation}; brightness: {Brightness}.";
        }

        //// // [UsedImplicitly]
        public Color ToColor()
        {
            return ColorConverting.HsbToRgb(this).ToColor();
        }

        //// // [UsedImplicitly]
        public RgbColor ToRgbColor()
        {
            return ColorConverting.HsbToRgb(this);
        }

        //// // [UsedImplicitly]
        public HsbColor ToHsbColor()
        {
            return new HsbColor(PreciseHue, PreciseSaturation, PreciseBrightness, Alpha);
        }

        //// // [UsedImplicitly]
        public HslColor ToHslColor()
        {
            return ColorConverting.RgbToHsl(ToRgbColor());
        }

        public override bool Equals(
            object obj)
        {
            var equal = false;

            if (obj is HsbColor color)
            {
                var hsb = color;

                if (Math.Abs(PreciseHue - hsb.PreciseHue) < 0.001 &&
                    Math.Abs(PreciseSaturation - hsb.PreciseSaturation) < 0.001 &&
                    Math.Abs(PreciseBrightness - hsb.PreciseBrightness) < 0.001)
                {
                    equal = true;
                }
            }

            return equal;
        }

        public override int GetHashCode()
        {
            return $@"H:{Hue}-S:{Saturation}-B:{Brightness}-A:{Alpha}".GetHashCode();
        }
    }
}