/*
 * Copyright © 2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
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

using Nokia.Music.Phone;
using MusicExplorer.ViewModels;

namespace MusicExplorer
{
    public partial class MixesPage : PhoneApplicationPage
    {
        public MixesPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            SystemTray.SetOpacity(this, 0.01);
        }

        void OnMixSelected(Object sender, SelectionChangedEventArgs e)
        {
            MixViewModel selectedMix = (MixViewModel)MixesList.SelectedItem;
            if (selectedMix != null)
            {
                App.MusicApi.LaunchMix(selectedMix.Id);
            }
        }
    }
}