using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Xna.Framework;

namespace VoiceRecorder
{
    public class XNAFrameworkDispatcherService : IApplicationService
    {
        private DispatcherTimer _FrameworkDispatcherTimer;

        public XNAFrameworkDispatcherService()
        {
            _FrameworkDispatcherTimer = new DispatcherTimer();
            _FrameworkDispatcherTimer.Interval = TimeSpan.FromTicks(333333);
            _FrameworkDispatcherTimer.Tick += FrameworkDispatcherTimer_Tick;
            FrameworkDispatcher.Update();
        }

        void FrameworkDispatcherTimer_Tick(object sender, EventArgs e)
        {
            FrameworkDispatcher.Update();
        }

        void IApplicationService.StartService(ApplicationServiceContext context)
        {
            _FrameworkDispatcherTimer.Start();
        }

        void IApplicationService.StopService()
        {
            _FrameworkDispatcherTimer.Stop();
        }
    }
}
