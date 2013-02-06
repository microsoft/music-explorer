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

        private Uri _thumbUri;
        /// <summary>
        /// ProductModel's ThumbUri property.
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

