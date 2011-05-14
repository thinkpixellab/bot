using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PixelLab.Demo.Core;
using PixelLab.Wpf.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    [DemoMetadata("Show Element", "A simple element used to 'show' a series of UIElements.")]
    public partial class ShowElementPage : Page
    {
        public ShowElementPage()
        {
            InitializeComponent();

            IList<BitmapImage> images = SampleImageHelper.GetBitmapImages(10).ToArray();
            int currentIndex = 0;

            var addElementAction = new Action(() =>
            {
                currentIndex %= images.Count;
                m_showElement.AddItem(new Image() { Source = images[currentIndex++] });
            });

            DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(4) };
            timer.Tick += (sender, args) => addElementAction();

            Unloaded += (sender, args) =>
            {
                timer.Stop();
            };

            addElementAction();
            timer.Start();
        }
    }
}