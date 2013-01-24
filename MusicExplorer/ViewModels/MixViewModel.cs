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
    public class MixViewModel : INotifyPropertyChanged
    {
        private string _name;
        /// <summary>
        /// MixViewModel's Name property; this property is used in the view to display its value using a Binding.
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

        private string _parentalAdvisory;
        /// <summary>
        /// MixViewModel's ParentalAdvisory property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
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
        /// MixViewModel's Id property; this property is used in some of Nokia Music API methods.
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

        private Uri _thumbUri;
        /// <summary>
        /// MixViewModel's ThumbUri property; this property is used in the view to display its value using a Binding.
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

