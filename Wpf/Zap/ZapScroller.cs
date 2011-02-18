using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using PixelLab.Common;

namespace PixelLab.Wpf
{
    public class ZapScroller : ItemsControl
    {
        static ZapScroller()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ZapScroller),
                new FrameworkPropertyMetadata(typeof(ZapScroller)));

            FocusableProperty.OverrideMetadata(
                typeof(ZapScroller),
                new FrameworkPropertyMetadata(false));
        }

        public ZapScroller()
        {
            m_firstCommand = new DelegateCommand(First, canFirst);
            m_previousCommand = new DelegateCommand(Previous, canPrevious);
            m_nextCommand = new DelegateCommand(Next, canNext);
            m_lastCommand = new DelegateCommand(Last, canLast);
        }

        public ICommand FirstCommand { get { return m_firstCommand; } }

        public ICommand PreviousCommand { get { return m_previousCommand; } }

        public ICommand NextCommand { get { return m_nextCommand; } }

        public ICommand LastCommand { get { return m_lastCommand; } }

        public ReadOnlyObservableCollection<ZapCommandItem> Commands
        {
            get
            {
                return m_commandItems.ReadOnly;
            }
        }

        public static readonly DependencyProperty CommandItemTemplateProperty =
            DependencyProperty.Register("CommandItemTemplate", typeof(DataTemplate), typeof(ZapScroller));

        public DataTemplate CommandItemTemplate
        {
            get { return (DataTemplate)GetValue(CommandItemTemplateProperty); }
            set { SetValue(CommandItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty CommandItemTemplateSelectorProperty =
            DependencyProperty.Register("CommandItemTemplateSelector", typeof(DataTemplateSelector), typeof(ZapScroller));

        public DataTemplateSelector CommandItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(CommandItemTemplateSelectorProperty); }
            set { SetValue(CommandItemTemplateSelectorProperty, value); }
        }

        private static readonly DependencyPropertyKey ItemCountPropertyKey =
            DependencyProperty.RegisterReadOnly("ItemCount",
            typeof(int), typeof(ZapScroller), new PropertyMetadata(0));

        public static readonly DependencyProperty ItemCountProperty = ItemCountPropertyKey.DependencyProperty;

        public int ItemCount
        {
            get { return (int)GetValue(ItemCountProperty); }
        }

        public static readonly DependencyProperty CurrentItemIndexProperty =
            DependencyProperty.Register("CurrentItemIndex", typeof(int), typeof(ZapScroller),
            new PropertyMetadata(new PropertyChangedCallback(currentItemIndex_changed)));

        public int CurrentItemIndex
        {
            get { return (int)GetValue(CurrentItemIndexProperty); }
            set { SetValue(CurrentItemIndexProperty, value); }
        }

        public void First()
        {
            if (canFirst())
            {
                CurrentItemIndex = 0;
            }
        }

        public void Previous()
        {
            if (canPrevious())
            {
                CurrentItemIndex--;
            }
        }

        public void Next()
        {
            if (canNext())
            {
                CurrentItemIndex++;
            }
        }

        public void Last()
        {
            if (canLast())
            {
                CurrentItemIndex = (ItemCount - 1);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            findZapDecorator();

            return base.MeasureOverride(availableSize);
        }

        public static readonly RoutedEvent CurrentItemIndexChangedEvent =
             EventManager.RegisterRoutedEvent("CurrentItemIndexChanged", RoutingStrategy.Bubble,
             typeof(RoutedPropertyChangedEventHandler<int>), typeof(ZapScroller));

        public event RoutedPropertyChangedEventHandler<int> CurrentItemChanged
        {
            add { base.AddHandler(CurrentItemIndexChangedEvent, value); }
            remove { base.RemoveHandler(CurrentItemIndexChangedEvent, value); }
        }

        protected virtual void OnCurrentItemIndexChanged(int oldValue, int newValue)
        {
            resetEdgeCommands();
            RoutedPropertyChangedEventArgs<int> args = new RoutedPropertyChangedEventArgs<int>(oldValue, newValue);
            args.RoutedEvent = CurrentItemIndexChangedEvent;
            base.RaiseEvent(args);
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            ItemCollection newItems = this.Items;

            if (newItems != m_internalItemCollection)
            {
                m_internalItemCollection = newItems;

                resetProperties();
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (m_internalItemCollection != Items)
            {
                m_internalItemCollection = Items;
            }

            resetProperties();
        }

        #region Implementation

        private static void currentItemIndex_changed(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            ZapScroller zapScroller = (ZapScroller)element;
            zapScroller.OnCurrentItemIndexChanged((int)e.OldValue, (int)e.NewValue);
        }

        private void resetEdgeCommands()
        {
            m_firstCommand.RaiseCanExecuteChanged();
            m_lastCommand.RaiseCanExecuteChanged();
            m_nextCommand.RaiseCanExecuteChanged();
            m_previousCommand.RaiseCanExecuteChanged();
        }

        private void resetCommands()
        {
            resetEdgeCommands();

            int parentItemsCount = this.ItemCount;

            if (parentItemsCount != m_commandItems.Count)
            {
                if (parentItemsCount > m_commandItems.Count)
                {
                    for (int i = m_commandItems.Count; i < parentItemsCount; i++)
                    {
                        m_commandItems.Add(new ZapCommandItem(this, i));
                    }
                }
                else
                {
                    Debug.Assert(parentItemsCount < m_commandItems.Count);
                    int delta = m_commandItems.Count - parentItemsCount;
                    for (int i = 0; i < delta; i++)
                    {
                        m_commandItems.RemoveAt(m_commandItems.Count - 1);
                    }
                }
            }

            Debug.Assert(Items.Count == m_commandItems.Count);

            for (int i = 0; i < parentItemsCount; i++)
            {
                m_commandItems[i].Content = Items[i];
            }

#if DEBUG
            for (int i = 0; i < m_commandItems.Count; i++)
            {
                Debug.Assert(((ZapCommandItem)m_commandItems[i]).Index == i);
            }
#endif
        }

        private void findZapDecorator()
        {
            if (this.Template != null)
            {
                ZapDecorator temp = this.Template.FindName(PART_ZapDecorator, this) as ZapDecorator;
                if (m_zapDecorator != temp)
                {
                    m_zapDecorator = temp;
                    if (m_zapDecorator != null)
                    {
                        Binding binding = new Binding("CurrentItemIndex");
                        binding.Source = this;
                        m_zapDecorator.SetBinding(ZapDecorator.TargetIndexProperty, binding);
                    }
                }
                else
                {
                    Debug.WriteLine("No element with name '" + PART_ZapDecorator + "' in the template.");
                }
            }
            else
            {
                Debug.WriteLine("No template defined for ZapScroller.");
            }
        }

        private void resetProperties()
        {
            if (m_internalItemCollection.Count != ItemCount)
            {
                SetValue(ItemCountPropertyKey, m_internalItemCollection.Count);
            }
            if (CurrentItemIndex >= ItemCount)
            {
                CurrentItemIndex = (ItemCount - 1);
            }
            else if (CurrentItemIndex == -1 && ItemCount > 0)
            {
                CurrentItemIndex = 0;
            }

            resetCommands();
        }

        private bool canFirst()
        {
            return (ItemCount > 1) && (CurrentItemIndex > 0);
        }

        private bool canNext()
        {
            return (CurrentItemIndex >= 0) && CurrentItemIndex < (ItemCount - 1);
        }

        private bool canPrevious()
        {
            return CurrentItemIndex > 0;
        }

        private bool canLast()
        {
            return (ItemCount > 1) && (CurrentItemIndex < (ItemCount - 1));
        }

        private ItemCollection m_internalItemCollection;

        private ZapDecorator m_zapDecorator;

        private readonly DelegateCommand m_firstCommand, m_previousCommand, m_nextCommand, m_lastCommand;

        private readonly ObservableCollectionPlus<ZapCommandItem> m_commandItems = new ObservableCollectionPlus<ZapCommandItem>();

        #endregion

        public const string PART_ZapDecorator = "PART_ZapDecorator";
    }
}