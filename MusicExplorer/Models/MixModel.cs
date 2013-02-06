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

        private Uri _thumbUri;
        /// <summary>
        /// MixModel's ThumbUri property.
        /// This property is used in the UI to display the image in Uri using a Binding.
        /// </summary>
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

