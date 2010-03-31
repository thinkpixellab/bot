using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PixelLab.Wpf.Demo {
  public partial class ShowElementPage : Page {
    public ShowElementPage() {
      InitializeComponent();

      IList<BitmapImage> images = Helpers.GetBitmapImages(10);
      int currentIndex = 0;

      var addElementAction = new Action(() => {
        currentIndex %= images.Count;
        m_showElement.AddItem(new Image() { Source = images[currentIndex++] });
      });


      DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(4) };
      timer.Tick += (sender, args) => addElementAction();
      
      Unloaded += (sender, args) => {
        timer.Stop();
      };

      addElementAction();
      timer.Start();
    }
  }
}