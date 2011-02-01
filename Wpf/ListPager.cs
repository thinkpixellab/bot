using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using PixelLab.Common;

namespace PixelLab.Wpf
{
    public class ListPager : DependencyObject
    {
        #region Public Properties

        #region ItemsSource
        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IList), typeof(ListPager),
            GetItemsSourcePropertyMetadata(), new ValidateValueCallback(ValidateItemsSource));

        public event EventHandler ItemsSourceChanged;

        protected virtual void OnItemsSourceChanged(EventArgs e)
        {
            EventHandler handler = ItemsSourceChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private static bool ValidateItemsSource(object value)
        {
            IList source = (IList)value;

            INotifyCollectionChanged incc = source as INotifyCollectionChanged;
            if (incc != null)
            {
                //don't support collection changes...yet
                return false;
            }

            return true;
        }

        private static void ItemsSourcePropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            ((ListPager)element).ItemsSourcePropertyChanged();
        }
        private void ItemsSourcePropertyChanged()
        {
            resetPage(CurrentPageIndex);
            CoerceValue(CurrentPageIndexProperty);
            OnItemsSourceChanged(EventArgs.Empty);
        }

        private static PropertyMetadata GetItemsSourcePropertyMetadata()
        {
            PropertyMetadata pm = new PropertyMetadata();
            pm.DefaultValue = null;
            pm.PropertyChangedCallback = new PropertyChangedCallback(ItemsSourcePropertyChanged);

            return pm;
        }

        #endregion

        #region CurrentPage
        public IList CurrentPage
        {
            get { return (IList)GetValue(CurrentPageProperty); }
        }

        private static readonly DependencyPropertyKey CurrentPagePropertyKey =
            DependencyProperty.RegisterReadOnly("CurrentPage", typeof(IList), typeof(ListPager), GetCurrentPagePropertyMetadata());

        public static readonly DependencyProperty CurrentPageProperty = CurrentPagePropertyKey.DependencyProperty;

        private static PropertyMetadata GetCurrentPagePropertyMetadata()
        {
            PropertyMetadata pm = new PropertyMetadata();
            pm.DefaultValue = new object[0];
            return pm;
        }

        #endregion

        #region CurrentPageIndex

        public int CurrentPageIndex
        {
            get { return (int)GetValue(CurrentPageIndexProperty); }
            set { SetValue(CurrentPageIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageIndexProperty =
            DependencyProperty.Register("CurrentPageIndex", typeof(int), typeof(ListPager), GetCurrentPageIndexPropetyMetadata(),
            new ValidateValueCallback(ValidateCurrentPageIndex));

        protected virtual void OnCurrentPageIndexChanged(EventArgs e)
        {
            EventHandler handler = CurrentPageIndexChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler CurrentPageIndexChanged;

        private static bool ValidateCurrentPageIndex(object value)
        {
            int val = (int)value;
            if (val < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static void CurrentPageIndexPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            ((ListPager)element).CurrentPageIndexPropertyChanged((int)args.NewValue);
        }
        private void CurrentPageIndexPropertyChanged(int newCurrentPageIndex)
        {
            resetPage(newCurrentPageIndex);
            OnCurrentPageIndexChanged(EventArgs.Empty);
        }

        private static object CoerceCurrentPageIndex(DependencyObject element, object value)
        {
            return ((ListPager)element).CoerceCurrentPageIndex((int)value);
        }

        private int CoerceCurrentPageIndex(int value)
        {
            return Math.Min(value, MaxPageIndex);
        }

        private static PropertyMetadata GetCurrentPageIndexPropetyMetadata()
        {
            PropertyMetadata pm = new PropertyMetadata();
            pm.DefaultValue = 0;
            pm.PropertyChangedCallback = new PropertyChangedCallback(CurrentPageIndexPropertyChanged);
            pm.CoerceValueCallback = new CoerceValueCallback(CoerceCurrentPageIndex);

            return pm;
        }

        #endregion

        #region MaxPageIndex

        public int MaxPageIndex
        {
            get
            {
                return (int)GetValue(MaxPageIndexProperty);
            }
        }

        private static readonly DependencyPropertyKey MaxPageIndexPropertyKey = DependencyProperty.RegisterReadOnly("MaxPageIndex",
            typeof(int), typeof(ListPager), new PropertyMetadata(0));

        public static readonly DependencyProperty MaxPageIndexProperty = MaxPageIndexPropertyKey.DependencyProperty;

        #endregion

        #region PageCount

        public int PageCount
        {
            get
            {
                return (int)GetValue(PageCountProperty);
            }
        }

        private static readonly DependencyPropertyKey PageCountPropertyKey = DependencyProperty.RegisterReadOnly(
            "PageCount", typeof(int), typeof(ListPager), new PropertyMetadata(0));

        public static readonly DependencyProperty PageCountProperty = PageCountPropertyKey.DependencyProperty;

        #endregion

        #region PageSize

        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }

        public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(
            "PageSize", typeof(int), typeof(ListPager), GetPageSizePropertyMetadata(), new ValidateValueCallback(ValidatePageSize));

        public event EventHandler PageSizeChanged;

        protected virtual void OnPageSizeChanged(EventArgs e)
        {
            EventHandler handler = PageSizeChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private static bool ValidatePageSize(object value)
        {
            int val = (int)value;
            if (val < 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static void PageSizePropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            ((ListPager)element).PageSizePropertyChanged();
        }
        private void PageSizePropertyChanged()
        {
            resetPage(CurrentPageIndex);
            CoerceValue(CurrentPageIndexProperty);
            OnPageSizeChanged(EventArgs.Empty);
        }

        private static PropertyMetadata GetPageSizePropertyMetadata()
        {
            PropertyMetadata pm = new PropertyMetadata();
            pm.DefaultValue = 10;
            pm.PropertyChangedCallback = new PropertyChangedCallback(PageSizePropertyChanged);
            return pm;
        }

        #endregion

        #endregion

        #region Implementation

        private void resetPage(int currentPageIndex)
        {
            IList items = this.ItemsSource;
            if (items == null)
            {
                SetValue(CurrentPagePropertyKey, CurrentPageProperty.DefaultMetadata.DefaultValue);
                SetValue(PageCountPropertyKey, 1);
                SetValue(MaxPageIndexPropertyKey, 0);
            }
            else
            {
                PagedList pagedList = new PagedList(items, PageSize, currentPageIndex);
                SetValue(CurrentPagePropertyKey, pagedList);
                SetValue(PageCountPropertyKey, 1 + (items.Count - 1) / PageSize);
                SetValue(MaxPageIndexPropertyKey, PageCount - 1);
            }
        }

        #endregion

        private class PagedList : ListBase<object>
        {
            public PagedList(IList source, int pageSize, int pageNumber)
            {
                Debug.Assert(source != null);
                Debug.Assert(pageSize > 0);
                Debug.Assert(pageNumber >= 0);

                _source = source;
                _pageSize = pageSize;
                _pageNumber = pageNumber;
                _initialCount = source.Count;
            }

            protected override object GetItem(int index)
            {
                Debug.Assert(_source.Count == _initialCount);
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return _source[_pageSize * _pageNumber + index];
            }

            public override int Count
            {
                get
                {
                    Debug.Assert(_source.Count == _initialCount);
                    if (_source.Count > MinIndex)
                    {
                        if (_pageSize * (_pageNumber + 1) <= _source.Count)
                        {
                            return _pageSize;
                        }
                        else
                        {
                            return _source.Count - MinIndex;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            protected override object SyncRoot
            {
                get
                {
                    return _source.SyncRoot;
                }
            }

            private int MinIndex
            {
                get
                {
                    return _pageSize * _pageNumber;
                }
            }

            private readonly IList _source;
            private readonly int _pageSize;
            private readonly int _pageNumber;

            private int _initialCount;
        }
    }
}
