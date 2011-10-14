using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Microsoft.Practices.Prism.Commands;
using PixelLab.Common;

namespace PixelLab.SL
{
    public class ScrollBehavior : Behavior<ListBox>
    {
        private readonly DelegateCommand _scrollRightCommand, _scrollLeftCommand;

        private ListBox _listBox;
        private ScrollViewer _scrollViewer;
        private ScrollContentPresenter _contentPresenter;

        public ScrollBehavior()
        {
            _scrollRightCommand = new DelegateCommand(scrollRight, canScrollRight);
            _scrollLeftCommand = new DelegateCommand(scrollLeft, canScrollLeft);
        }

        public ICommand ScrollRightCommand
        {
            get
            {
                return _scrollRightCommand;
            }
        }

        public ICommand ScrollLeftCommand
        {
            get
            {
                return _scrollLeftCommand;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            processVisualTree(AssociatedObject);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            processVisualTree(null);
        }

        private void scrollRight()
        {
            Debug.Assert(canScrollRight());
            _contentPresenter.LineRight();
            updateCommands();
        }

        private bool canScrollRight()
        {
            if (_contentPresenter == null)
            {
                return false;
            }

            var max = _contentPresenter.ExtentWidth - _contentPresenter.ViewportWidth;
            return _contentPresenter.CanHorizontallyScroll && _contentPresenter.HorizontalOffset < max;
        }

        private void scrollLeft()
        {
            Debug.Assert(canScrollLeft());
            _contentPresenter.LineLeft();
            updateCommands();
        }

        private bool canScrollLeft()
        {
            return _contentPresenter != null && _contentPresenter.CanHorizontallyScroll && _contentPresenter.HorizontalOffset > 0;
        }

        private void processVisualTree(ListBox listBox)
        {
            if (listBox != _listBox)
            {
                if (_listBox != null)
                {
                    _listBox.LayoutUpdated -= childLayoutUpdated;
                }
                _listBox = listBox;
                if (_listBox != null)
                {
                    _listBox.LayoutUpdated += childLayoutUpdated;
                }
            }

            ScrollViewer foundScrollViewer = null;
            if (_listBox != null)
            {
                foundScrollViewer = _listBox.GetVisualDescendents().OfType<ScrollViewer>().SingleOrDefault();
            }

            if (foundScrollViewer != _scrollViewer)
            {
                if (_scrollViewer != null)
                {
                    _scrollViewer.LayoutUpdated -= childLayoutUpdated;
                }
                _scrollViewer = foundScrollViewer;
                if (_scrollViewer != null)
                {
                    _scrollViewer.LayoutUpdated += childLayoutUpdated;
                }
            }

            ScrollContentPresenter foundCP = null;
            if (_scrollViewer != null)
            {
                foundCP = _scrollViewer.GetVisualDescendents().OfType<ScrollContentPresenter>().Where(cp => cp.Name == "ScrollContentPresenter").FirstOrDefault();
            }
            if (foundCP != _contentPresenter)
            {
                if (_contentPresenter != null)
                {
                    _contentPresenter.LayoutUpdated -= childLayoutUpdated;
                }
                _contentPresenter = foundCP;
                if (_contentPresenter != null)
                {
                    Debug.Assert(_contentPresenter.ScrollOwner == _scrollViewer);
                    _contentPresenter.LayoutUpdated -= childLayoutUpdated;
                }
            }

            updateCommands();
        }

        private void updateCommands()
        {
            _scrollRightCommand.RaiseCanExecuteChanged();
            _scrollLeftCommand.RaiseCanExecuteChanged();
        }

        private void childLayoutUpdated(object sender, EventArgs e)
        {
            processVisualTree(this._listBox);
        }
    }
}