/*
 * Copyright © 2012 Nokia Corporation. All rights reserved.
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
                App.ViewModel.AlbumsForArtist.Clear();
                App.ViewModel.SinglesForArtist.Clear();
                App.ViewModel.TracksForArtist.Clear();
                App.ViewModel.SimilarForArtist.Clear();
                App.ViewModel.SelectedArtist = selected;
                App.MusicApi.GetProductsForArtist(selected.Id);
                App.MusicApi.GetSimilarArtists(selected.Id);
                LocalAudioList.SelectedItem = null;
                NavigationService.Navigate(new Uri("/ArtistPivotPage.xaml", UriKind.Relative));
            }
        }

        void OnRecommendationSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ArtistViewModel selected = (ArtistViewModel)RecommendationsList.SelectedItem;
            if (selected != null)
            {
                App.ViewModel.AlbumsForArtist.Clear();
                App.ViewModel.SinglesForArtist.Clear();
                App.ViewModel.TracksForArtist.Clear();
                App.ViewModel.SimilarForArtist.Clear();
                App.ViewModel.SelectedArtist = selected;
                App.MusicApi.GetProductsForArtist(selected.Id);
                App.MusicApi.GetSimilarArtists(selected.Id);
                TopArtistsList.SelectedItem = null;
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
                App.ViewModel.AlbumsForArtist.Clear();
                App.ViewModel.SinglesForArtist.Clear();
                App.ViewModel.TracksForArtist.Clear();
                App.ViewModel.SimilarForArtist.Clear();
                App.ViewModel.SelectedArtist = selected;
                App.MusicApi.GetProductsForArtist(selected.Id);
                App.MusicApi.GetSimilarArtists(selected.Id);
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
                // Couldn't get current location - location might be disabled in settings
                MessageBox.Show("Current location cannot be obtained. Check that location service is turned on in phone settings.");
            }
        }

        private void ReverseGeocodeQuery_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 0)
                {
                    MapAddress address = e.Result[0].Information.Address;
                    string countryCode = address.CountryCode;
                    int countryCodeIndex = -1;
                    for (int i = 0; i < threeLetterCountryCodes.Length; i++)
                    {
                        if (address.CountryCode == threeLetterCountryCodes[i])
                        {
                            countryCodeIndex = i;
                            break;
                        }
                    }
                    string twoLetterCountryCode = twoLetterCountryCodes[countryCodeIndex];

                    App.MusicApi.Initialize(twoLetterCountryCode);
                    App.MusicApi.GetArtistInfoForLocalAudio();
                    App.MusicApi.GetNewReleases();
                    App.MusicApi.GetTopArtists();
                    App.MusicApi.GetGenres();
                    App.MusicApi.GetMixGroups();

                }
            }
        }


        // implementation based on http://en.wikipedia.org/wiki/ISO_3166-1
        private string[] threeLetterCountryCodes = 
        {
            "AFG",
            "ALA",
            "ALB",
            "DZA",
            "ASM",
            "AND",
            "AGO",
            "AIA",
            "ATA",
            "ATG",
            "ARG",
            "ARM",
            "ABW",
            "AUS",
            "AUT",
            "AZE",
            "BHS",
            "BHR",
            "BGD",
            "BRB",
            "BLR",
            "BEL",
            "BLZ",
            "BEN",
            "BMU",
            "BTN",
            "BOL",
            "BES",
            "BIH",
            "BWA",
            "BVT",
            "BRA",
            "IOT",
            "BRN",
            "BGR",
            "BFA",
            "BDI",
            "KHM",
            "CMR",
            "CAN",
            "CPV",
            "CYM",
            "CAF",
            "TCD",
            "CHL",
            "CHN",
            "CXR",
            "CCK",
            "COL",
            "COM",
            "COG",
            "COD",
            "COK",
            "CRI",
            "CIV",
            "HRV",
            "CUB",
            "CUW",
            "CYP",
            "CZE",
            "DNK",
            "DJI",
            "DMA",
            "DOM",
            "ECU",
            "EGY",
            "SLV",
            "GNQ",
            "ERI",
            "EST",
            "ETH",
            "FLK",
            "FRO",
            "FJI",
            "FIN",
            "FRA",
            "GUF",
            "PYF",
            "ATF",
            "GAB",
            "GMB",
            "GEO",
            "DEU",
            "GHA",
            "GIB",
            "GRC",
            "GRL",
            "GRD",
            "GLP",
            "GUM",
            "GTM",
            "GGY",
            "GIN",
            "GNB",
            "GUY",
            "HTI",
            "HMD",
            "VAT",
            "HND",
            "HKG",
            "HUN",
            "ISL",
            "IND",
            "IDN",
            "IRN",
            "IRQ",
            "IRL",
            "IMN",
            "ISR",
            "ITA",
            "JAM",
            "JPN",
            "JEY",
            "JOR",
            "KAZ",
            "KEN",
            "KIR",
            "PRK",
            "KOR",
            "KWT",
            "KGZ",
            "LAO",
            "LVA",
            "LBN",
            "LSO",
            "LBR",
            "LBY",
            "LIE",
            "LTU",
            "LUX",
            "MAC",
            "MKD",
            "MDG",
            "MWI",
            "MYS",
            "MDV",
            "MLI",
            "MLT",
            "MHL",
            "MTQ",
            "MRT",
            "MUS",
            "MYT",
            "MEX",
            "FSM",
            "MDA",
            "MCO",
            "MNG",
            "MNE",
            "MSR",
            "MAR",
            "MOZ",
            "MMR",
            "NAM",
            "NRU",
            "NPL",
            "NLD",
            "NCL",
            "NZL",
            "NIC",
            "NER",
            "NGA",
            "NIU",
            "NFK",
            "MNP",
            "NOR",
            "OMN",
            "PAK",
            "PLW",
            "PSE",
            "PAN",
            "PNG",
            "PRY",
            "PER",
            "PHL",
            "PCN",
            "POL",
            "PRT",
            "PRI",
            "QAT",
            "REU",
            "ROU",
            "RUS",
            "RWA",
            "BLM",
            "SHN",
            "KNA",
            "LCA",
            "MAF",
            "SPM",
            "VCT",
            "WSM",
            "SMR",
            "STP",
            "SAU",
            "SEN",
            "SRB",
            "SYC",
            "SLE",
            "SGP",
            "SXM",
            "SVK",
            "SVN",
            "SLB",
            "SOM",
            "ZAF",
            "SGS",
            "SSD",
            "ESP",
            "LKA",
            "SDN",
            "SUR",
            "SJM",
            "SWZ",
            "SWE",
            "CHE",
            "SYR",
            "TWN",
            "TJK",
            "TZA",
            "THA",
            "TLS",
            "TGO",
            "TKL",
            "TON",
            "TTO",
            "TUN",
            "TUR",
            "TKM",
            "TCA",
            "TUV",
            "UGA",
            "UKR",
            "ARE",
            "GBR",
            "USA",
            "UMI",
            "URY",
            "UZB",
            "VUT",
            "VEN",
            "VNM",
            "VGB",
            "VIR",
            "WLF",
            "ESH",
            "YEM",
            "ZMB",
            "ZWE"
        };

        // implementation based on http://en.wikipedia.org/wiki/ISO_3166-1
        private string[] twoLetterCountryCodes = 
        {
            "AF",
            "AX",
            "AL",
            "DZ",
            "AS",
            "AD",
            "AO",
            "AI",
            "AQ",
            "AG",
            "AR",
            "AM",
            "AW",
            "AU",
            "AT",
            "AZ",
            "BS",
            "BH",
            "BD",
            "BB",
            "BY",
            "BE",
            "BZ",
            "BJ",
            "BM",
            "BT",
            "BO",
            "BQ",
            "BA",
            "BW",
            "BV",
            "BR",
            "IO",
            "BN",
            "BG",
            "BF",
            "BI",
            "KH",
            "CM",
            "CA",
            "CV",
            "KY",
            "CF",
            "TD",
            "CL",
            "CN",
            "CX",
            "CC",
            "CO",
            "KM",
            "CG",
            "CD",
            "CK",
            "CR",
            "CI",
            "HR",
            "CU",
            "CW",
            "CY",
            "CZ",
            "DK",
            "DJ",
            "DM",
            "DO",
            "EC",
            "EG",
            "SV",
            "GQ",
            "ER",
            "EE",
            "ET",
            "FK",
            "FO",
            "FJ",
            "FI",
            "FR",
            "GF",
            "PF",
            "TF",
            "GA",
            "GM",
            "GE",
            "DE",
            "GH",
            "GI",
            "GR",
            "GL",
            "GD",
            "GP",
            "GU",
            "GT",
            "GG",
            "GN",
            "GW",
            "GY",
            "HT",
            "HM",
            "VA",
            "HN",
            "HK",
            "HU",
            "IS",
            "IN",
            "ID",
            "IR",
            "IQ",
            "IE",
            "IM",
            "IL",
            "IT",
            "JM",
            "JP",
            "JE",
            "JO",
            "KZ",
            "KE",
            "KI",
            "KP",
            "KR",
            "KW",
            "KG",
            "LA",
            "LV",
            "LB",
            "LS",
            "LR",
            "LY",
            "LI",
            "LT",
            "LU",
            "MO",
            "MK",
            "MG",
            "MW",
            "MY",
            "MV",
            "ML",
            "MT",
            "MH",
            "MQ",
            "MR",
            "MU",
            "YT",
            "MX",
            "FM",
            "MD",
            "MC",
            "MN",
            "ME",
            "MS",
            "MA",
            "MZ",
            "MM",
            "NA",
            "NR",
            "NP",
            "NL",
            "NC",
            "NZ",
            "NI",
            "NE",
            "NG",
            "NU",
            "NF",
            "MP",
            "NO",
            "OM",
            "PK",
            "PW",
            "PS",
            "PA",
            "PG",
            "PY",
            "PE",
            "PH",
            "PN",
            "PL",
            "PT",
            "PR",
            "QA",
            "RE",
            "RO",
            "RU",
            "RW",
            "BL",
            "SH",
            "KN",
            "LC",
            "MF",
            "PM",
            "VC",
            "WS",
            "SM",
            "ST",
            "SA",
            "SN",
            "RS",
            "SC",
            "SL",
            "SG",
            "SX",
            "SK",
            "SI",
            "SB",
            "SO",
            "ZA",
            "GS",
            "SS",
            "ES",
            "LK",
            "SD",
            "SR",
            "SJ",
            "SZ",
            "SE",
            "CH",
            "SY",
            "TW",
            "TJ",
            "TZ",
            "TH",
            "TL",
            "TG",
            "TK",
            "TO",
            "TT",
            "TN",
            "TR",
            "TM",
            "TC",
            "TV",
            "UG",
            "UA",
            "AE",
            "GB",
            "US",
            "UM",
            "UY",
            "UZ",
            "VU",
            "VE",
            "VN",
            "VG",
            "VI",
            "WF",
            "EH",
            "YE",
            "ZM",
            "ZW",
        };
    }
}