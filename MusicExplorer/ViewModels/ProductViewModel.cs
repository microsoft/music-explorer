/*
 * Copyright © 2012 Nokia Corporation. All rights reserved.
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
    public class ProductViewModel : INotifyPropertyChanged
    {
        private string _performers;
        /// <summary>
        /// ProductViewModel's Performers property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
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
        /// ProductViewModel's Name property; this property is used in the view to display its value using a Binding.
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

        private string _category;
        /// <summary>
        /// ProductViewModel's Category property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
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

        private Uri _thumbUri;
        /// <summary>
        /// ProductViewModel's ThumbUri property; this property is used in the view to display its value using a Binding.
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
        /// ProductViewModel's Id property; this property is used in some of Nokia Music API methods.
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

