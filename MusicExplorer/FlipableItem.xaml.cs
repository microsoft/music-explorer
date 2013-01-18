/*
 * Copyright © 2012 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MusicExplorer
{
    public partial class FlipableItem : UserControl
    {
        System.Windows.Threading.DispatcherTimer flipTimer = null;

        public FlipableItem()
        {
            InitializeComponent();

            TiltEffect.TiltableItems.Add(typeof(FlipableItem));

            SetupFlipTimer();
        }

        void OnLoaded(Object sender, RoutedEventArgs e)
        {
            ApplyTemplate();
            if (FrontPrimaryText == "MusicExplorerTitlePlaceholder")
            {
                FrontSide.Visibility = Visibility.Collapsed;
                BackSide.Visibility = Visibility.Collapsed;
                Title.Visibility = Visibility.Visible;
                StopFlip();
                return;
            }
            FrontSide.Visibility = Visibility.Visible;
            if (Int32.Parse(FrontSecondaryText) == 1)
            {
                TrackTextBlock.Text = "track";
            }
            else if (Int32.Parse(FrontSecondaryText) > 1)
            {
                TrackTextBlock.Text = "tracks";
            }
        }

        private void SetupFlipTimer()
        {
            if (flipTimer == null)
            {
                flipTimer = new System.Windows.Threading.DispatcherTimer();
                flipTimer.Interval = TimeSpan.FromMilliseconds(5000);
                flipTimer.Tick += (sender, args) => StartFlip();
            }

            if (!flipTimer.IsEnabled)
            {
                flipTimer.Start();
            }
        }

        private void StartFlip()
        {
            double startDelay = Int32.Parse(FrontSecondaryText) * 100;
            while (startDelay > 4000)
            {
                startDelay = startDelay / 2;
            }
            Flip(startDelay);
        }

        private void StopFlip()
        {
            if (flipTimer != null && flipTimer.IsEnabled)
            {
                flipTimer.Stop();
            }
        }

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register("ItemWidth", typeof(string), typeof(FlipableItem), null);
        public string ItemWidth
        {
            get
            {
                return (string)GetValue(ItemWidthProperty);
            }
            set
            {
                SetValue(ItemWidthProperty, value);
            }
        }

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", typeof(string), typeof(FlipableItem), null);
        public string ItemHeight
        {
            get
            {
                return (string)GetValue(ItemHeightProperty);
            }
            set
            {
                SetValue(ItemHeightProperty, value);
            }
        }

        public static readonly DependencyProperty FrontPrimaryTextProperty =
            DependencyProperty.Register("FrontPrimaryText", typeof(string), typeof(FlipableItem), null);
        public string FrontPrimaryText
        {
            get
            {
                return (string)GetValue(FrontPrimaryTextProperty);
            }
            set
            {
                SetValue(FrontPrimaryTextProperty, value);
            }
        }

        public static readonly DependencyProperty FrontSecondaryTextProperty =
            DependencyProperty.Register("FrontSecondaryText", typeof(string), typeof(FlipableItem), null);
        public string FrontSecondaryText
        {
            get
            {
                return (string)GetValue(FrontSecondaryTextProperty);
            }
            set
            {
                SetValue(FrontSecondaryTextProperty, value);
            }
        }

        public static readonly DependencyProperty BackImageProperty =
            DependencyProperty.Register("BackImage", typeof(string), typeof(FlipableItem), null);
        public string BackImage
        {
            get
            {
                return (string)GetValue(BackImageProperty);
            }
            set
            {
                SetValue(BackImageProperty, value);
            }
        }

        public event EventHandler<Visibility> VisibilityChanged;

        public void Flip(double startDelayMs = 0)
        {
            if (BackImage == null || BackImage.Length <= 0) return;
            try
            {
                // Start the flipping animation after the specified duration.
                TimeSpan startTime = TimeSpan.FromMilliseconds(startDelayMs);
                if (_backSideShown)
                {
                    FlipBackToFrontSB.BeginTime = startTime;
                    FlipBackToFrontSB.Begin();
                    FlipBackToFrontSB.Completed += (sender, args) => _backSideShown = false;
                }
                else
                {
                    FlipFrontToBackSB.BeginTime = startTime;
                    FlipFrontToBackSB.Begin();
                    FlipFrontToBackSB.Completed += (sender, args) => _backSideShown = true;
                }
            }
            catch (Exception /*ex*/)
            {
            }
        }

        public void Stop()
        {
            try
            {
                FlipFrontToBackSB.Stop();
                FlipBackToFrontSB.Stop();
                _backSideShown = false;
            }
            catch (Exception /*ex*/)
            {
            }
        }

        private void Image_Opened(object sender, RoutedEventArgs e)
        {
            if (LayoutRoot.Visibility == Visibility.Collapsed)
            {
                LayoutRoot.Visibility = Visibility.Visible;
                EventHandler<Visibility> handler = VisibilityChanged;
                if (handler != null)
                {
                    VisibilityChanged(this, LayoutRoot.Visibility);
                }
            }
        }

        private bool _backSideShown = false;
    }
}
