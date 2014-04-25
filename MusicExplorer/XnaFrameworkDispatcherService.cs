/**
 * Copyright (c) 2013-2014 Microsoft Mobile. All rights reserved.
 *
 * Nokia, Nokia Connecting People, Nokia Developer, and HERE are trademarks
 * and/or registered trademarks of Nokia Corporation. Other product and company
 * names mentioned herein may be trademarks or trade names of their respective
 * owners.
 *
 * See the license text file delivered with this project for more information.
 */
 
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using System.Windows;
using System;

namespace MusicExplorer
{
    /// <summary>
    /// Class needed to handle XNA Framework event messages, placed in a queue
    /// that is processed by the XNA FrameworkDispatcher. MediaPlayer, 
    /// MediaLibrary and related classes depend on XNA framework.
    /// Implementation from http://msdn.microsoft.com/library/ff842408.aspx
    /// </summary>
    public class XNAFrameworkDispatcherService : IApplicationService
    {
        // Members
        private DispatcherTimer frameworkDispatcherTimer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public XNAFrameworkDispatcherService()
        {
            this.frameworkDispatcherTimer = new DispatcherTimer();
            this.frameworkDispatcherTimer.Interval = TimeSpan.FromTicks(333333);
            this.frameworkDispatcherTimer.Tick += frameworkDispatcherTimer_Tick;
            FrameworkDispatcher.Update();
        }

        /// <summary>
        /// Calls FrameworkDispatcher.Update()
        /// </summary>
        void frameworkDispatcherTimer_Tick(object sender, EventArgs e)
        {
            FrameworkDispatcher.Update();
        }

        /// <summary>
        /// Starts the dispatcher timer.
        /// </summary>
        void IApplicationService.StartService(ApplicationServiceContext context)
        {
            this.frameworkDispatcherTimer.Start();
        }

        /// <summary>
        /// Stops the dispatcher timer.
        /// </summary>
        void IApplicationService.StopService()
        {
            this.frameworkDispatcherTimer.Stop();
        }
    }
}
