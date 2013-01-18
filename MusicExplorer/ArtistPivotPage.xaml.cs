/*
 * Copyright © 2012 Nokia Corporation. All rights reserved.
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

using MusicExplorer.ViewModels;

namespace MusicExplorer
{
    public partial class ArtistPivotPage : PhoneApplicationPage
    {
        public ArtistPivotPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
 	        base.OnNavigatedTo(e);
            TitleText.Title = App.ViewModel.SelectedArtist.Name.ToUpper();
        }

        void OnTrackSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductViewModel selected = (ProductViewModel)TracksList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchProduct(selected.Id);
                TracksList.SelectedItem = null;
            }
        }

        void OnAlbumSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductViewModel selected = (ProductViewModel)AlbumsList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchProduct(selected.Id);
                AlbumsList.SelectedItem = null;
            }
        }

        void OnSingleSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductViewModel selected = (ProductViewModel)SinglesList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchProduct(selected.Id);
                SinglesList.SelectedItem = null;
            }
        }

        void OnSimilarArtistsSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ArtistViewModel selected = (ArtistViewModel)SimilarList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchArtist(selected.Id);
                SimilarList.SelectedItem = null;
            }
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            App.MusicApi.PlayLocalArtist(App.ViewModel.SelectedArtist.Name);
        }

        private void OnShowArtistClick(object sender, RoutedEventArgs e)
        {
            App.MusicApi.LaunchArtist(App.ViewModel.SelectedArtist.Id);
        }

        private void OnPlayMixClick(object sender, RoutedEventArgs e)
        {
            App.MusicApi.LaunchArtistMix(App.ViewModel.SelectedArtist.Name);
        }
    }
}