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

using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Shell;
using Nokia.Music;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Windows.Devices.Geolocation;

using MusicExplorer.Models;

namespace MusicExplorer
{
    /// <summary>
    /// Main page for the application. Consists of a panorama with 6 items.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        // Members
        private DispatcherTimer timeoutTimer = 
            new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
        private CountryResolver resolver = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            SystemTray.SetOpacity(this, 0.01);
            LocalAudioList.LayoutUpdated += OnFavoriteLayoutUpdated;
            RecommendationsList.LayoutUpdated += OnRecommendationsLayoutUpdated;
        }

        /// <summary>
        /// When the app is launched and this page is navigated to,
        /// preparations are made for further initialization of the app.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!App.ViewModel.IsDataLoaded)
            {
                timeoutTimer.Tick += LoadLocalData;
                timeoutTimer.Start();
            }
        }

        /// <summary>
        /// Instructs ViewModel to load local artist information.
        /// </summary>
        /// <param name="sender">Timeout timer</param>
        /// <param name="e">Event arguments</param>
        private void LoadLocalData(object sender, EventArgs e)
        {
            timeoutTimer.Stop();
            timeoutTimer.Tick -= LoadLocalData;

            App.ViewModel.LoadData();

            timeoutTimer.Tick += GetCurrentLocation;
            timeoutTimer.Start();
        }

        /// <summary>
        /// Begins the procedure to get current location needed when 
        /// initializing Nokia Music API.
        /// </summary>
        /// <param name="sender">Geo timer</param>
        /// <param name="e">Event arguments</param>
        private void GetCurrentLocation(object sender, EventArgs e)
        {
            timeoutTimer.Stop();
            GetCurrentCoordinate();
        }

        /// <summary>
        /// Artist pivot page is launched for selected favourite artist.
        /// </summary>
        /// <param name="sender">ListBox - list of favourite artists</param>
        /// <param name="e">Event arguments</param>
        void OnFavoriteSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ArtistModel selected = (ArtistModel)LocalAudioList.SelectedItem;
            if (selected != null && LocalAudioList.SelectedIndex > 0) // title is in index 0
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

        /// <summary>
        /// Artist pivot page is launched for selected recommended artist.
        /// </summary>
        /// <param name="sender">LongListSelector - list of recommended artists</param>
        /// <param name="e">Event arguments</param>
        void OnRecommendationSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ArtistModel selected = (ArtistModel)RecommendationsList.SelectedItem;
            if (selected != null && RecommendationsList.SelectedIndex > 0) // title is in index 0
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

        /// <summary>
        /// Launches Nokia Music Application to the selected product view.
        /// </summary>
        /// <param name="sender">LongListSelector - list of new releases</param>
        /// <param name="e">Event arguments</param>
        void OnNewReleasesSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductModel selected = (ProductModel)NewReleasesList.SelectedItem;
            if (selected != null && NewReleasesList.SelectedIndex > 0) // title is in index 0
            {
                App.MusicApi.LaunchProduct(selected.Id);
                NewReleasesList.SelectedItem = null;
            }
        }

        /// <summary>
        /// Artist pivot page is launched for selected artist.
        /// </summary>
        /// <param name="sender">LongListSelector - list of top artists</param>
        /// <param name="e">Event arguments</param>
        void OnTopArtistsSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ArtistModel selected = (ArtistModel)TopArtistsList.SelectedItem;
            if (selected != null && TopArtistsList.SelectedIndex > 0) // title is in index 0
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

        /// <summary>
        /// TopArtistsForGenrePage is launched for selected genre.
        /// </summary>
        /// <param name="sender">LongListSelector - list of genres</param>
        /// <param name="e">Event arguments</param>
        void OnGenresSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            GenreModel selected = (GenreModel)GenresList.SelectedItem;
            if (selected != null)
            {
                if (selected.Name.ToLower() != App.ViewModel.SelectedGenre)
                {
                    App.ViewModel.TopArtistsForGenre.Clear();
                    App.MusicApi.GetTopArtistsForGenre(selected.Id);
                    App.ViewModel.SelectedGenre = selected.Name.ToUpper();
                }
                NavigationService.Navigate(new Uri("/TopArtistsForGenrePage.xaml", UriKind.Relative));
                GenresList.SelectedItem = null;
            }
        }

        /// <summary>
        /// MixesPage is launched for selected mix group.
        /// </summary>
        /// <param name="e">LongListSelector - mix groups</param>
        /// <param name="e">Event arguments</param>
        void OnMixGroupsSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            MixGroupModel selected = (MixGroupModel)MixGroupsList.SelectedItem;
            if (selected != null)
            {
                if (selected.Id != App.ViewModel.SelectedMixGroup)
                {
                    App.ViewModel.Mixes.Clear();
                    App.MusicApi.GetMixes(selected.Id);
                    App.ViewModel.SelectedMixGroup = selected.Name.ToUpper();
                }
                NavigationService.Navigate(new Uri("/MixesPage.xaml", UriKind.Relative));
                MixGroupsList.SelectedItem = null;
            }
        }

        /// <summary>
        /// Navigates to About page.
        /// </summary>
        /// <param name="sender">About menu item</param>
        /// <param name="e">Event arguments</param>
        private void About_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Gets current geocoordinate for use with Nokia Music API.
        /// </summary>
        private async void GetCurrentCoordinate()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.Default;

            try
            {
                Geoposition currentPosition = await geolocator.GetGeopositionAsync(
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromSeconds(10));

                Dispatcher.BeginInvoke(() =>
                {
                    GeoCoordinate coordinate = new GeoCoordinate(
                        currentPosition.Coordinate.Latitude, 
                        currentPosition.Coordinate.Longitude);
                    ReverseGeocodeQuery reverseGeocodeQuery = new ReverseGeocodeQuery();
                    reverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(
                        coordinate.Latitude, 
                        coordinate.Longitude);
                    reverseGeocodeQuery.QueryCompleted += ReverseGeocodeQuery_QueryCompleted;
                    reverseGeocodeQuery.QueryAsync();
                });
            }
            catch (Exception /*ex*/)
            {
                // Couldn't get current location. Location may be disabled.
                // Region info from the device is used as a fallback.
                MessageBox.Show("Current location cannot be obtained. It is " 
                              + "recommended that location service is turned "
                              + "on in phone settings when using Music Explorer."
                              + "\n\nUsing region info from phone settings instead.");

                InitializeNokiaMusicApi(null);
            }
        }

        /// <summary>
        /// Translates current location into a two letter ISO country code used
        /// by Nokia Music API. Makes the inital requests to Nokia Music API.
        /// </summary>
        /// <param name="sender">ReverseGeocodeQuery</param>
        /// <param name="e">Event arguments</param>
        private void ReverseGeocodeQuery_QueryCompleted(
            object sender, 
            QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 0)
                {
                    MapAddress address = e.Result[0].Information.Address;
                    string twoLetterCountryCode = 
                        CountryCodes.TwoLetterFromThreeLetter(address.CountryCode);
                    InitializeNokiaMusicApi(twoLetterCountryCode);
                }
            }
        }

        /// <summary>
        /// Checks the availability of Nokia Music in a locale.
        /// Initializes Nokia Music API if it is available.
        /// </summary>
        /// <param name="twoLetterCountryCode">An ISO 3166-2 country code</param>
        private void InitializeNokiaMusicApi(string twoLetterCountryCode)
        {
            if (resolver == null)
            {
                resolver = new CountryResolver(MusicApi.MUSIC_EXPLORER_APP_ID);
            }

            if (twoLetterCountryCode != null)
            {
                resolver.CheckAvailability((Response<bool> response) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (!response.Result)
                        {
                            MessageBox.Show("Sorry, Nokia Music is not available in this locale.");
                            twoLetterCountryCode = null;
                        }
                    });
                },
                twoLetterCountryCode.ToLower());
            }
            
            // If country code is null, phone region settings are used
            App.MusicApi.Initialize(twoLetterCountryCode);

            // Make initial requests to fill models
            App.MusicApi.GetArtistInfoForLocalAudio();
            App.MusicApi.GetNewReleases();
            App.MusicApi.GetTopArtists();
            App.MusicApi.GetGenres();
            App.MusicApi.GetMixGroups();
        }

        /// <summary>
        /// Starts flipping favourite items when Favourites page is visible,
        /// otherwise stops flipping the favourite items.
        /// </summary>
        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Panorama p = (Panorama)sender;
            App.ViewModel.FlipFavourites = p.SelectedIndex == 0 ? true : false;
        }

        private void OnFavoriteLayoutUpdated(object sender, EventArgs e)
        {
            if (LocalAudioList.Items.Count > 1)
            {
                LocalAudioList.IsHitTestVisible = true;
                LocalAudioList.LayoutUpdated -= OnFavoriteLayoutUpdated;
            }
        }

        private void OnRecommendationsLayoutUpdated(object sender, EventArgs e)
        {
            if (RecommendationsList.Items.Count > 1)
            {
                RecommendationsList.IsHitTestVisible = true;
                RecommendationsList.LayoutUpdated -= OnRecommendationsLayoutUpdated;
            }
        }
    }
}