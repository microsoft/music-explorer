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
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Nokia.Music;
using MusicExplorer.Models;

namespace MusicExplorer
{
    /// Page for displaying mixes in a specific mix group.
    public partial class MixesPage : PhoneApplicationPage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MixesPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            SystemTray.SetOpacity(this, 0.01);
        }

        /// <summary>
        /// Called when a mix is selected. 
        /// Launches the mix through Nokia Music API.
        /// </summary>
        /// <param name="sender">MixesList (LongListSelector).</param>
        /// <param name="e">Event arguments.</param>
        void OnMixSelected(Object sender, SelectionChangedEventArgs e)
        {
            MixModel selectedMix = (MixModel)MixesList.SelectedItem;
            if (selectedMix != null)
            {
                App.MusicApi.LaunchMix(selectedMix.Id);
            }
        }
    }
}