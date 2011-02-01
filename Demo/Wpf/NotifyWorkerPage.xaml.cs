using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using PixelLab.Common;
using PixelLab.Demo.Core;

namespace PixelLab.Wpf.Demo
{
    [DemoMetadata("Notify Worker")]
    public partial class NotifyWorkerPage : Page
    {
        public NotifyWorkerPage()
        {
            InitializeComponent();

            this.Loaded += delegate(object sender, RoutedEventArgs e)
            {
                // In general, you could handle this directly in the constructor,
                //  but in the case where a component is re-loaded, it's good to do this

                Debug.Assert(m_notifyWorker == null);
                m_notifyWorker = new NotifyWorker(preWork, work, postWork);
                m_notifyWorker.ClientException += m_notifyWorker_ClientException;
                m_textBlockLastException.DataContext = m_notifyWorker;
            };

            this.Unloaded += delegate(object sender, RoutedEventArgs e)
            {
                // As a usage pattern, dispose NotifyWorker when done with it
                //  this ensures that the 'work' being done finishes and
                //  the worker thread exits cleanly

                m_notifyWorker.Dispose();
                m_notifyWorker.ClientException -= m_notifyWorker_ClientException;
                m_notifyWorker = null;

                m_textBlockLastException.DataContext = null;
            };
        }

        private void sliderSlow_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_textBoxSlowZ.Text = SlowSum(m_sliderSlowX.Value, m_sliderSlowY.Value).ToString("0.00");
        }

        private void sliderFast_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_notifyWorker.NotifyNewWork();
        }

        private bool preWork()
        {
            VerifyAccess(); // This method is called on the UI thread

            if (m_safeX == m_sliderFastX.Value && m_safeY == m_sliderFastY.Value)
            {
                // No need to do work, the values are the same
                return false;
            }
            else
            {
                m_safeX = m_sliderFastX.Value;
                m_safeY = m_sliderFastY.Value;
                return true;
            }
        }

        private void work()
        {
            Debug.Assert(
                !CheckAccess(),
                "This method should be called off the UI thread, "
                + "via the NotifyWorker background thread.");

            if ((Util.Rnd.Next(10) % 9) == 0)
            {
                throw new Exception("Test exception...");
            }

            m_safeZ = SlowSum(m_safeX, m_safeY);
        }

        private void postWork()
        {
            VerifyAccess(); // This method is called on the UI thread

            m_textBoxFastZ.Text = m_safeZ.ToString("0.00");
        }

        private static double SlowSum(double x, double y)
        {
            Thread.Sleep(250);

            return x + y;
        }

        private void m_notifyWorker_ClientException(object sender, NotifyWorkerClientExceptionEventArgs e)
        {
            m_textBlockExceptionCount.Text = (m_clientExceptionCount++).ToString();
        }

        // Any code that uses NotifyWorker should have a notion
        //  of 'safe' state that is only changed in the context of
        //  prework/work/postwork
        private double m_safeX, m_safeY, m_safeZ;

        private int m_clientExceptionCount;
        private NotifyWorker m_notifyWorker;
    }
}