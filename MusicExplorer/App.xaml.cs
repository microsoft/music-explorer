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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MusicExplorer.Resources;
using MusicExplorer.Models;
using System.Windows.Media;

namespace MusicExplorer
{
    public partial class App : Application
    {
        // Members
        private static MusicApi musicApi = null;
        private static MainViewModel viewModel = null;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
        }

        /// <summary>
        /// A static MusicApi used by the application.
        /// </summary>
        /// <returns>The MusicApi object.</returns>
        public static MusicApi MusicApi
        {
            get
            {
                // Delay creation of the view model until necessary
                if (musicApi == null)
                    musicApi = new MusicApi();

                return musicApi;
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Initialize theming
            InitializeTheming();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Ensure that application state is restored appropriately
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
            else
            {
                e.Handled = true;

                // Show what exception occurred, for debugging purposes
                MessageBox.Show(e.ExceptionObject.Source + ": " 
                                + e.ExceptionObject.Message + "\n"
                                + "\n" + e.ExceptionObject.StackTrace);

                MessageBox.Show("It is strongly suggested to restart the application as it may not work as expected.");
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame();

            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }

        private void InitializeTheming()
        {
            ((SolidColorBrush)Resources["PhoneForegroundBrush"]).Color = (Color)Resources["MusicExplorerForegroundColor"];
            ((SolidColorBrush)Resources["PhoneBackgroundBrush"]).Color = (Color)Resources["MusicExplorerBackgroundColor"];
            ((SolidColorBrush)Resources["PhoneContrastForegroundBrush"]).Color = (Color)Resources["MusicExplorerContrastForegroundColor"];
            ((SolidColorBrush)Resources["PhoneContrastBackgroundBrush"]).Color = (Color)Resources["MusicExplorerContrastBackgroundColor"];
            ((SolidColorBrush)Resources["PhoneTextCaretBrush"]).Color = (Color)Resources["MusicExplorerTextCaretColor"];
            ((SolidColorBrush)Resources["PhoneTextBoxBrush"]).Color = (Color)Resources["MusicExplorerTextBoxColor"];
            ((SolidColorBrush)Resources["PhoneTextBoxForegroundBrush"]).Color = (Color)Resources["MusicExplorerTextBoxForegroundColor"];
            ((SolidColorBrush)Resources["PhoneTextBoxEditBackgroundBrush"]).Color = (Color)Resources["MusicExplorerTextBoxEditBackgroundColor"];
            ((SolidColorBrush)Resources["PhoneTextBoxEditBorderBrush"]).Color = (Color)Resources["MusicExplorerAccentColor"];
            ((SolidColorBrush)Resources["PhoneTextBoxReadOnlyBrush"]).Color = (Color)Resources["MusicExplorerTextBoxReadOnlyColor"];
            ((SolidColorBrush)Resources["PhoneTextBoxSelectionForegroundBrush"]).Color = (Color)Resources["MusicExplorerTextBoxSelectionForegroundColor"];
            ((SolidColorBrush)Resources["PhoneSubtleBrush"]).Color = (Color)Resources["MusicExplorerSubtleColor"];
            ((SolidColorBrush)Resources["PhoneButtonBasePressedForegroundBrush"]).Color = (Color)Resources["MusicExplorerButtonBasePressedForegroundColor"];
            ((SolidColorBrush)Resources["PhoneRadioCheckBoxBrush"]).Color = (Color)Resources["MusicExplorerTransparent"];
            ((SolidColorBrush)Resources["PhoneRadioCheckBoxBorderBrush"]).Color = (Color)Resources["MusicExplorerForegroundColor"];
            ((SolidColorBrush)Resources["PhoneRadioCheckBoxCheckBrush"]).Color = (Color)Resources["MusicExplorerForegroundColor"];
            ((SolidColorBrush)Resources["PhoneRadioCheckBoxPressedBrush"]).Color = (Color)Resources["MusicExplorerAccentColor"];
            ((SolidColorBrush)Resources["PhoneDisabledBrush"]).Color = (Color)Resources["MusicExplorerDisabledColor"];
            ((SolidColorBrush)Resources["PhoneSemitransparentBrush"]).Color = (Color)Resources["MusicExplorerSemitransparentColor"];
            ((SolidColorBrush)Resources["PhoneChromeBrush"]).Color = (Color)Resources["MusicExplorerChromeColor"];
            ((SolidColorBrush)Resources["PhoneAccentBrush"]).Color = (Color)Resources["MusicExplorerAccentColor"];
            ((SolidColorBrush)Resources["PhoneProgressBarBackgroundBrush"]).Color = (Color)Resources["MusicExplorerProgressBarBackgroundColor"];
            ((SolidColorBrush)Resources["PhoneTextHighContrastBrush"]).Color = (Color)Resources["MusicExplorerTextHighContrastColor"];
            ((SolidColorBrush)Resources["PhoneTextMidContrastBrush"]).Color = (Color)Resources["MusicExplorerTextMidContrastColor"];
            ((SolidColorBrush)Resources["PhoneTextLowContrastBrush"]).Color = (Color)Resources["MusicExplorerTextLowContrastColor"];

            RootFrame.Background = new SolidColorBrush((Color)Resources["MusicExplorerBackgroundColor"]);
        }
    }
}