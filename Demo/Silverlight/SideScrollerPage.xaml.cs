using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Microsoft.Practices.Prism.Commands;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.SL.Demo
{
    [DemoMetadata("Side Scroll")]
    public partial class SideScrollerPage : UserControl
    {
        public SideScrollerPage()
        {
            InitializeComponent();

            rightButton.Command = scrollBehavior.ScrollRightCommand;
            leftButton.Command = scrollBehavior.ScrollLeftCommand;
        }
    }

}
