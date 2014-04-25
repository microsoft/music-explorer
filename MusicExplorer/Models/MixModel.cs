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
    /// Data model to hold all the needed data for a mix.
    /// </summary>
    public class MixModel : INotifyPropertyChanged
    {
        private string _name;
        /// <summary>
        /// MixModel's Name property.
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

        private string _parentalAdvisory;
        /// <summary>
        /// MixModel's ParentalAdvisory property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string ParentalAdvisory
        {
            get
            {
                return _parentalAdvisory;
            }
            set
            {
                if (value != _parentalAdvisory)
                {
                    _parentalAdvisory = value;
                    NotifyPropertyChanged("ParentalAdvisory");
                }
            }
        }

        private string _id;
        /// <summary>
        /// MixModel's Id property.
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

        private Uri _thumb100Uri;
        /// <summary>
        /// MixModel's Thumb100Uri (100 px) property.
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
        /// MixModel's Thumb200Uri (200 px) property.
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
        /// MixModel's Thumb320Uri (320 px) property.
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
        /// MixModel's ThumbUri property.
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

        private string _itemWidth;
        /// <summary>
        /// Mix's itemWidth property.
        /// This property is used when determining width of the mix item.
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
        /// Mix's ItemWidth property.
        /// This property is used when determining height of the mix item.
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

