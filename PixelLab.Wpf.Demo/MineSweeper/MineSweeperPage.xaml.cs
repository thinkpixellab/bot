using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

namespace PixelLab.Wpf.Demo.MineSweeper {
  public partial class MineSweeperPage : Page {

    public MineSweeperPage() {
      InitializeComponent();

      Initialized += (sender, args) => {
        _playingBrush = (GradientBrush)FindResource("PlayingBackgroundBrush");
        _wonBrush = (GradientBrush)FindResource("WonBackgroundBrush");
        _lostBrush = (GradientBrush)FindResource("LostBackgroundBrush");
      };
    }

    private void _mineFieldElement_PropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == MineField.StatePropertyName) {
        if (m_mineField.State == WinState.Won) {
          this.Background = _wonBrush;
        }
        else if (m_mineField.State == WinState.Lost) {
          this.Background = _lostBrush;
        }
        else if (m_mineField.State == WinState.Unknown) {
          this.Background = _playingBrush;
        }
      }
    }

    void NewGame(object sender, RoutedEventArgs e) {
      m_mineField.NewGame();
    }

    Brush _playingBrush;
    Brush _wonBrush;
    Brush _lostBrush;
  }
}