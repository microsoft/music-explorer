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
    /// Data model to hold all the needed data for an artist.
    /// </summary>
    public class ArtistModel : INotifyPropertyChanged
    {
        private string _name;
        /// <summary>
        /// Artist's Name property. 
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

        private string _country;
        /// <summary>
        /// Artist's Country property. 
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string Country
        {
            get
            {
                return _country;
            }
            set
            {
                if (value != _country)
                {
                    _country = value;
                    NotifyPropertyChanged("Country");
                }
            }
        }

        private string _genres;
        /// <summary>
        /// Artist's Genres property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string Genres
        {
            get
            {
                return _genres;
            }
            set
            {
                if (value != _genres)
                {
                    _genres = value;
                    NotifyPropertyChanged("Genres");
                }
            }
        }

        private Uri _thumb100Uri;
        /// <summary>
        /// Artist's Thumb100Uri (100 px) property.
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
        /// Artist's Thumb200Uri (200 px) property.
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
        /// Artist's Thumb320Uri (320 px) property.
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
        /// Artist's ThumbUri property.
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
        /// Artist's Id property.
        /// This property is used in some Nokia Music API methods.
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

        private string _localTrackCount;
        /// <summary>
        /// Artist's LocalTrackCount property.
        /// This property is used in the UI to display its value using a Binding.
        /// </summary>
        public string LocalTrackCount
        {
            get
            {
                return _localTrackCount;
            }
            set
            {
                if (value != _localTrackCount)
                {
                    _localTrackCount = value;
                    NotifyPropertyChanged("LocalTrackCount");
                }
            }
        }

        private string _playCount;
        /// <summary>
        /// Artist's PlayCount property.
        /// This property is used when determining the order of artists in favourites view.
        /// </summary>
        public string PlayCount
        {
            get
            {
                return _playCount;
            }
            set
            {
                if (value != _playCount)
                {
                    _playCount = value;
                    NotifyPropertyChanged("PlayCount");
                }
            }
        }

        private string _itemWidth;
        /// <summary>
        /// Artist's itemWidth property.
        /// This property is used when determining width of the artist item in favourites view.
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
        /// Artist's ItemWidth property.
        /// This property is used when determining height of the artist item in favourites view.
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

        private int _similarArtistCount;
        /// <summary>
        /// Artist's SimilarArtistCount property.
        /// This property is used when determining the order of artists in Recommended view.
        /// </summary>
        public int SimilarArtistCount
        {
            get
            {
                return _similarArtistCount;
            }
            set
            {
                if (value != _similarArtistCount)
                {
                    _similarArtistCount = value;
                    NotifyPropertyChanged("SimilarArtistCount");
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

