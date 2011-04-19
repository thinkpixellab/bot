using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    [DemoMetadata("Graph", "This is an example of supporting multi-item data binding without using ItemsControl. It uses tricks that are similar to AnimatingTilePanel, specifically physics inspired custom animations using CompsitionTarget.Render.")]
    public partial class GraphPage : Page
    {
        static GraphPage()
        {
            CommandManager.RegisterClassCommandBinding(typeof(GraphPage),
                new CommandBinding(ChangeCenter, (sender, e) =>
                {
                    object theThing = e.Parameter;
                    ((GraphPage)sender).theGraph.CenterObject = theThing;
                }));
        }

        public GraphPage()
        {
            InitializeComponent();

            m_dispatchTimer.Interval = TimeSpan.FromSeconds(.1);
            m_dispatchTimer.Tick += churn;

            m_nodes = new NodeCollection<string>(new string[] {
                "Okoboji", "Kevin", "Brian", "John", "Bob", "Wynette", "Robert",
                "Karen", "Robby", "Boji", "Ralph", "Steve", "Larry", "Joe", "Joseph", "Jeremy",
                "Rebecca", "Becca", "Megan", "Shanna", "Beth",
                "Aaron", "Gabriel", "Dhruv", "Jill", "Sarah"});

            theGraph.CenterObject = m_nodes["Okoboji"];

            this.Unloaded += (sender, e) => m_dispatchTimer.Stop();

            m_churnNodesButton.Click += (sender, args) =>
            {
                if (m_dispatchTimer.IsEnabled)
                {
                    m_dispatchTimer.Stop();
                    m_churnNodesButton.Content = "Start Churning";
                }
                else
                {
                    m_dispatchTimer.Start();
                    m_churnNodesButton.Content = "Stop Churning";
                }
            };

            m_churnSelectorButton.Click += (sender, args) =>
            {
                if (theGraph.NodeTemplateSelector == null)
                {
                    m_churnSelectorButton.Content = "Remove NodeTemplateSelector";
                    theGraph.NodeTemplateSelector = new NodeTemplateSelector();
                }
                else
                {
                    m_churnSelectorButton.Content = "Set NodeTemplateSelector";
                    theGraph.NodeTemplateSelector = null;
                }
            };

            m_clearButton.Click += (sender, args) =>
            {
                if (theGraph.CenterObject == null)
                {
                    theGraph.CenterObject = m_nodes["Okoboji"];
                    m_clearButton.Content = "Clear Center Node";
                }
                else
                {
                    theGraph.CenterObject = null;
                    m_clearButton.Content = "Set Center Node";
                }
            };
        }

        public static readonly RoutedUICommand ChangeCenter = new RoutedUICommand("Change Center", "ChangeCenter", typeof(GraphPage));

        #region Implementation

        private void churn(object sender, EventArgs e)
        {
            if (Util.Rnd.Next(20) == 0)
            {
                if (CenterNode != null && CenterNode.ChildNodes.Count > 0)
                {
                    theGraph.CenterObject = CenterNode.ChildNodes.Random();
                }
            }
            else
            {
                m_nodes.Churn();
            }
        }

        private Node<string> CenterNode
        {
            get
            {
                return (Node<string>)theGraph.CenterObject;
            }
        }

        private void moreButtonClick(object sender, RoutedEventArgs e)
        {
            m_nodes.MoreFriends(CenterNode.Item);
        }

        private void lessButtonClick(object sender, RoutedEventArgs e)
        {
            m_nodes.LessFriends(CenterNode.Item);
        }

        private readonly DispatcherTimer m_dispatchTimer = new DispatcherTimer(DispatcherPriority.Background);

        private readonly NodeCollection<string> m_nodes;

        #endregion

    }

    internal class NodeTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Node<string> node = (Node<string>)item;
            if (node.Item == "Okoboji")
            {
                if (m_specialTemplate == null)
                {
                    m_specialTemplate =
                        (DataTemplate)((FrameworkElement)container).FindResource("specialTemplate");
                }
                return m_specialTemplate;
            }
            else
            {
                if (m_nodeTemplate == null)
                {
                    m_nodeTemplate = (DataTemplate)((FrameworkElement)container).FindResource("nodeTemplate");
                }
                return m_nodeTemplate;
            }
        }

        private DataTemplate m_specialTemplate, m_nodeTemplate;
    }

    internal class NodeColorConverter : SimpleValueConverter<Node<string>, Brush>
    {
        protected override Brush ConvertBase(Node<string> input)
        {
            long hash = -(long)int.MinValue + input.Item.GetHashCode();
            int index = (int)(hash % App.DemoColors.Count);
            return App.DemoColors[index].ToCachedBrush();
        }
    }
}