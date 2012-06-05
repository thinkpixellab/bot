using System;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
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
            return Color.FromArgb(255, (byte)(red * 255), (byte)(green * 255), (byte)(blue * 255));
#else
            return Color.FromScRgb(1, red, green, blue);
#endif
        }

        public static Color ParseHexColor(string colorString)
        {
            Contract.Requires(!colorString.IsNullOrWhiteSpace(), "colorString");
            Contract.Requires(((colorString.Length == 4) || (colorString.Length == 5) || (colorString.Length == 7) || (colorString.Length == 9)) && (colorString[0] == '#'), "colorString");

            int num;
            int num2;
            int num3;
            int num4 = 255;
            if (colorString.Length > 7)
            {
                num4 = (ParseHexChar(colorString[1]) * 16) + ParseHexChar(colorString[2]);
                num3 = (ParseHexChar(colorString[3]) * 16) + ParseHexChar(colorString[4]);
                num2 = (ParseHexChar(colorString[5]) * 16) + ParseHexChar(colorString[6]);
                num = (ParseHexChar(colorString[7]) * 16) + ParseHexChar(colorString[8]);
            }
            else if (colorString.Length > 5)
            {
                num3 = (ParseHexChar(colorString[1]) * 16) + ParseHexChar(colorString[2]);
                num2 = (ParseHexChar(colorString[3]) * 16) + ParseHexChar(colorString[4]);
                num = (ParseHexChar(colorString[5]) * 16) + ParseHexChar(colorString[6]);
            }
            else if (colorString.Length > 4)
            {
                num4 = ParseHexChar(colorString[1]);
                num4 += num4 * 16;
                num3 = ParseHexChar(colorString[2]);
                num3 += num3 * 16;
                num2 = ParseHexChar(colorString[3]);
                num2 += num2 * 16;
                num = ParseHexChar(colorString[4]);
                num += num * 16;
            }
            else
            {
                num3 = ParseHexChar(colorString[1]);
                num3 += num3 * 16;
                num2 = ParseHexChar(colorString[2]);
                num2 += num2 * 16;
                num = ParseHexChar(colorString[3]);
                num += num * 16;
            }
            return Color.FromArgb((byte)num4, (byte)num3, (byte)num2, (byte)num);
        }

        private static int ParseHexChar(char c)
        {
            int num = c;
            if ((num >= 48) && (num <= 57))
            {
                return (num - 48);
            }
            if ((num >= 97) && (num <= 102))
            {
                return ((num - 97) + 10);
            }
            if ((num < 65) || (num > 70))
            {
                throw new FormatException();
            }
            return ((num - 65) + 10);
        }
    }
}
