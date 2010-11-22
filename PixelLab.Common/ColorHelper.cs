using System;
using System.Diagnostics.Contracts;
using System.Windows.Media;

namespace PixelLab.Common
{
    public static class ColorHelper
    {
        public static Color HsbToRgb(float hue, float saturation, float brightness)
        {
            Contract.Requires(hue >= 0f && hue <= 1f);
            Contract.Requires(saturation >= 0f && saturation <= 1f);
            Contract.Requires(brightness >= 0f && brightness <= 1f);

            float red = 0, green = 0, blue = 0;

            if (saturation == 0f)
            {
                red = green = blue = brightness;
            }
            else
            {
                float num = (float)((hue - Math.Floor((double)hue)) * 6.0);
                int num2 = (int)num;
                float num3 = num - num2;
                float num4 = brightness * (1f - saturation);
                float num5 = brightness * (1f - (saturation * num3));
                float num6 = brightness * (1f - (saturation * (1f - num3)));
                switch ((num2 % 6))
                {
                    case 0:
                        red = brightness;
                        green = num6;
                        blue = num4;
                        break;

                    case 1:
                        red = num5;
                        green = brightness;
                        blue = num4;
                        break;

                    case 2:
                        red = num4;
                        green = brightness;
                        blue = num6;
                        break;

                    case 3:
                        red = num4;
                        green = num5;
                        blue = brightness;
                        break;

                    case 4:
                        red = num6;
                        green = num4;
                        blue = brightness;
                        break;

                    case 5:
                        red = brightness;
                        green = num4;
                        blue = num5;
                        break;
                }

            }

            red = Math.Min(1f, Math.Max(0f, red));
            green = Math.Min(1f, Math.Max(0f, green));
            blue = Math.Min(1f, Math.Max(0f, blue));

#if SILVERLIGHT
            return Color.FromArgb(255, (byte)(red / 255), (byte)(green / 255), (byte)(blue / 255));
#else
      return Color.FromScRgb(1, red, green, blue);
#endif
        }


    }
}
