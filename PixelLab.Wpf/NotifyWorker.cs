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

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using PixelLab.Common;

namespace PixelLab.Wpf {
  public class NotifyWorker : DispatcherObject, IDisposable, INotifyPropertyChanged {
    public NotifyWorker(
        Func<bool> preWork,
        Action work,
        Action postWork)
      : base() {
      Util.RequireNotNull(preWork, "preWork");
      Util.RequireNotNull(work, "work");
      Util.RequireNotNull(postWork, "postWork");

      m_preWork = preWork;
      m_work = work;
      m_postWork = postWork;
    }

    public void NotifyNewWork() {
      VerifyAccess();

      using (m_lockHelper.GetLock()) {
        if (m_disposed) {
          throw new ObjectDisposedException("This instance of NotifyWorker has been disposed.");
        }
        else if (m_thread == null) {
          m_thread = new Thread(work);
          m_thread.Priority = ThreadPriority.Lowest;
          m_thread.IsBackground = true;
          m_thread.Start();
          m_workWaiting = true;
        }
        else {
          m_workWaiting = true;
          m_lockHelper.Pulse();
        }
      }
    }

    public void Dispose() {
      VerifyAccess();

      using (m_lockHelper.GetLock()) {
        if (!m_disposed) {
          m_disposed = true;
          m_lockHelper.Pulse();
        }
      }

      if (m_thread != null) {
        // Must do this assignment, because m_operation could be set to null
        //  between the null check and the call to abort.
        DispatcherOperation operation = m_operation;
        if (operation != null) {
          operation.Abort();
        }

        m_thread.Join();
      }
    }

    public NotifyWorkerClientExceptionEventArgs LastClientExceptionEventArgs {
      get {
        VerifyAccess();
        return m_lastClientExceptionEventArgs;
      }
      private set {
        VerifyAccess();
        if (m_lastClientExceptionEventArgs != value) {
          m_lastClientExceptionEventArgs = value;
          OnPropertyChanged(new PropertyChangedEventArgs("LastClientExceptionEventArgs"));
        }
      }
    }

    public event EventHandler<NotifyWorkerClientExceptionEventArgs> ClientException;

    protected virtual void OnClientException(NotifyWorkerClientExceptionEventArgs args) {
      // Raise the event on the Dispatcher thread
      if (Application.Current.Dispatcher.CheckAccess()) {
        if (args != null) {
          EventHandler<NotifyWorkerClientExceptionEventArgs> handler = ClientException;
          if (handler != null) {
            handler(this, args);
          }
        }

        // Might be null. After postWork, if everything has worked, null 
        //  is called to clear things out.
        LastClientExceptionEventArgs = args;
      }
      else {
        Dispatcher.Invoke(
            DispatcherPriority.Background,
            new Action<NotifyWorkerClientExceptionEventArgs>(OnClientException),
            args);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) {
      VerifyAccess();
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null) {
        handler(this, args);
      }
    }

    #region Implementaiton

    private void work() {
      while (!m_disposed) {
        bool getInput = false;

        using (m_lockHelper.GetLock()) {
          if (m_disposed) {
            continue;
          }
          else if (m_workWaiting) {
            getInput = true;
            m_workWaiting = false;
          }
          else {
            m_lockHelper.Wait();
            continue;
          }
        }

        if (getInput && !m_disposed) {

          m_operation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new Func<bool>(() => { return m_preWork(); }));

          if (!m_disposed && m_operation.Wait() == DispatcherOperationStatus.Completed) {
            bool processInput = (bool)m_operation.Result;
            m_operation = null;

            if (processInput) {
              bool workSucceeded = false;
              try {
                m_work();
                workSucceeded = true;
              }
              catch (Exception ex) {
                if (Util.IsCriticalException(ex)) {
                  throw;
                }
                else {
                  OnClientException(new NotifyWorkerClientExceptionEventArgs(ex));
                }
              }

              if (!m_disposed && workSucceeded) {
                // Aside from just calling m_postWork,
                //  we want to call OnClientException with null to clear
                //  out a LastException, if one exists...all on the Dispatcher thread
                m_operation = Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(delegate() {
                  m_postWork();
                  OnClientException(null);
                }));

                if (!m_disposed) {
                  m_operation.Wait();
                }

                m_operation = null;

              } // if (!m_disposed && workSucceeded)

            } // if (processInput && !m_disposed)

          } // if Operation completed

        } // if (getInput)

      } // while (!m_disposed)

    } //*** void work()

    private bool m_disposed;
    private bool m_workWaiting;
    private Thread m_thread;
    private NotifyWorkerClientExceptionEventArgs m_lastClientExceptionEventArgs;
    private DispatcherOperation m_operation;

    private readonly Func<bool> m_preWork;
    private readonly Action m_work;
    private readonly Action m_postWork;
    private readonly LockHelper m_lockHelper = new LockHelper("PollingWorker");

    #endregion

  } //*** class NotifyWorker

  public class NotifyWorkerClientExceptionEventArgs : EventArgs {
    public NotifyWorkerClientExceptionEventArgs(Exception exception) {
      Util.RequireNotNull(exception, "exception");

      Exception = exception;
    }

    public Exception Exception { get; private set; }

    public override string ToString() {
      return string.Format("Exception: {0}", this.Exception);
    }
  }

} //*** PixelLab.Wpf