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

using MusicExplorer.Models;

namespace MusicExplorer
{
    /// Page for displaying top artists in a specific genre.
    /// </summary>
    public partial class TopArtistsForGenrePage : PhoneApplicationPage
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TopArtistsForGenrePage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            SystemTray.SetOpacity(this, 0.01);
        }

        /// <summary>
        /// Called when an artist is selected. Opens up ArtistPivotPage.
        /// </summary>
        /// <param name="sender">TopArtistsForGenreList (LongListSelector).</param>
        /// <param name="e">Event arguments.</param>
        void OnArtistSelected(Object sender, SelectionChangedEventArgs e)
        {
            ArtistModel selected = (ArtistModel)TopArtistsForGenreList.SelectedItem;
            if (selected != null)
            {
                if (selected != App.ViewModel.SelectedArtist)
                {
                    App.ViewModel.SelectedArtist = selected;
                    App.ViewModel.AlbumsForArtist.Clear();
                    App.ViewModel.SinglesForArtist.Clear();
                    App.ViewModel.TracksForArtist.Clear();
                    App.ViewModel.SimilarForArtist.Clear();
                    App.MusicApi.GetProductsForArtist(selected.Id);
                    App.MusicApi.GetSimilarArtists(selected.Id);
                }
                TopArtistsForGenreList.SelectedItem = null;
                NavigationService.Navigate(new Uri("/ArtistPivotPage.xaml", UriKind.Relative));
            }
        }
    }
}