using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TextureMerge
{
    /// <summary>
    /// Interakční logika pro ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        private const int decimalPlaces = 1;
        public Color PickedColor { get; private set; }

        private bool canChange = true;

        public ColorPicker(Color initialColor)
        {
            InitializeComponent();
            PickedColor = initialColor;
            RValue.Text = initialColor.R.ToString();
            GValue.Text = initialColor.G.ToString();
            BValue.Text = initialColor.B.ToString();
        }

        private void RGBChanged(object sender, TextChangedEventArgs e)
        {
            if (canChange && GetRGB(out byte R, out byte G, out byte B))
            {
                canChange = false;
                RGBToHSV(R, G, B, out double H, out double S, out double V);
                RGBToHex(R, G, B, out string hex);
                HValue.Text = H.ToStringRounded(decimalPlaces);
                SValue.Text = S.ToStringRounded(decimalPlaces);
                VValue.Text = V.ToStringRounded(decimalPlaces);
                HexValue.Text = hex;
                canChange = true;
            }
            UpdateColorView();
        }

        private void HSVChanged(object sender, TextChangedEventArgs e)
        {
            if (canChange && GetHSV(out double H, out double S, out double V))
            {
                canChange = false;
                HSVToRGB(H, S, V, out byte R, out byte G, out byte B);
                RGBToHex(R, G, B, out string hex);
                RValue.Text = R.ToString();
                GValue.Text = G.ToString();
                BValue.Text = B.ToString();
                HexValue.Text = hex;
                canChange = true;
            }
        }

        private void HEXChanged(object sender, TextChangedEventArgs e)
        {
            if (canChange)
            {
                canChange = false;
                HexToRGB(HexValue.Text, out byte R, out byte G, out byte B);
                RGBToHSV(R, G, B, out double H, out double S, out double V);
                RValue.Text = R.ToString();
                GValue.Text = G.ToString();
                BValue.Text = B.ToString();
                HValue.Text = H.ToStringRounded(decimalPlaces);
                SValue.Text = S.ToStringRounded(decimalPlaces);
                VValue.Text = V.ToStringRounded(decimalPlaces);
                canChange = true;
            }
        }

        private void ButtonBlack(object sender, RoutedEventArgs e)
        {
            RValue.Text = "0";
            GValue.Text = "0";
            BValue.Text = "0";
        }

        private void ButtonWhite(object sender, RoutedEventArgs e)
        {
            RValue.Text = "255";
            GValue.Text = "255";
            BValue.Text = "255";
        }

        private void ButtonOk(object sender, RoutedEventArgs e)
        {
            if (GetRGB(out byte R, out byte G, out byte B))
            {
                PickedColor = new Color() { R = R, G = G, B = B, A = 255 };
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Entered value is not a number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void UpdateColorView()
        {
            if (GetRGB(out byte R, out byte G, out byte B))
            {
                ColorView.Fill = new SolidColorBrush(new Color() { R = R, G = G, B = B, A = 255 });
            }
        }

        private bool GetRGB(out byte R, out byte G, out byte B)
        {
            R = G = B = 0;
            return byte.TryParse(RValue.Text, out R) &&
                byte.TryParse(GValue.Text, out G) &&
                byte.TryParse(BValue.Text, out B);
        }

        private bool GetHSV(out double H, out double S, out double V)
        {
            H = S = V = 0;
            bool parsed = double.TryParse(HValue.Text, out H) &&
                double.TryParse(SValue.Text, out S) &&
                double.TryParse(VValue.Text, out V);
            return (parsed && H >= 0 && H < 360 && S >= 0 && S <= 100 && V >= 0 && V <= 100);
        }

        private static void RGBToHSV(
            byte R, byte G, byte B,
            out double H, out double S, out double V)
        {
            double r = R / 255.0;
            double g = G / 255.0;
            double b = B / 255.0;

            double cmax = Math.Max(r, Math.Max(g, b));
            double cmin = Math.Min(r, Math.Min(g, b));
            double diff = cmax - cmin;
            H = -1;

            if (cmax == cmin)
                H = 0;

            else if (cmax == r)
                H = (60 * ((g - b) / diff) + 360) % 360;

            else if (cmax == g)
                H = (60 * ((b - r) / diff) + 120) % 360;

            else if (cmax == b)
                H = (60 * ((r - g) / diff) + 240) % 360;

            if (cmax <= 0.9 / 255.0)
                S = 0;
            else
                S = (diff / cmax) * 100;

            V = cmax * 100;
        }

        private static void RGBToHex(byte R, byte G, byte B, out string hex)
        {
            hex = new StringBuilder(7).Append('#').AppendFormat("{0:x2}{1:x2}{2:x2}", R, G, B).ToString();
        }

        private static void HexToRGB(string hex, out byte R, out byte G, out byte B)
        {
            if (hex.Length == 0 || !hex.StartsWith("#") || (hex.Length != 7 && hex.Length != 9))
            {
                R = G = B = 0;
                return;
            }
            R = Convert.ToByte(hex.Substring(1, 2), 16);
            G = Convert.ToByte(hex.Substring(3, 2), 16);
            B = Convert.ToByte(hex.Substring(5, 2), 16);
        }

        private static void HSVToRGB(double H, double S, double V, out byte R, out byte G, out byte B)
        {
            if (H < 0 || H >= 360 || S < 0 || S > 100 || V < 0 || V > 100)
            {
                R = G = B = 0;
                return;
            }

            if (S <= 0.001)
            {
                R = G = B = (byte)(V / 100.0 * 255.0);
                return;
            }

            double s = S / 100.0;
            double v = V / 100.0;
            double h = H / 60.0;
            long i = (long)h;
            double ff = h - i;
            double p = v * (1.0 - s);
            double q = v * (1.0 - (s * ff));
            double t = v * (1.0 - (s * (1.0 - ff)));

            double r, g, b;

            switch (i)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                default:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }
            R = (byte)(r * 255.0);
            G = (byte)(g * 255.0);
            B = (byte)(b * 255.0);
        }
    }
}
