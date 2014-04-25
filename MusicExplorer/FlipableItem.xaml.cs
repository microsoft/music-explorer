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

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MusicExplorer
{
    /// <summary>
    /// This class represents flipable artist item in Favourites view.
    /// The front side contains artist name and track count, and the
    /// backside contains an image of the artist (if available - default
    /// image is shown if no image is found for an artist).
    /// </summary>
    public partial class FlipableItem : UserControl
    {
        // Members
        private DispatcherTimer flipTimer = null;
        private bool backSideShown = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FlipableItem()
        {
            InitializeComponent();
            TiltEffect.TiltableItems.Add(typeof(FlipableItem));
            SetupFlipTimer();
        }

        /// <summary>
        /// Make additional formatting depending on data.
        /// </summary>
        void OnLoaded(Object sender, RoutedEventArgs e)
        {
            ApplyTemplate();

            // Special case - "Favourites" Title is in fact a FlipableItem
            if (FrontPrimaryText == "TitlePlaceholder")
            {
                FrontSide.Visibility = Visibility.Collapsed;
                BackSide.Visibility = Visibility.Collapsed;
                Title.Visibility = Visibility.Visible;
                StopFlip();
                return;
            }

            // Track(s) text depends on the track count
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

        /// <summary>
        /// Starts to flip the item in 5 second interval.
        /// </summary>
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

        /// <summary>
        /// A delay depending on the track count of the artist is used
        /// to introduce some variation to the time when the items flip.
        /// </summary>
        private void StartFlip()
        {
            double startDelay = Int32.Parse(FrontSecondaryText) * 100;
            while (startDelay > 4000)
            {
                startDelay = startDelay / 2;
            }
            Flip(startDelay);
        }

        /// <summary>
        /// Stops flipping the item.
        /// </summary>
        private void StopFlip()
        {
            if (flipTimer != null && flipTimer.IsEnabled)
            {
                flipTimer.Stop();
            }
        }

        /// <summary>
        /// Flips the item using one of two storyboards, front to back or
        /// back to front.
        /// </summary>
        public void Flip(double startDelayMs = 0)
        {
            if (!App.ViewModel.FlipFavourites || BackImage == null || BackImage.Length <= 0) return;
            try
            {
                // Start the flipping animation after the specified duration.
                TimeSpan startTime = TimeSpan.FromMilliseconds(startDelayMs);
                if (backSideShown)
                {
                    FlipBackToFrontSB.BeginTime = startTime;
                    FlipBackToFrontSB.Begin();
                    FlipBackToFrontSB.Completed += (sender, args) => backSideShown = false;
                }
                else
                {
                    FlipFrontToBackSB.BeginTime = startTime;
                    FlipFrontToBackSB.Begin();
                    FlipFrontToBackSB.Completed += (sender, args) => backSideShown = true;
                }
            }
            catch (Exception /*ex*/)
            {
            }
        }

        /// <summary>
        /// Stops currently ongoing flip animation.
        /// </summary>
        public void Stop()
        {
            try
            {
                FlipFrontToBackSB.Stop();
                FlipBackToFrontSB.Stop();
                backSideShown = false;
            }
            catch (Exception /*ex*/)
            {
            }
        }

        /// <summary>
        /// ItemWidth property to enable binding.
        /// </summary>
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

        /// <summary>
        /// ItemHeight property to enable binding.
        /// </summary>
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

        /// <summary>
        /// FrontyPrimaryText property to enable binding. Name of the artist.
        /// </summary>
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

        /// <summary>
        /// FrontSecondaryText property to enable binding. Track count.
        /// </summary>
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

        /// <summary>
        /// BackImage property to enable binding. Artist image.
        /// </summary>
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
    }
}
