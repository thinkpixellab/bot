using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PixelLab.Common;

namespace PixelLab.Wpf.Demo {
  public partial class GraphPage : Page {
    static GraphPage() {
      CommandManager.RegisterClassCommandBinding(typeof(GraphPage),
          new CommandBinding(ChangeCenter, (sender, e) => {
            object theThing = e.Parameter;
            ((GraphPage)sender).theGraph.CenterObject = theThing;
          }));
    }

    public GraphPage() {
      InitializeComponent();

      m_dispatchTimer.Interval = TimeSpan.FromSeconds(.1);
      m_dispatchTimer.Tick += churn;

      m_nodes = new NodeCollection<string>(new string[] { 
                "Okoboji", "Kevin", "Brian", "John", "Bob", "Wynette", "Robert",
                "Karen", "Robby", "Boji", "Ralph", "Steve", "Larry", "Joe", "Joseph", "Jeremy",
                "Rebecca", "Becca", "Megan", "Shanna", "Beth",
                "Aaron", "Gabriel", "Dhruv", "Jill", "Sarah"});

      this.AddHandler(Button.ClickEvent, new RoutedEventHandler(click));

      theGraph.CenterObject = m_nodes["Okoboji"];

      this.Unloaded += (sender, e) => m_dispatchTimer.Stop();
    }

    public static readonly RoutedUICommand ChangeCenter = new RoutedUICommand("Change Center", "ChangeCenter", typeof(GraphPage));

    #region Implementation

    private void click(object sender, RoutedEventArgs args) {
      ButtonBase button = (ButtonBase)args.OriginalSource;

      Node<string> node;

      switch (button.Name) {
        case "ChurnNodes":
          if (m_dispatchTimer.IsEnabled) {
            m_dispatchTimer.Stop();
            button.Content = "Start Churning";
          }
          else {
            m_dispatchTimer.Start();
            button.Content = "Stop Churning";
          }
          break;

        case "ChurnSelector":
          if (theGraph.NodeTemplateSelector == null) {
            button.Content = "Remove NodeTemplateSelector";
            theGraph.NodeTemplateSelector = new NodeTemplateSelector();
          }
          else {
            button.Content = "Set NodeTemplateSelector";
            theGraph.NodeTemplateSelector = null;
          }
          break;
        case "Clear":
          if (theGraph.CenterObject == null) {
            theGraph.CenterObject = m_nodes["Okoboji"];
            button.Content = "Clear Center Node";
          }
          else {
            theGraph.CenterObject = null;
            button.Content = "Set Center Node";
          }
          break;
        case "MoreFriends":
          node = (Node<String>)theGraph.CenterObject;
          m_nodes.MoreFriends(node.Item);
          break;
        case "LessFriends":
          node = (Node<String>)theGraph.CenterObject;
          m_nodes.LessFriends(node.Item);
          break;
      } // switch

    } //*** click

    private void churn(object sender, EventArgs e) {
      if (Util.Rnd.Next(10) == 0) {
        Node<string> node = (Node<string>)theGraph.CenterObject;
        if (node != null && node.ChildNodes.Count > 0) {
          theGraph.CenterObject = node.ChildNodes.Random();
        }
      }
      else {
        m_nodes.Churn();
      }
    }

    private readonly DispatcherTimer m_dispatchTimer = new DispatcherTimer(DispatcherPriority.Background);

    private readonly NodeCollection<string> m_nodes;

    #endregion

  }

  internal class NodeTemplateSelector : DataTemplateSelector {
    public override DataTemplate SelectTemplate(object item, DependencyObject container) {
      Node<string> node = (Node<string>)item;
      if (node.Item == "Okoboji") {
        if (m_specialTemplate == null) {
          m_specialTemplate =
              (DataTemplate)((FrameworkElement)container).FindResource("specialTemplate");
        }
        return m_specialTemplate;
      }
      else {
        if (m_nodeTemplate == null) {
          m_nodeTemplate = (DataTemplate)((FrameworkElement)container).FindResource("nodeTemplate");
        }
        return m_nodeTemplate;
      }
    }

    private DataTemplate m_specialTemplate, m_nodeTemplate;
  }

  internal class NodeColorConverter : SimpleValueConverter<Node<string>, Brush> {
    protected override Brush ConvertBase(Node<string> input) {
      long hash = -(long)int.MinValue + input.Item.GetHashCode();
      int index = (int)(hash % App.DemoColors.Count);
      return App.DemoColors[index].ToCachedBrush();
    }
  }

}