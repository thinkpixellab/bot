/*
The MIT License

Copyright (c) 2010 Pixel Lab

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PixelLab.Wpf.Demo.Set
{
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

        private void test_click(object sender, RoutedEventArgs e)
        {
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