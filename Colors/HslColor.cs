using System;
using System.Windows.Media;

// sourced from https://gist.github.com/UweKeim/fb7f829b852c209557bc49c51ba14c8b

namespace LightGrid.Colors
{
    /// <summary>
    /// Represents a HSL color space.
    /// http://en.wikipedia.org/wiki/HSV_color_space
    /// </summary>
    public sealed class HslColor
    {
        public HslColor(
            double hue,
            double saturation,
            double light,
            int alpha)
        {
            PreciseHue = hue;
            PreciseSaturation = saturation;
            PreciseLight = light;
            Alpha = alpha;
        }

        public HslColor(
            int hue,
            int saturation,
            int light,
            int alpha)
        {
            PreciseHue = hue;
            PreciseSaturation = saturation;
            PreciseLight = light;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets or sets the hue. Values from 0 to 360.
        /// </summary>
        // // // [UsedImplicitly]
        public int Hue
        {
            get => (int)PreciseHue;
            set => PreciseHue = value;
        }

        /// <summary>
        /// Gets or sets the precise hue. Values from 0 to 360.
        /// </summary>
        // // [UsedImplicitly]
        public double PreciseHue { get; set; }

        /// <summary>
        /// Gets or sets the saturation. Values from 0 to 100.
        /// </summary>
        // // [UsedImplicitly]
        public int Saturation
        {
            get => (int)PreciseSaturation;
            set => PreciseSaturation = value;
        }

        /// <summary>
        /// Gets or sets the precise saturation. Values from 0 to 100.
        /// </summary>
        public double PreciseSaturation { get; set; }

        /// <summary>
        /// Gets or sets the light. Values from 0 to 100.
        /// </summary>
        // // [UsedImplicitly]
        public int Light
        {
            get => (int)PreciseLight;
            set => PreciseLight = value;
        }

        /// <summary>
        /// Gets or sets the precise light. Values from 0 to 100.
        /// </summary>
        public double PreciseLight { get; set; }

        /// <summary>
        /// Gets or sets the alpha. Values from 0 to 255
        /// </summary>
        public int Alpha { get; set; }

        public static HslColor FromColor(
            Color color)
        {
            return ColorConverting.RgbToHsl(
                ColorConverting.ColorToRgb(color));
        }

        // // [UsedImplicitly]
        public static HslColor FromRgbColor(
            RgbColor color)
        {
            return color.ToHslColor();
        }

        // // [UsedImplicitly]
        public static HslColor FromHslColor(
            HslColor color)
        {
            return new HslColor(
                color.PreciseHue,
                color.PreciseSaturation,
                color.PreciseLight,
                color.Alpha);
        }

        // // [UsedImplicitly]
        public static HslColor FromHsbColor(
            HsbColor color)
        {
            return FromRgbColor(color.ToRgbColor());
        }

        public override string ToString()
        {
            return Alpha < 255
                ? $@"hsla({Hue}, {Saturation}%, {Light}%, {Alpha / 255f})"
                : $@"hsl({Hue}, {Saturation}%, {Light}%)";
        }

        public Color ToColor()
        {
            return ColorConverting.HslToRgb(this).ToColor();
        }

        public RgbColor ToRgbColor()
        {
            return ColorConverting.HslToRgb(this);
        }

        public HslColor ToHslColor()
        {
            return this;
        }

        // // [UsedImplicitly]
        public HsbColor ToHsbColor()
        {
            return ColorConverting.RgbToHsb(ToRgbColor());
        }

        public override bool Equals(
            object obj)
        {
            var equal = false;

            if (obj is HslColor color)
            {
                var hsb = color;

                if (Math.Abs(Hue - hsb.PreciseHue) < 0.0001 &&
                    Math.Abs(Saturation - hsb.PreciseSaturation) < 0.0001 &&
                    Math.Abs(Light - hsb.PreciseLight) < 0.0001)
                {
                    equal = true;
                }
            }

            return equal;
        }

        public override int GetHashCode()
        {
            return $@"H:{PreciseHue}-S:{PreciseSaturation}-L:{PreciseLight}".GetHashCode();
        }
    }
}