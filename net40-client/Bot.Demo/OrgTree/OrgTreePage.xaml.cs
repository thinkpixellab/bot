using System.Windows.Controls;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo.OrgTree
{
    [DemoMetadata("Org Tree", "A play off treeview, but stacked like an org chart. Right-click on nodes to add or remove children.")]
    public partial class OrgTreePage : Page
    {
        public OrgTreePage()
        {
            InitializeComponent();
            _tree.ItemsSource = OrgViewItem.GetViewArray(GetDemoData());
        }

        private static OrgItem[] GetDemoData()
        {
            return new OrgItem[]{
        new Company("Acme", new OrgItem[]{
          new Department("Sales", new OrgItem[]{
              new Employee("Bill Jones", null),
              new Employee("Bob Jones", null),
              new Employee("Beth Jones", null)
          }),
          new Department("Service", new OrgItem[]{
              new Employee("Bill Smith", null),
              new Employee("Bob Smith", null),
              new Employee("Beth Smith", null)
          }),
          new Department("Marketing", new OrgItem[]{
              new Employee("Bill Adams", null),
              new Employee("Bob Adams", null),
              new Employee("Beth Adams", null)
          }),
          new Department("Engineering", new OrgItem[]{
              new Employee("Bill Obama", null),
              new Employee("Bob Obama", null),
              new Employee("Beth Obama", null)
          })
          })
      };
        }
    }
}
