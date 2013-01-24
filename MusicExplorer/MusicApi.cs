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
using System.Text;
using System.Threading.Tasks;
using Nokia.Music.Phone;
using Nokia.Music.Phone.Tasks;
using Nokia.Music.Phone.Types;
using System.Windows.Threading;

using MusicExplorer.ViewModels;
using System.Windows;
using System.Globalization;
using System.Collections.ObjectModel;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using System.ComponentModel;

namespace MusicExplorer
{
    /// <summary>
    /// 
    /// </summary>
    public class MusicApi
    {
        // Members
        private MusicClient client = null;
        private bool initialized = false;

        private Collection<Artist> localArtists = new Collection<Artist>();
        private Collection<string> progressIndicatorTexts = new Collection<string>();

        private int localAudioResponses = 0;
        private int recommendResponses = 0;

        /// <summary>
        /// 
        /// </summary>
        public MusicApi()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countryCode"></param>
        public void Initialize(string countryCode)
        {
            // Create a music client with correct AppId and Token/AppCode
            if (countryCode == null || countryCode.Length <= 0)
            {
                client = new MusicClient("v0Bh5kUAzCDp7PVS4kKr", "OfItN9r8E5QHfNO5mw-rEg");
            }
            else
            {
                client = new MusicClient("v0Bh5kUAzCDp7PVS4kKr", "OfItN9r8E5QHfNO5mw-rEg", countryCode);
            }
            initialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetArtistInfoForLocalAudio()
        {
            if (!initialized || localAudioResponses >= App.ViewModel.LocalAudio.Count)
            {
                return;
            }

            ArtistViewModel m = App.ViewModel.LocalAudio[localAudioResponses];

            client.SearchArtists((ListResponse<Artist> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        m.Id = response.Result[0].Id;
                        m.Country = GetCountryName(response.Result[0].Country);
                        m.Genres = response.Result[0].Genres[0].Name;
                        int itemHeight = Int32.Parse(m.ProportionalHeight);

                        if (response.Result[0].Thumb200Uri != null && itemHeight > 100)
                        {
                            m.ThumbUri = response.Result[0].Thumb200Uri;
                        }
                        else if (response.Result[0].Thumb100Uri != null)
                        {
                            m.ThumbUri = response.Result[0].Thumb100Uri;
                        }
                        else
                        {
                            m.ThumbUri = new Uri("/Assets/thumb_100_placeholder.png", UriKind.Relative);
                        }

                        localArtists.Add(response.Result[0]);
                    }

                    if (response != null && response.Error != null)
                    {
                        MessageBox.Show("Nokia Music API error. Please ensure that the device is connected to Internet and restart the application.");
                    }
                    HideProgressIndicator("GetArtistInfoForLocalAudio()");

                    localAudioResponses++;
                    GetArtistInfoForLocalAudio();

                    if (localAudioResponses == App.ViewModel.LocalAudio.Count)
                    {
                        // Request recommendations after receiving info for all local artists
                        FetchRecommendations();
                    }
                });
            }, m.Name);
            ShowProgressIndicator("GetArtistInfoForLocalAudio()");
        }

        /// <summary>
        /// 
        /// </summary>
        private void FetchRecommendations()
        {
            if (!initialized || localArtists.Count <= recommendResponses)
            {
                return;
            }

            client.GetSimilarArtists((ListResponse<Artist> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        foreach (Artist a in response.Result)
                        {
                            bool handled = false;

                            // Don't recommend artists already stored in device.
                            if (App.ViewModel.IsLocalArtist(a.Name))
                            {
                                handled = true;
                            }

                            // Check if the artist has already been recommended -> add some weight
                            // to recommendation.
                            if (!handled)
                            {
                                for (int i = 0; i < App.ViewModel.Recommendations.Count; i++)
                                {
                                    if (App.ViewModel.Recommendations[i].Name == a.Name)
                                    {
                                        handled = true;
                                        App.ViewModel.Recommendations[i].SimilarArtistCount++;

                                        // position according to count
                                        if (i > 0)
                                        {
                                            int j = 0;

                                            for (j = i - 1; j > 0; j--)
                                            {
                                                if (App.ViewModel.Recommendations[j].SimilarArtistCount >=
                                                    App.ViewModel.Recommendations[i].SimilarArtistCount)
                                                {
                                                    j++; // This item (j) has been ranked higher or equal - correct index is one more
                                                    break;
                                                }
                                            }

                                            if (i > j)
                                            {
                                                ArtistViewModel artist = App.ViewModel.Recommendations[i];
                                                App.ViewModel.Recommendations.RemoveAt(i);
                                                App.ViewModel.Recommendations.Insert(j, artist);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            // If the artist is not present in the device and has not yet been
                            // recommended, do it now.
                            if (!handled)
                            {
                                if (a.Thumb100Uri != null)
                                {
                                    App.ViewModel.Recommendations.Add(new ArtistViewModel()
                                        { 
                                            Name = a.Name, 
                                            Country = GetCountryName(a.Country), 
                                            Genres = a.Genres[0].Name, 
                                            ThumbUri = a.Thumb100Uri, 
                                            Id = a.Id, 
                                            SimilarArtistCount = 1
                                        });
                                }
                                else
                                {
                                    App.ViewModel.Recommendations.Add(new ArtistViewModel() 
                                        { 
                                            Name = a.Name, 
                                            Country = GetCountryName(a.Country), 
                                            Genres = a.Genres[0].Name, 
                                            ThumbUri = new Uri("/Assets/thumb_100_placeholder.png", 
                                                               UriKind.Relative), 
                                            Id = a.Id, 
                                            SimilarArtistCount = 1
                                        });
                                }
                            }
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        MessageBox.Show("Nokia Music API error. Please ensure that the device is connected to Internet and restart the application.");
                    }
                    HideProgressIndicator("FetchRecommendations()");

                    recommendResponses++;
                    FetchRecommendations();
                });
            }, localArtists[recommendResponses].Id);
            ShowProgressIndicator("FetchRecommendations()");
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetTopArtists()
        {
            if (!initialized)
            {
                return;
            }

            client.GetTopArtists((ListResponse<Artist> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        App.ViewModel.TopArtists.Clear();

                        foreach (Artist a in response.Result)
                        {
                            if (a.Thumb100Uri != null)
                            {
                                App.ViewModel.TopArtists.Add(new ArtistViewModel() 
                                    { 
                                        Name = a.Name, 
                                        Country = GetCountryName(a.Country), 
                                        Genres = a.Genres[0].Name, 
                                        ThumbUri = a.Thumb100Uri, 
                                        Id = a.Id
                                    });
                            }
                            else
                            {
                                App.ViewModel.TopArtists.Add(new ArtistViewModel() 
                                    { 
                                        Name = a.Name, 
                                        Country = GetCountryName(a.Country), 
                                        Genres = a.Genres[0].Name, 
                                        ThumbUri = new Uri("/Assets/thumb_100_placeholder.png", 
                                                           UriKind.Relative), 
                                        Id = a.Id
                                    });
                            }
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        MessageBox.Show("Nokia Music API error. Please ensure that the device is connected to Internet and restart the application.");
                    }
                    HideProgressIndicator("GetTopArtists()");
                });
            });
            ShowProgressIndicator("GetTopArtists()");
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetNewReleases()
        {
            if (!initialized)
            {
                return;
            }

            client.GetNewReleases((ListResponse<Product> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        App.ViewModel.NewReleases.Clear();

                        foreach (Product p in response.Result)
                        {
                            string categoryString = "Album";

                            if (p.Category == Category.Single)
                            {
                                categoryString = "Track";
                            }
                            else if (p.Category == Category.Track)
                            {
                                categoryString = "Track";
                            }

                            string performersString = "";

                            if (p.Performers != null && p.Performers.Length > 0)
                            {
                                performersString = p.Performers[0].Name;
                            }

                            if (p.Thumb100Uri != null)
                            {
                                App.ViewModel.NewReleases.Add(new ProductViewModel()
                                    {
                                        Performers = performersString,
                                        Name = p.Name,
                                        Category = categoryString,
                                        ThumbUri = p.Thumb100Uri,
                                        Id = p.Id
                                    });
                            }
                            else
                            {
                                App.ViewModel.NewReleases.Add(new ProductViewModel()
                                    {
                                        Performers = performersString,
                                        Name = p.Name,
                                        Category = categoryString,
                                        ThumbUri = new Uri("/Assets/thumb_100_placeholder.png",
                                                           UriKind.Relative),
                                        Id = p.Id
                                    });
                            }
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        MessageBox.Show("Nokia Music API error. Please ensure that the device is connected to Internet and restart the application.");
                    }
                    HideProgressIndicator("GetNewReleases()");
                });
            }, Category.Album);
            ShowProgressIndicator("GetNewReleases()");
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetGenres()
        {
            if (!initialized)
            {
                return;
            }

            client.GetGenres((ListResponse<Genre> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        int genreCount = response.Count();
                        App.ViewModel.Genres.Clear();

                        foreach (Genre g in response.Result)
                        {
                            App.ViewModel.Genres.Add(new GenreViewModel() { Name = g.Name, Id = g.Id });
                        }
                    }
                    if (response != null && response.Error != null)
                    {
                        MessageBox.Show("Nokia Music API error. Please ensure that the device is connected to Internet and restart the application.");
                    }
                    HideProgressIndicator("GetGenres()");
                });
            });
            ShowProgressIndicator("GetGenres()");
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetTopArtistsForGenre(string id)
        {
            if (!initialized)
            {
                return;
            }

            client.GetTopArtistsForGenre((ListResponse<Artist> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        App.ViewModel.TopArtistsForGenre.Clear();

                        foreach (Artist a in response.Result)
                        {
                            if (a.Thumb100Uri != null)
                            {
                                App.ViewModel.TopArtistsForGenre.Add(new ArtistViewModel() 
                                    { 
                                        Name = a.Name, 
                                        Country = GetCountryName(a.Country), 
                                        Genres = a.Genres[0].Name, 
                                        ThumbUri = a.Thumb100Uri, 
                                        Id = a.Id
                                    });
                            }
                            else
                            {
                                App.ViewModel.TopArtistsForGenre.Add(new ArtistViewModel() 
                                    { 
                                        Name = a.Name, 
                                        Country = GetCountryName(a.Country), 
                                        Genres = a.Genres[0].Name, 
                                        ThumbUri = new Uri("/Assets/thumb_100_placeholder.png", 
                                                           UriKind.Relative), 
                                        Id = a.Id
                                    });
                            }
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        MessageBox.Show("Nokia Music API error. Please ensure that the device is connected to Internet and restart the application.");
                    }
                    HideProgressIndicator("GetTopArtistsForGenre()");
                });
            }, id);
            ShowProgressIndicator("GetTopArtistsForGenre()");
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetMixGroups()
        {
            if (!initialized)
            {
                return;
            }

            client.GetMixGroups((ListResponse<MixGroup> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        App.ViewModel.MixGroups.Clear();

                        foreach (MixGroup mg in response.Result)
                        {
                            App.ViewModel.MixGroups.Add(new MixGroupViewModel()
                                {
                                    Name = mg.Name,
                                    Id = mg.Id
                                });
                        }
                    }
					
                    if (response != null && response.Error != null)
                    {
                        MessageBox.Show("Nokia Music API error. Please ensure that the device is connected to Internet and restart the application.");
                    }
                    HideProgressIndicator("GetMixGroups()");
                });
            });
            ShowProgressIndicator("GetMixGroups()");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void GetMixes(string id)
        {
            if (!initialized)
            {
                return;
            }

            client.GetMixes((ListResponse<Mix> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        App.ViewModel.Mixes.Clear();

                        foreach (Mix m in response.Result)
                        {
                            string parentalAdvisoryString = "";

                            if (m.ParentalAdvisory)
                            {
                                parentalAdvisoryString = "Parental advisory";
                            }

                            App.ViewModel.Mixes.Add(new MixViewModel()
                                {
                                    Name = m.Name,
                                    ParentalAdvisory = parentalAdvisoryString,
                                    Id = m.Id,
                                    ThumbUri = m.Thumb100Uri
                                });
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        MessageBox.Show("Nokia Music API error. Please ensure that the device is connected to Internet and restart the application.");
                    }
                    HideProgressIndicator("GetMixes()");
                });
            }, id);
            ShowProgressIndicator("GetMixes()");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void GetProductsForArtist(string id)
        {
            if (!initialized)
            {
                return;
            }

            App.ViewModel.TracksForArtist.Clear();
            App.ViewModel.AlbumsForArtist.Clear();
            App.ViewModel.SinglesForArtist.Clear();

            App.ViewModel.NoTracksVisibility = Visibility.Visible;
            App.ViewModel.NoAlbumsVisibility = Visibility.Visible;
            App.ViewModel.NoSinglesVisibility = Visibility.Visible;

            client.GetArtistProducts((ListResponse<Product> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        foreach (Product p in response.Result)
                        {
                            string priceString = "";

                            if (p.Price != null)
                            {
                                priceString = p.Price.Value + p.Price.Currency;
                            }

                            string takenFromString = "";

                            if (p.TakenFrom != null)
                            {
                                takenFromString = p.TakenFrom.Name;
                            }

                            string performersString = "";

                            if (p.Performers != null && p.Performers.Length > 0)
                            {
                                performersString = p.Performers[0].Name;
                            }

                            string genresString = "";

                            if (p.Genres != null && p.Genres.Length > 0)
                            {
                                genresString = p.Genres[0].Name;
                            }

                            Uri thumbUri;

                            if (p.Thumb100Uri != null)
                            {
                                thumbUri = p.Thumb100Uri;
                            }
                            else
                            {
                                thumbUri = new Uri("/Assets/thumb_100_placeholder.png",
                                                   UriKind.Relative);
                            }

                            string categoryString = "Album";

                            if (p.Category == Category.Track)
                            {
                                categoryString = "Track";
                                App.ViewModel.TracksForArtist.Add(new ProductViewModel()
                                    {
                                        Performers = performersString,
                                        Name = p.Name,
                                        Category = categoryString,
                                        ThumbUri = thumbUri,
                                        Id = p.Id
                                    });
                            }
                            else if (p.Category == Category.Single)
                            {
                                categoryString = "Single";
                                App.ViewModel.SinglesForArtist.Add(new ProductViewModel()
                                    {
                                        Performers = performersString,
                                        Name = p.Name,
                                        Category = categoryString,
                                        ThumbUri = thumbUri,
                                        Id = p.Id
                                    });
                            }
                            else
                            {
                                App.ViewModel.AlbumsForArtist.Add(new ProductViewModel()
                                    {
                                        Performers = performersString,
                                        Name = p.Name,
                                        Category = categoryString,
                                        ThumbUri = thumbUri,
                                        Id = p.Id
                                    });
                            }
                        }

                        if (App.ViewModel.TracksForArtist.Count > 0)
                        {
                            App.ViewModel.NoTracksVisibility = Visibility.Collapsed;
                        }

                        if (App.ViewModel.AlbumsForArtist.Count > 0)
                        {
                            App.ViewModel.NoAlbumsVisibility = Visibility.Collapsed;
                        }

                        if (App.ViewModel.SinglesForArtist.Count > 0)
                        {
                            App.ViewModel.NoSinglesVisibility = Visibility.Collapsed;
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        MessageBox.Show("Nokia Music API error. Please ensure that the device is connected to Internet and restart the application.");
                    }
                    HideProgressIndicator("GetProductsForArtist()");
                });
            }, id, null, 0, 30);
            ShowProgressIndicator("GetProductsForArtist()");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void GetSimilarArtists(string id)
        {
            if (!initialized)
            {
                return;
            }

            App.ViewModel.SimilarForArtist.Clear();
            App.ViewModel.NoSimilarVisibility = Visibility.Visible;

            client.GetSimilarArtists((ListResponse<Artist> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        foreach (Artist a in response.Result)
                        {
                            if (a.Thumb100Uri != null)
                            {
                                App.ViewModel.SimilarForArtist.Add(new ArtistViewModel() 
                                    { 
                                        Name = a.Name, 
                                        Country = GetCountryName(a.Country), 
                                        Genres = a.Genres[0].Name, 
                                        ThumbUri = a.Thumb100Uri, 
                                        Id = a.Id
                                    });
                            }
                            else
                            {
                                App.ViewModel.SimilarForArtist.Add(new ArtistViewModel() 
                                { 
                                    Name = a.Name, 
                                    Country = GetCountryName(a.Country), 
                                    Genres = a.Genres[0].Name, 
                                    ThumbUri = new Uri("/Assets/thumb_100_placeholder.png", 
                                                       UriKind.Relative), 
                                    Id = a.Id 
                                });
                            }
                        }
                    }

                    if (App.ViewModel.SimilarForArtist.Count() > 0)
                    {
                        App.ViewModel.NoSimilarVisibility = Visibility.Collapsed;
                    }


                    if (response != null && response.Error != null)
                    {
                        MessageBox.Show("Nokia Music API error. Please ensure that the device is connected to Internet and restart the application.");
                    }
                    HideProgressIndicator("GetSimilarArtists()");
                });
            }, id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void LaunchMix(string id)
        {
            if (!initialized)
            {
                return;
            }

            PlayMixTask task = new PlayMixTask();
            task.MixId = id;
            task.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="artistName"></param>
        public void LaunchArtistMix(string artistName)
        {
            if (!initialized)
            {
                return;
            }

            PlayMixTask task = new PlayMixTask();
            task.ArtistName = artistName;
            task.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void LaunchProduct(string id)
        {
            if (!initialized)
            {
                return;
            }

            ShowProductTask task = new ShowProductTask();
            task.ProductId = id;
            task.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void LaunchArtist(string id)
        {
            if (!initialized)
            {
                return;
            }

            ShowArtistTask task = new ShowArtistTask();
            task.ArtistId = id;
            task.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localArtistName"></param>
        public void PlayLocalArtist(string localArtistName)
        {
            Microsoft.Xna.Framework.Media.MediaLibrary lib =
                new Microsoft.Xna.Framework.Media.MediaLibrary();

            for (int i = 0; i < lib.Artists.Count; i++)
            {
                if (localArtistName == lib.Artists[i].Name)
                {
                    // generate a random track index
                    Random rand = new Random();
                    int track = rand.Next(0, lib.Artists[i].Songs.Count - 1);
                    
                    Microsoft.Xna.Framework.Media.SongCollection songCollection = lib.Artists[i].Songs;
                    Microsoft.Xna.Framework.Media.MediaPlayer.Play(songCollection, track);
                    Microsoft.Xna.Framework.Media.MediaPlayer.IsShuffled = true;
                    Microsoft.Xna.Framework.FrameworkDispatcher.Update();
                    break;
                }
            }
        }

        private void ShowProgressIndicator(string text)
        {
            progressIndicatorTexts.Add(text);
            App.ViewModel.ProgressIndicatorText = progressIndicatorTexts[progressIndicatorTexts.Count - 1];
            App.ViewModel.ProgressIndicatorVisible = true;
        }

        private void HideProgressIndicator(string text)
        {
            progressIndicatorTexts.Remove(text);
            if (progressIndicatorTexts.Count > 0)
            {
                App.ViewModel.ProgressIndicatorText = progressIndicatorTexts[progressIndicatorTexts.Count - 1];
            }
            else
            {
                App.ViewModel.ProgressIndicatorText = "";
                App.ViewModel.ProgressIndicatorVisible = false;
            }
        }

        public string GetCountryName(string countryCode)
        {
            string ret = "";
            if (countryCode == null) return ret;

            countryCode = countryCode.ToLower();
            for (int i = 0; i < twoLetterCountryCodes.Length; i++)
            {
                if (countryCode == twoLetterCountryCodes[i].ToLower())
                {
                    ret = CountryNames[i];
                    break;
                }
            }
            return ret;
        }

        public string GetTwoLetterCountryCode(string threeLetterCountryCode)
        {
            string ret = "";
            if (threeLetterCountryCode == null) return ret;

            threeLetterCountryCode = threeLetterCountryCode.ToLower();
            for (int i = 0; i < threeLetterCountryCodes.Length; i++)
            {
                if (threeLetterCountryCode == threeLetterCountryCodes[i].ToLower())
                {
                    ret = twoLetterCountryCodes[i];
                    break;
                }
            }
            return ret;
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

        // implementation based on http://en.wikipedia.org/wiki/ISO_3166-1
        private string[] CountryNames = 
        {
            "Afghanistan",
            "Åland Islands",
            "Albania",
            "Algeria",
            "American Samoa",
            "Andorra",
            "Angola",
            "Anguilla",
            "Antarctica",
            "Antigua and Barbuda",
            "Argentina",
            "Armenia",
            "Aruba",
            "Australia",
            "Austria",
            "Azerbaijan",
            "Bahamas",
            "Bahrain",
            "Bangladesh",
            "Barbados",
            "Belarus",
            "Belgium",
            "Belize",
            "Benin",
            "Bermuda",
            "Bhutan",
            "Bolivia, Plurinational State of",
            "Bonaire, Sint Eustatius and Saba",
            "Bosnia and Herzegovina",
            "Botswana",
            "Bouvet Island",
            "Brazil",
            "British Indian Ocean Territory",
            "Brunei Darussalam",
            "Bulgaria",
            "Burkina Faso",
            "Burundi",
            "Cambodia",
            "Cameroon",
            "Canada",
            "Cape Verde",
            "Cayman Islands",
            "Central African Republic",
            "Chad",
            "Chile",
            "China",
            "Christmas Island",
            "Cocos (Keeling) Islands",
            "Colombia",
            "Comoros",
            "Congo",
            "Congo, the Democratic Republic of the",
            "Cook Islands",
            "Costa Rica",
            "Côte d'Ivoire",
            "Croatia",
            "Cuba",
            "Curaçao",
            "Cyprus",
            "Czech Republic",
            "Denmark",
            "Djibouti",
            "Dominica",
            "Dominican Republic",
            "Ecuador",
            "Egypt",
            "El Salvador",
            "Equatorial Guinea",
            "Eritrea",
            "Estonia",
            "Ethiopia",
            "Falkland Islands (Malvinas)",
            "Faroe Islands",
            "Fiji",
            "Finland",
            "France",
            "French Guiana",
            "French Polynesia",
            "French Southern Territories",
            "Gabon",
            "Gambia",
            "Georgia",
            "Germany",
            "Ghana",
            "Gibraltar",
            "Greece",
            "Greenland",
            "Grenada",
            "Guadeloupe",
            "Guam",
            "Guatemala",
            "Guernsey",
            "Guinea",
            "Guinea-Bissau",
            "Guyana",
            "Haiti",
            "Heard Island and McDonald Islands",
            "Holy See (Vatican City State)",
            "Honduras",
            "Hong Kong",
            "Hungary",
            "Iceland",
            "India",
            "Indonesia",
            "Iran, Islamic Republic of",
            "Iraq",
            "Ireland",
            "Isle of Man",
            "Israel",
            "Italy",
            "Jamaica",
            "Japan",
            "Jersey",
            "Jordan",
            "Kazakhstan",
            "Kenya",
            "Kiribati",
            "Korea, Democratic People's Republic of",
            "Korea, Republic of",
            "Kuwait",
            "Kyrgyzstan",
            "Lao People's Democratic Republic",
            "Latvia",
            "Lebanon",
            "Lesotho",
            "Liberia",
            "Libya",
            "Liechtenstein",
            "Lithuania",
            "Luxembourg",
            "Macao",
            "Macedonia, The Former Yugoslav Republic of",
            "Madagascar",
            "Malawi",
            "Malaysia",
            "Maldives",
            "Mali",
            "Malta",
            "Marshall Islands",
            "Martinique",
            "Mauritania",
            "Mauritius",
            "Mayotte",
            "Mexico",
            "Micronesia, Federated States of",
            "Moldova, Republic of",
            "Monaco",
            "Mongolia",
            "Montenegro",
            "Montserrat",
            "Morocco",
            "Mozambique",
            "Myanmar",
            "Namibia",
            "Nauru",
            "Nepal",
            "Netherlands",
            "New Caledonia",
            "New Zealand",
            "Nicaragua",
            "Niger",
            "Nigeria",
            "Niue",
            "Norfolk Island",
            "Northern Mariana Islands",
            "Norway",
            "Oman",
            "Pakistan",
            "Palau",
            "Palestinian Territory, Occupied",
            "Panama",
            "Papua New Guinea",
            "Paraguay",
            "Peru",
            "Philippines",
            "Pitcairn",
            "Poland",
            "Portugal",
            "Puerto Rico",
            "Qatar",
            "Réunion",
            "Romania",
            "Russian Federation",
            "Rwanda",
            "Saint Barthélemy",
            "Saint Helena, Ascension and Tristan da Cunha",
            "Saint Kitts and Nevis",
            "Saint Lucia",
            "Saint Martin (French part)",
            "Saint Pierre and Miquelon",
            "Saint Vincent and the Grenadines",
            "Samoa",
            "San Marino",
            "Sao Tome and Principe",
            "Saudi Arabia",
            "Senegal",
            "Serbia",
            "Seychelles",
            "Sierra Leone",
            "Singapore",
            "Sint Maarten (Dutch part)",
            "Slovakia",
            "Slovenia",
            "Solomon Islands",
            "Somalia",
            "South Africa",
            "South Georgia and the South Sandwich Islands",
            "South Sudan",
            "Spain",
            "Sri Lanka",
            "Sudan",
            "Suriname",
            "Svalbard and Jan Mayen",
            "Swaziland",
            "Sweden",
            "Switzerland",
            "Syrian Arab Republic",
            "Taiwan, Province of China",
            "Tajikistan",
            "Tanzania, United Republic of",
            "Thailand",
            "Timor-Leste",
            "Togo",
            "Tokelau",
            "Tonga",
            "Trinidad and Tobago",
            "Tunisia",
            "Turkey",
            "Turkmenistan",
            "Turks and Caicos Islands",
            "Tuvalu",
            "Uganda",
            "Ukraine",
            "United Arab Emirates",
            "United Kingdom",
            "United States",
            "United States Minor Outlying Islands",
            "Uruguay",
            "Uzbekistan",
            "Vanuatu",
            "Venezuela, Bolivarian Republic of",
            "Viet Nam",
            "Virgin Islands, British",
            "Virgin Islands, U.S.",
            "Wallis and Futuna",
            "Western Sahara",
            "Yemen",
            "Zambia",
            "Zimbabwe"
        };
    }
}
