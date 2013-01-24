/*
 * Copyright © 2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicExplorer.ViewModels
{
    public class ArtistViewModel : INotifyPropertyChanged
    {
        private string _name;
        /// <summary>
        /// Artist's Name property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
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
        /// Artist's Country property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
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
        /// Artist's Genres property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
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

        private Uri _thumbUri;
        /// <summary>
        /// Artist's ThumbUri property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public Uri ThumbUri
        {
            get
            {
                return _thumbUri;
            }
            set
            {
                if (value != _thumbUri)
                {
                    _thumbUri = value;
                    NotifyPropertyChanged("ThumbUri");
                }
            }
        }

        private string _id;
        /// <summary>
        /// Artist's Id property; this property is used in some Nokia Music API methods.
        /// </summary>
        /// <returns></returns>
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

        private string _trackCount;
        /// <summary>
        /// Artist's TrackCount property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
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

        private string _playCount;
        /// <summary>
        /// Artist's PlayCount property; this property is used when determining the order of artists in favourites view.
        /// </summary>
        /// <returns></returns>
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

        private string _proportionalWidth;
        /// <summary>
        /// Artist's ProportionalWidth property; this property is used when determining width of the artist item in favourites view.
        /// </summary>
        /// <returns></returns>
        public string ProportionalWidth
        {
            get
            {
                return _proportionalWidth;
            }
            set
            {
                if (value != _proportionalWidth)
                {
                    _proportionalWidth = value;
                    NotifyPropertyChanged("ProportionalWidth");
                }
            }
        }

        private string _proportionalHeight;
        /// <summary>
        /// Artist's ProportionalWidth property; this property is used when determining height of the artist item in favourites view.
        /// </summary>
        /// <returns></returns>
        public string ProportionalHeight
        {
            get
            {
                return _proportionalHeight;
            }
            set
            {
                if (value != _proportionalHeight)
                {
                    _proportionalHeight = value;
                    NotifyPropertyChanged("ProportionalHeight");
                }
            }
        }

        private int _similarArtistCount;
        /// <summary>
        /// Artist's SimilarArtistCount property; this property is used when determining the order of artists in Recommended view.
        /// </summary>
        /// <returns></returns>
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

