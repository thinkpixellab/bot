#define _FRAME_RATE

using System;
using System.Diagnostics;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif
using System.Windows;
using System.Windows.Media;

namespace PixelLab.Common
{
    public class CompositionTargetRenderingListener :
#if !SILVERLIGHT
 System.Windows.Threading.DispatcherObject,
#endif
 IDisposable
    {
        public CompositionTargetRenderingListener() { }

        public void StartListening()
        {
            requireAccessAndNotDisposed();

            if (!m_isListening)
            {
                IsListening = true;
                CompositionTarget.Rendering += compositionTarget_Rendering;
#if FRAME_RATE
        m_startTicks = Environment.TickCount;
        m_count = 0;
#endif
            }
        }

        public void StopListening()
        {
            requireAccessAndNotDisposed();

            if (m_isListening)
            {
                IsListening = false;
                CompositionTarget.Rendering -= compositionTarget_Rendering;
#if FRAME_RATE
        var ticks = Environment.TickCount - m_startTicks;
        var seconds = ticks / 1000.0;
        Debug.WriteLine("Seconds: {0}, Count: {1}, Frame rate: {2}", seconds, m_count, m_count/seconds);
#endif
            }
        }

#if !WP7
        public void WireParentLoadedUnloaded(FrameworkElement parent)
        {
            Contract.Requires(parent != null);
            requireAccessAndNotDisposed();

            parent.Loaded += delegate(object sender, RoutedEventArgs e)
            {
                this.StartListening();
            };

            parent.Unloaded += delegate(object sender, RoutedEventArgs e)
            {
                this.StopListening();
            };
        }
#endif

        public bool IsListening
        {
            get { return m_isListening; }
            private set
            {
                if (value != m_isListening)
                {
                    m_isListening = value;
                    OnIsListeneningChanged(EventArgs.Empty);
                }
            }
        }

        public bool IsDisposed
        {
            get
            {
#if !SILVERLIGHT
                VerifyAccess();
#endif
                return m_disposed;
            }
        }

        public event EventHandler Rendering;

        protected virtual void OnRendering(EventArgs args)
        {
            requireAccessAndNotDisposed();

            EventHandler handler = Rendering;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public event EventHandler IsListeningChanged;

        protected virtual void OnIsListeneningChanged(EventArgs args)
        {
            var handler = IsListeningChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public void Dispose()
        {
            requireAccessAndNotDisposed();
            StopListening();

            Rendering
              .GetInvocationList()
              .ForEach(d => Rendering -= (EventHandler)d);

            m_disposed = true;
        }

        #region Implementation

        [DebuggerStepThrough]
        private void requireAccessAndNotDisposed()
        {
#if !SILVERLIGHT
            VerifyAccess();
#endif
            Util.ThrowUnless<ObjectDisposedException>(!m_disposed, "This object has been disposed");
        }

        private void compositionTarget_Rendering(object sender, EventArgs e)
        {
#if FRAME_RATE
      m_count++;
#endif
            OnRendering(e);
        }

        private bool m_isListening;
        private bool m_disposed;

#if FRAME_RATE
    private int m_count = 0;
    private int m_startTicks;
#endif

        #endregion
    }
}