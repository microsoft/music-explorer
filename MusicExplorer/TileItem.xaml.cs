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
    /// This class represents a tile item in artist/product views.
    /// Shows an image with overlaid textual information.
    /// </summary>
    public partial class TileItem : UserControl
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TileItem()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Make additional formatting depending on data.
        /// </summary>
        void OnLoaded(Object sender, RoutedEventArgs e)
        {
            ApplyTemplate();

            // Special case - Title Tile
            String titlePlaceholder = "TitlePlaceholder";
            if (PrimaryText.StartsWith(titlePlaceholder))
            {
                TitleText.Text = PrimaryText.Substring(titlePlaceholder.Length);
                TileLayout.Visibility = Visibility.Collapsed;
                TitleLayout.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                TileLayout.Visibility = Visibility.Visible;
                if (NoteImage)
                {
                    Note.Visibility = Visibility.Visible;
                }
            }

            // Special case - wrap text if primary and secondary texts are same
            if (PrimaryText.Equals(SecondaryText))
            {
                double rightMargin = 10.0;
                if (Note.Visibility == Visibility.Visible)
                {
                    rightMargin += Note.ActualWidth;
                }
                SecondaryTextBlock.Visibility = Visibility.Collapsed;
                PrimaryTextBlock.Margin = new Thickness(4, 0, rightMargin, 0);
                PrimaryTextBlock.TextWrapping = TextWrapping.Wrap;
            }
        }

        /// <summary>
        /// ItemWidth property to enable binding.
        /// </summary>
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register("ItemWidth", typeof(string), typeof(TileItem), null);
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
            DependencyProperty.Register("ItemHeight", typeof(string), typeof(TileItem), null);
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
        /// PrimaryText property to enable binding.
        /// </summary>
        public static readonly DependencyProperty PrimaryTextProperty =
            DependencyProperty.Register("PrimaryText", typeof(string), typeof(TileItem), null);
        public string PrimaryText
        {
            get
            {
                return (string)GetValue(PrimaryTextProperty);
            }
            set
            {
                SetValue(PrimaryTextProperty, value);
            }
        }

        /// <summary>
        /// SecondaryText property to enable binding.
        /// </summary>
        public static readonly DependencyProperty SecondaryTextProperty =
            DependencyProperty.Register("SecondaryText", typeof(string), typeof(TileItem), null);
        public string SecondaryText
        {
            get
            {
                return (string)GetValue(SecondaryTextProperty);
            }
            set
            {
                SetValue(SecondaryTextProperty, value);
            }
        }

        /// <summary>
        /// Image property to enable binding. Artist/Product image.
        /// </summary>
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(string), typeof(TileItem), null);
        public string Image
        {
            get
            {
                return (string)GetValue(ImageProperty);
            }
            set
            {
                SetValue(ImageProperty, value);
            }
        }

        /// <summary>
        /// NoteImage property for showing note image on tile.
        /// </summary>
        public static readonly DependencyProperty NoteImageProperty =
            DependencyProperty.Register("NoteImage", typeof(bool), typeof(TileItem), null);
        public bool NoteImage
        {
            get
            {
                return (bool)GetValue(NoteImageProperty);
            }
            set
            {
                SetValue(NoteImageProperty, value);
            }
        }
    }
}
