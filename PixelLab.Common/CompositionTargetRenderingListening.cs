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

#define _FRAME_RATE

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace PixelLab.Common {
  public class CompositionTargetRenderingListener :
#if WPF35
 System.Windows.Threading.DispatcherObject,
#endif
 IDisposable {
    public CompositionTargetRenderingListener() { }

    public void StartListening() {
      requireAccessAndNotDisposed();

      if (!m_isListening) {
        IsListening = true;
        CompositionTarget.Rendering += compositionTarget_Rendering;
#if FRAME_RATE
        m_startTicks = Environment.TickCount;
        m_count = 0;
#endif
      }
    }

    public void StopListening() {
      requireAccessAndNotDisposed();

      if (m_isListening) {
        IsListening = false;
        CompositionTarget.Rendering -= compositionTarget_Rendering;
#if FRAME_RATE
        var ticks = Environment.TickCount - m_startTicks;
        var seconds = ticks / 1000.0;
        Debug.WriteLine("Seconds: {0}, Count: {1}, Frame rate: {2}", seconds, m_count, m_count/seconds);
#endif
      }
    }

#if !SL3
    public void WireParentLoadedUnloaded(FrameworkElement parent) {
      requireAccessAndNotDisposed();
      Util.RequireNotNull(parent, "parent");

      parent.Loaded += delegate(object sender, RoutedEventArgs e) {
        this.StartListening();
      };

      parent.Unloaded += delegate(object sender, RoutedEventArgs e) {
        this.StopListening();
      };
    }
#endif

    public bool IsListening {
      get { return m_isListening; }
      private set {
        if (value != m_isListening) {
          m_isListening = value;
          OnIsListeneningChanged(EventArgs.Empty);
        }
      }
    }

    public bool IsDisposed {
      get {
#if WPF35
        VerifyAccess();
#endif
        return m_disposed;
      }
    }

    public event EventHandler Rendering;

    protected virtual void OnRendering(EventArgs args) {
      requireAccessAndNotDisposed();

      EventHandler handler = Rendering;
      if (handler != null) {
        handler(this, args);
      }
    }

    public event EventHandler IsListeningChanged;

    protected virtual void OnIsListeneningChanged(EventArgs args) {
      var handler = IsListeningChanged;
      if (handler != null) {
        handler(this, args);
      }
    }

    public void Dispose() {
      requireAccessAndNotDisposed();
      StopListening();

      Rendering
        .GetInvocationList()
        .ForEach(d => Rendering -= (EventHandler)d);

      m_disposed = true;
    }

    #region Implementation

    [DebuggerStepThrough]
    private void requireAccessAndNotDisposed() {
#if WPF35
      VerifyAccess();
#endif
      if (m_disposed) {
        throw new ObjectDisposedException(string.Empty);
      }
    }

    private void compositionTarget_Rendering(object sender, EventArgs e) {
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