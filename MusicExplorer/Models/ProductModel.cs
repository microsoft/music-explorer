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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicExplorer.Models
{
    /// <summary>
    /// Data model to hold all the needed data for a product.
    /// </summary>
    public class ProductModel : INotifyPropertyChanged
    {
        private string _performers;
        /// <summary>
        /// ProductModel's Performers property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string Performers
        {
            get
            {
                return _performers;
            }
            set
            {
                if (value != _performers)
                {
                    _performers = value;
                    NotifyPropertyChanged("Performers");
                }
            }
        }

        private string _name;
        /// <summary>
        /// ProductModel's Name property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _takenFrom;
        /// <summary>
        /// ProductModel's TakenFrom property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string TakenFrom
        {
            get
            {
                return _takenFrom;
            }
            set
            {
                if (value != _takenFrom)
                {
                    _takenFrom = value;
                    NotifyPropertyChanged("TakenFrom");
                }
            }
        }

        private string _trackCount;
        /// <summary>
        /// ProductModel's TrackCount property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string TrackCount
        {
            get
            {
                return _trackCount;
            }
            set
            {
                if (value != _trackCount)
                {
                    _trackCount = value;
                    NotifyPropertyChanged("TrackCount");
                }
            }
        }

        private string _category;
        /// <summary>
        /// ProductModel's Category property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                if (value != _category)
                {
                    _category = value;
                    NotifyPropertyChanged("Category");
                }
            }
        }

        private Uri _thumb100Uri;
        /// <summary>
        /// ProductModel's Thumb100Uri (100 px) property.
        /// This property is used in the UI to display the image in Uri using a Binding.
        /// </summary>
        public Uri Thumb100Uri
        {
            get
            {
                return _thumb100Uri;
            }
            set
            {
                if (value != _thumb100Uri)
                {
                    _thumb100Uri = value;
                    NotifyPropertyChanged("Thumb100Uri");
                    NotifyPropertyChanged("ThumbUri");
                }
            }
        }

        private Uri _thumb200Uri;
        /// <summary>
        /// ProductModel's Thumb200Uri (200 px) property.
        /// This property is used in the UI to display the image in Uri using a Binding.
        /// </summary>
        public Uri Thumb200Uri
        {
            get
            {
                return _thumb200Uri;
            }
            set
            {
                if (value != _thumb200Uri)
                {
                    _thumb200Uri = value;
                    NotifyPropertyChanged("Thumb200Uri");
                    NotifyPropertyChanged("ThumbUri");
                }
            }
        }

        private Uri _thumb320Uri;
        /// <summary>
        /// ProductModel's Thumb320Uri (320 px) property.
        /// This property is used in the UI to display the image in Uri using a Binding.
        /// </summary>
        public Uri Thumb320Uri
        {
            get
            {
                return _thumb320Uri;
            }
            set
            {
                if (value != _thumb320Uri)
                {
                    _thumb320Uri = value;
                    NotifyPropertyChanged("Thumb320Uri");
                    NotifyPropertyChanged("ThumbUri");
                }
            }
        }

        /// <summary>
        /// ProductModel's ThumbUri property.
        /// This property is used in the UI to display the image in Uri using a Binding.
        /// </summary>
        public Uri ThumbUri
        {
            get
            {
                if (_thumb320Uri != null && ResolutionHelper.CurrentResolution == Resolutions.HD1080p)
                {
                    return _thumb320Uri;
                }
                else if (_thumb200Uri != null)
                {
                    return _thumb200Uri;
                }
                else if (_thumb100Uri != null)
                {
                    return _thumb100Uri;
                }
                else
                {
                    return new Uri("/Assets/thumb_100_placeholder.png",
                                   UriKind.Relative);
                }
            }
        }

        private string _id;
        /// <summary>
        /// ProductModel's Id property.
        /// This property is used in some of Nokia Music API methods.
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        private string _itemWidth;
        /// <summary>
        /// Product's itemWidth property.
        /// This property is used when determining width of the product item.
        /// </summary>
        public string ItemWidth
        {
            get
            {
                return _itemWidth;
            }
            set
            {
                if (value != _itemWidth)
                {
                    _itemWidth = value;
                    NotifyPropertyChanged("ItemWidth");
                }
            }
        }

        private string _itemHeight;
        /// <summary>
        /// Product's ItemWidth property.
        /// This property is used when determining height of the product item.
        /// </summary>
        public string ItemHeight
        {
            get
            {
                return _itemHeight;
            }
            set
            {
                if (value != _itemHeight)
                {
                    _itemHeight = value;
                    NotifyPropertyChanged("ItemHeight");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

