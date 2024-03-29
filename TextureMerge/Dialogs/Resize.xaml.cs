﻿using System.Windows;

namespace TextureMerge
{
    public partial class Resize : Window
    {
        public int NewWidth { get; private set; }
        public int NewHeight { get; private set; }

        public Resize(int width, int height)
        {
            InitializeComponent();
            WidthBox.Text = (NewWidth = width).ToString();
            HeightBox.Text = (NewHeight = height).ToString();
        }

        private void OKButton(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WidthBox.Text, out int width) && int.TryParse(HeightBox.Text, out int height) && width > 0 && height > 0)
            {
                NewWidth = width;
                NewHeight = height;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageDialog.Show("Invalid input", type: MessageDialog.Type.Error);
            }
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
