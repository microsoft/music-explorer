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
    public partial class TopArtistsForGenrePage : PhoneApplicationPage
    {
        public TopArtistsForGenrePage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
        }

        void OnArtistSelected(Object sender, SelectionChangedEventArgs e)
        {
            ArtistViewModel selected = (ArtistViewModel)TopArtistsForGenreList.SelectedItem;
//            ArtistViewModel selected = (ArtistViewModel)TopArtistsList.SelectedItem;
            if (selected != null)
            {
                App.ViewModel.AlbumsForArtist.Clear();
                App.ViewModel.SinglesForArtist.Clear();
                App.ViewModel.TracksForArtist.Clear();
                App.ViewModel.SimilarForArtist.Clear();
                App.ViewModel.SelectedArtist = selected;
                App.MusicApi.GetProductsForArtist(selected.Id);
                App.MusicApi.GetSimilarArtists(selected.Id);
                TopArtistsForGenreList.SelectedItem = null;
                NavigationService.Navigate(new Uri("/ArtistPivotPage.xaml", UriKind.Relative));
            }

            /*
            if (selected != null)
            {
                App.ViewModel.AlbumsForArtist.Clear();
                App.ViewModel.SinglesForArtist.Clear();
                App.ViewModel.TracksForArtist.Clear();
                App.ViewModel.SimilarForArtist.Clear();
                App.MusicApi.SearchForLocalArtist(selected.Id);
//                App.ViewModel.SelectedArtist = selected.Name;
//                App.MusicApi.GetProductsForArtist(selected.Id);
//                App.MusicApi.GetSimilarArtists(selected.Id);
                NavigationService.Navigate(new Uri("/ArtistPivotPage.xaml", UriKind.Relative));
                TopArtistsForGenreList.SelectedItem = null;
            }
            */
        }
    }
}