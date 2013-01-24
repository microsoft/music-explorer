/*
 * Copyright © 2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using MusicExplorer.ViewModels;
using System.Windows.Threading;
using System.Threading.Tasks;

using Windows.Devices.Geolocation;
using System.Device.Location;
using Microsoft.Phone.Maps.Services;
using System.Globalization;

namespace MusicExplorer
{
    public partial class MainPage : PhoneApplicationPage
    {
        DispatcherTimer localAudioTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.3) };
        DispatcherTimer geoTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            SystemTray.SetOpacity(this, 0.01);
        }

        private void LocalAudioTimerTick(object sender, EventArgs e)
        {
            localAudioTimer.Stop();
            App.ViewModel.LoadData();
        }

        private void GeoTimerTick(object sender, EventArgs e)
        {
            geoTimer.Stop();
            GetCurrentCoordinate();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                localAudioTimer.Tick += LocalAudioTimerTick;
                localAudioTimer.Start();

                geoTimer.Tick += GeoTimerTick;
                geoTimer.Start();
            }
        }

        void OnFavoriteSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ArtistViewModel selected = (ArtistViewModel)LocalAudioList.SelectedItem;
            if (selected != null)
            {
                // Artist info cannot be fetched from Nokia Music API until the Id
                // of the artist is known (received from API with artist search).
                if (selected.Id == null)
                {
                    MessageBox.Show("Missing necessary data to browse artist info. Please wait for a while and try again.\n" +
                                    "If the problem persists, ensure the device is connected to Internet and restart the application.");
                    LocalAudioList.SelectedItem = null;
                    return;
                }

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
                LocalAudioList.SelectedItem = null;
                NavigationService.Navigate(new Uri("/ArtistPivotPage.xaml", UriKind.Relative));
            }
        }

        void OnRecommendationSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ArtistViewModel selected = (ArtistViewModel)RecommendationsList.SelectedItem;
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
                RecommendationsList.SelectedItem = null;
                NavigationService.Navigate(new Uri("/ArtistPivotPage.xaml", UriKind.Relative));
            }
        }

        void OnNewReleasesSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductViewModel selected = (ProductViewModel)NewReleasesList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchProduct(selected.Id);
                NewReleasesList.SelectedItem = null;
            }
        }

        void OnTopArtistsSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ArtistViewModel selected = (ArtistViewModel)TopArtistsList.SelectedItem;
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
                TopArtistsList.SelectedItem = null;
                NavigationService.Navigate(new Uri("/ArtistPivotPage.xaml", UriKind.Relative));
            }
        }

        void OnGenresSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            GenreViewModel selected = (GenreViewModel)GenresList.SelectedItem;
            if (selected != null)
            {
                App.ViewModel.TopArtistsForGenre.Clear();
                App.MusicApi.GetTopArtistsForGenre(selected.Id);
                App.ViewModel.SelectedGenre = selected.Name.ToLower();
                NavigationService.Navigate(new Uri("/TopArtistsForGenrePage.xaml", UriKind.Relative));
                GenresList.SelectedItem = null;
            }
        }

        void OnMixGroupsSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            MixGroupViewModel selected = (MixGroupViewModel)MixGroupsList.SelectedItem;
            if (selected != null)
            {
                App.ViewModel.Mixes.Clear();
                App.MusicApi.GetMixes(selected.Id);
                NavigationService.Navigate(new Uri("/MixesPage.xaml", UriKind.Relative));
                MixGroupsList.SelectedItem = null;
            }
        }

        /// <summary>
        /// Event handler for clicking about menu item.
        /// </summary>
        private void About_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private async void GetCurrentCoordinate()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.Default;

            try
            {
                Geoposition currentPosition = await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(1),
                                                                                   TimeSpan.FromSeconds(10));

                Dispatcher.BeginInvoke(() =>
                {
                    GeoCoordinate coordinate = new GeoCoordinate(currentPosition.Coordinate.Latitude, currentPosition.Coordinate.Longitude);
                    ReverseGeocodeQuery reverseGeocodeQuery = null;
                    reverseGeocodeQuery = new ReverseGeocodeQuery();
                    reverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
                    reverseGeocodeQuery.QueryCompleted += ReverseGeocodeQuery_QueryCompleted;
                    reverseGeocodeQuery.QueryAsync();
                });
            }
            catch (Exception ex)
            {
                // Couldn't get current location - location might be disabled in settings - use region info from device
                MessageBox.Show("Current location cannot be obtained. Check that location service is turned on in phone settings and restart the application.");
            }
        }

        private void ReverseGeocodeQuery_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 0)
                {
                    MapAddress address = e.Result[0].Information.Address;
                    string twoLetterCountryCode = App.MusicApi.GetTwoLetterCountryCode(address.CountryCode);

                    App.MusicApi.Initialize(twoLetterCountryCode);
                    App.MusicApi.GetArtistInfoForLocalAudio();
                    App.MusicApi.GetNewReleases();
                    App.MusicApi.GetTopArtists();
                    App.MusicApi.GetGenres();
                    App.MusicApi.GetMixGroups();
                }
            }
        }

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Panorama p = (Panorama)sender;
            App.ViewModel.FlipFavourites = p.SelectedIndex == 0 ? true : false;
        }
    }
}