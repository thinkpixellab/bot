using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo.Set
{
    [DemoMetadata("Set Game", "The game Set in WPF.")]
    public partial class SetPage : Page
    {
        public SetPage()
        {
            InitializeComponent();

            this.DataContext = m_setGame;

            m_setBoardElement.ProvideGame(m_setGame);

            this.Unloaded += delegate(object sender, RoutedEventArgs e)
            {
                m_setBoardElement.Dispose();
            };

#if DEBUG

      Button testButton = new Button();
      testButton.Content = "_Test";
      testButton.Click += test_click;
      testButton.SetBinding(Button.IsEnabledProperty, "CanPlay");

      testButton.Padding = new Thickness(5);
      testButton.Margin = new Thickness(10, 0, 0, 0);

      m_stackPanel.Children.Add(testButton);

#endif
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                m_setBoardElement.ResetCards(false);

                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        private void newGame_click(object sender, RoutedEventArgs e)
        {
            m_setGame.NewGame();
            m_setBoardElement.ResetCards(true);
        }

#if DEBUG

    private void test_click(object sender, RoutedEventArgs e) {
      m_setGame.Test();
      m_setBoardElement.ResetCards(false);
    }

#endif

        private readonly SetGame m_setGame = new SetGame();
    }

    public class ButtonEnabledConverter : IMultiValueConverter
    {
        public static bool Convert(bool canPlay, SetCard card)
        {
            return canPlay && (card != null);
        }

        public static readonly ButtonEnabledConverter Instance = new ButtonEnabledConverter();

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((bool)values[0], (SetCard)values[1]);
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}