using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo.MineSweeper
{
    [DemoMetadata("Mine Sweeper", "An oldy, but a goody. Done in WPF. Custom templates are used a lot here.")]
    public partial class MineSweeperPage : Page
    {
        public MineSweeperPage()
        {
            InitializeComponent();

            Initialized += (sender, args) =>
            {
                _playingBrush = (GradientBrush)FindResource("PlayingBackgroundBrush");
                _wonBrush = (GradientBrush)FindResource("WonBackgroundBrush");
                _lostBrush = (GradientBrush)FindResource("LostBackgroundBrush");
            };
        }

        private void _mineFieldElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == MineField.StatePropertyName)
            {
                if (m_mineField.State == WinState.Won)
                {
                    this.Background = _wonBrush;
                }
                else if (m_mineField.State == WinState.Lost)
                {
                    this.Background = _lostBrush;
                }
                else if (m_mineField.State == WinState.Unknown)
                {
                    this.Background = _playingBrush;
                }
            }
        }

        void NewGame(object sender, RoutedEventArgs e)
        {
            m_mineField.NewGame();
        }

        Brush _playingBrush;
        Brush _wonBrush;
        Brush _lostBrush;
    }
}