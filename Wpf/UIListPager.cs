using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace PixelLab.Wpf
{
    public class UIListPager : ListPager
    {
        public UIListPager()
        {
            _firstCommand = new UIListPagerCommand(this, UIListPagerCommandType.FirstPage, -1);
            _lastCommand = new UIListPagerCommand(this, UIListPagerCommandType.LastPage, -1);
            _nextCommand = new UIListPagerCommand(this, UIListPagerCommandType.NextPage, -1);
            _previousCommand = new UIListPagerCommand(this, UIListPagerCommandType.PreviousPage, -1);

            _commands = new UIListPagerCommand[0];
        }

        public UIListPagerCommand FirstCommand { get { return _firstCommand; } }
        public UIListPagerCommand LastCommand { get { return _lastCommand; } }
        public UIListPagerCommand NextCommand { get { return _nextCommand; } }
        public UIListPagerCommand PreviousCommand { get { return _previousCommand; } }

        #region PageCommands
        public IList<UIListPagerCommand> PageCommands
        {
            get
            {
                return (IList<UIListPagerCommand>)GetValue(PageCommandsProperty);
            }
        }
        private static readonly DependencyPropertyKey PageCommandsPropertyKey = DependencyProperty.RegisterReadOnly(
            "PageCommands", typeof(IList<UIListPagerCommand>), typeof(UIListPager), new PropertyMetadata(new UIListPagerCommand[0]));

        public static readonly DependencyProperty PageCommandsProperty = PageCommandsPropertyKey.DependencyProperty;

        private void resetPageCommands()
        {
            Debug.Assert(_commands.Length == PageCommands.Count);
            Debug.Assert(CheckAccess());

            if (_commands.Length != this.PageCount)
            {
                UIListPagerCommand[] newCommands = new UIListPagerCommand[this.PageCount];
                for (int i = 0; i < newCommands.Length; i++)
                {
                    if (i < _commands.Length)
                    {
                        newCommands[i] = _commands[i];
                    }
                    else
                    {
                        newCommands[i] = new UIListPagerCommand(this, UIListPagerCommandType.SpecificPage, i);
                    }
                }

                _commands = newCommands;
                SetValue(PageCommandsPropertyKey, new ReadOnlyCollection<UIListPagerCommand>(_commands));
            }
        }
        #endregion

        #region overrides
        protected override void OnCurrentPageIndexChanged(EventArgs e)
        {
            base.OnCurrentPageIndexChanged(e);
            resetPageCommands();
        }
        protected override void OnItemsSourceChanged(EventArgs e)
        {
            base.OnItemsSourceChanged(e);
            resetPageCommands();
        }
        protected override void OnPageSizeChanged(EventArgs e)
        {
            base.OnPageSizeChanged(e);
            resetPageCommands();
        }
        #endregion

        #region implementation

        private UIListPagerCommand[] _commands;

        private readonly UIListPagerCommand _firstCommand, _lastCommand, _nextCommand, _previousCommand;
        #endregion
    }

    [TypeConverter(typeof(UIListPagerCommandConverter))]
    public class UIListPagerCommand : ICommand
    {
        internal UIListPagerCommand(UIListPager ulp, UIListPagerCommandType type, int pageNumber)
        {
            Debug.Assert(ulp != null);
            Debug.Assert(type == UIListPagerCommandType.SpecificPage || pageNumber == -1);

            _pager = ulp;
            _pager.CurrentPageIndexChanged += _pager_PropertyChanged;
            _pager.ItemsSourceChanged += _pager_PropertyChanged;
            _pager.PageSizeChanged += _pager_PropertyChanged;

            _type = type;
            _pageIndex = pageNumber;

            _canExecuteCache = canExecute();
        }

        private void _pager_PropertyChanged(object sender, EventArgs e)
        {
            bool newCanExecute = canExecute();
            if (newCanExecute != _canExecuteCache)
            {
                _canExecuteCache = newCanExecute;
                OnCanExecuteChanged(EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteCache;
        }

        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            Debug.Assert(canExecute());

            switch (_type)
            {
                case UIListPagerCommandType.FirstPage:
                    _pager.CurrentPageIndex = 0;
                    break;
                case UIListPagerCommandType.LastPage:
                    _pager.CurrentPageIndex = _pager.MaxPageIndex;
                    break;
                case UIListPagerCommandType.PreviousPage:
                    Debug.Assert(_pager.CurrentPageIndex > 0);
                    if (_pager.CurrentPageIndex > 0)
                    {
                        _pager.CurrentPageIndex--;
                    }
                    break;
                case UIListPagerCommandType.NextPage:
                    _pager.CurrentPageIndex++;
                    break;
                case UIListPagerCommandType.SpecificPage:
                    _pager.CurrentPageIndex = _pageIndex;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Zero-based page index. Better for people who think like developers.
        /// </summary>
        public int PageIndex
        {
            get
            {
                Debug.Assert(_pageIndex >= -1);
                return _pageIndex;
            }
        }

        /// <summary>
        /// One-based index into the page index to bind UI to. Better for people who think like mortals.
        /// </summary>
        public int PageNumber
        {
            get
            {
                Debug.Assert(_pageIndex >= -1);
                if (_pageIndex == -1)
                {
                    return -1;
                }
                else
                {
                    return _pageIndex + 1;
                }
            }
        }

        public override string ToString()
        {
            switch (_type)
            {
                case UIListPagerCommandType.FirstPage:
                    return "UIListPagerComand: FirstPage";
                case UIListPagerCommandType.LastPage:
                    return "UIListPagerComand: LastPage";
                case UIListPagerCommandType.NextPage:
                    return "UIListPagerComand: NextPage";
                case UIListPagerCommandType.PreviousPage:
                    return "UIListPagerComand: PreviousPAge";
                case UIListPagerCommandType.SpecificPage:
                    return "UIListPagerComand: SpecificPage(" + _pageIndex + ")";
                default:
                    throw new InvalidOperationException("should never get here!");
            }
        }

        private bool canExecute()
        {
            switch (_type)
            {
                case UIListPagerCommandType.FirstPage:
                    return _pager.CurrentPageIndex != 0;
                case UIListPagerCommandType.LastPage:
                    return _pager.CurrentPageIndex != _pager.MaxPageIndex;
                case UIListPagerCommandType.PreviousPage:
                    return _pager.CurrentPageIndex > 0;
                case UIListPagerCommandType.NextPage:
                    return _pager.CurrentPageIndex < _pager.MaxPageIndex;
                case UIListPagerCommandType.SpecificPage:
                    Debug.Assert(_pageIndex >= 0);
                    return _pager.CurrentPageIndex != _pageIndex && _pageIndex <= _pager.MaxPageIndex;
                default:
                    throw new InvalidOperationException();
            }
        }

        private bool _canExecuteCache;

        private UIListPager _pager;
        private UIListPagerCommandType _type;
        private int _pageIndex;
    }

    public class UIListPagerCommandConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(ICommand))
            {
                return true;
            }
            else if (destinationType == typeof(string))
            {
                return true;
            }
            else
            {
                return base.CanConvertTo(context, destinationType);
            }
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            if (destinationType == typeof(ICommand))
            {
                return (ICommand)value;
            }
            else if (destinationType == typeof(string))
            {
                return ((UIListPagerCommand)value).ToString();
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }

    public enum UIListPagerCommandType
    {
        FirstPage, LastPage,
        PreviousPage, NextPage,
        SpecificPage
    }
}
