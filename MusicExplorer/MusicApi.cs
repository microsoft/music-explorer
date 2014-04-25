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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;

using Nokia.Music;
using Nokia.Music.Tasks;
using Nokia.Music.Types;

using MusicExplorer.Models;


namespace MusicExplorer
{
    /// <summary>
    /// This class represents Nokia Music API to the rest of the application.
    /// All requests to Nokia Music API are sent by an instance of this class,
    /// and all the responses from Nokia Music API are handled by this class.
    /// </summary>
    public class MusicApi
    {
        // Constants
        public const string MUSIC_EXPLORER_APP_ID = "music_explorer_private_app_id"; // real app id not shown here
        public const string MUSIC_EXPLORER_APP_TOKEN = "music_explorer_private_app_token"; // real app token not shown here

        // Members
        private MusicClient client = null;
        private bool initialized = false;

        private Collection<Artist> localArtists = new Collection<Artist>();
        private Collection<string> progressIndicatorTexts = new Collection<string>();

        private int localAudioResponses = 0;
        private int recommendResponses = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MusicApi()
        {
        }

        /// <summary>
        /// Initializes Nokia Music API for further requests. Responses to  
        /// requests depend on the region - TopArtists are country specific
        /// for example, and genres are localized by the region.
        /// </summary>
        /// <param name="countryCode">An ISO 3166-2 country code validated by the Nokia Music API CountryResolver</param>
        public void Initialize(string countryCode)
        {
            // Create a music client with correct AppId and Token/AppCode
            if (countryCode == null || countryCode.Length != 2)
            {
                client = new MusicClient(MUSIC_EXPLORER_APP_ID);
            }
            else
            {
                client = new MusicClient(MUSIC_EXPLORER_APP_ID, 
                                         countryCode.ToLower());
            }
            initialized = true;
        }

        /// <summary>
        /// Retrieves information (id, genre, thumbnail, etc.) for local artists.
        /// This method initiates a chain of requests, which
        /// 1. requests artist information for each of the local artists
        ///    one after another.
        /// 2. Initiates recommendations searching.
        /// </summary>
        public void GetArtistInfoForLocalAudio()
        {
            if (!initialized || localAudioResponses >= App.ViewModel.LocalAudio.Count)
            {
                return;
            }

            ArtistModel m = App.ViewModel.LocalAudio[localAudioResponses];

            client.SearchArtists((ListResponse<Artist> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        m.Id = response.Result[0].Id;
                        m.Country = CountryCodes.CountryNameFromTwoLetter(response.Result[0].Country);
                        m.Genres = response.Result[0].Genres[0].Name;
                        int itemHeight = Int32.Parse(m.ItemHeight);

                        m.Thumb100Uri = response.Result[0].Thumb100Uri;
                        m.Thumb200Uri = response.Result[0].Thumb200Uri;
                        m.Thumb320Uri = response.Result[0].Thumb320Uri;

                        localArtists.Add(response.Result[0]);
                    }

                    if (response != null && response.Error != null)
                    {
                        ShowNokiaMusicApiError();
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
        /// Builds the recommended artists lists.
        /// This method initiates a chain of requests, which requests similar
        /// artists information for each of the local artists one after another.
        /// </summary>
        private void FetchRecommendations()
        {
            if (!initialized || localArtists.Count <= recommendResponses)
            {
                if (initialized && App.ViewModel.Recommendations.Count <= 0)
                {
                    App.ViewModel.NoRecommendedVisibility = Visibility.Visible;
                }
                else
                {
                    App.ViewModel.NoRecommendedVisibility = Visibility.Collapsed;
                }

                // limit the number of recommended artists to 20
                if (localArtists.Count == recommendResponses)
                {
                    int i = App.ViewModel.Recommendations.Count - 1;
                    while (i > 20)
                    {
                        App.ViewModel.Recommendations.RemoveAt(i);
                        i--;
                    }
                }

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

                                        // position according to weight
                                        if (i > 1)
                                        {
                                            int j = 1;

                                            for (j = i - 1; j > 1; j--)
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
                                                ArtistModel artist = App.ViewModel.Recommendations[i];
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
                                App.ViewModel.Recommendations.Add(new ArtistModel()
                                {
                                    Name = a.Name,
                                    Country = CountryCodes.CountryNameFromTwoLetter(a.Country),
                                    Genres = a.Genres[0].Name,
                                    Thumb100Uri = a.Thumb100Uri,
                                    Thumb200Uri = a.Thumb200Uri,
                                    Thumb320Uri = a.Thumb320Uri,
                                    Id = a.Id,
                                    SimilarArtistCount = 1,
                                    ItemWidth = "205",
                                    ItemHeight = "205"
                                });
                            }
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        ShowNokiaMusicApiError();
                    }
                    HideProgressIndicator("FetchRecommendations()");

                    recommendResponses++;
                    FetchRecommendations();
                });
            }, localArtists[recommendResponses].Id);
            ShowProgressIndicator("FetchRecommendations()");
        }

        /// <summary>
        /// Retrieves top artists (10 most popular) from Nokia Music API.
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

                        // Insert a place holder for title text
                        App.ViewModel.TopArtists.Add(new ArtistModel()
                        {
                            Name = "TitlePlaceholderwho's hot",
                            ItemHeight = "110",
                            ItemWidth = "450"
                        });

                        foreach (Artist a in response.Result)
                        {
                            App.ViewModel.TopArtists.Add(new ArtistModel()
                            {
                                Name = a.Name,
                                Country = CountryCodes.CountryNameFromTwoLetter(a.Country),
                                Genres = a.Genres[0].Name,
                                Thumb100Uri = a.Thumb100Uri,
                                Thumb200Uri = a.Thumb200Uri,
                                Thumb320Uri = a.Thumb320Uri,
                                Id = a.Id,
                                ItemWidth = "205",
                                ItemHeight = "205"
                            });
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        ShowNokiaMusicApiError();
                    }
                    HideProgressIndicator("GetTopArtists()");
                });
            });
            ShowProgressIndicator("GetTopArtists()");
        }

        /// <summary>
        /// Retrieves new releases (10 latest albums) from Nokia Music API.
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

                        // Insert a place holder for title text
                        App.ViewModel.NewReleases.Add(new ProductModel()
                        {
                            Performers = "TitlePlaceholderwhat's new",
                            ItemHeight = "110",
                            ItemWidth = "450"
                        });

                        foreach (Product p in response.Result)
                        {
                            string categoryString = "Album";

                            if (p.Category == Category.Single)
                            {
                                categoryString = "Single";
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

                            App.ViewModel.NewReleases.Add(new ProductModel()
                            {
                                Performers = performersString,
                                Name = p.Name,
                                Category = categoryString,
                                Thumb100Uri = p.Thumb100Uri,
                                Thumb200Uri = p.Thumb200Uri,
                                Thumb320Uri = p.Thumb320Uri,
                                Id = p.Id,
                                ItemWidth = "205",
                                ItemHeight = "205"
                            });
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        ShowNokiaMusicApiError();
                    }
                    HideProgressIndicator("GetNewReleases()");
                });
            }, Category.Album);
            ShowProgressIndicator("GetNewReleases()");
        }

        /// <summary>
        /// Retrieves available genres from Nokia Music API.
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
                            App.ViewModel.Genres.Add(new GenreModel() { Name = g.Name, Id = g.Id });
                        }
                    }
                    if (response != null && response.Error != null)
                    {
                        ShowNokiaMusicApiError();
                    }
                    HideProgressIndicator("GetGenres()");
                });
            });
            ShowProgressIndicator("GetGenres()");
        }

        /// <summary>
        /// Retrieves top artists of a selected genre from Nokia Music API.
        /// </summary>
        /// <param name="id">Id of the genre.</param>
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
                            App.ViewModel.TopArtistsForGenre.Add(new ArtistModel()
                            {
                                Name = a.Name,
                                Country = CountryCodes.CountryNameFromTwoLetter(a.Country),
                                Genres = a.Genres[0].Name,
                                Thumb100Uri = a.Thumb100Uri,
                                Thumb200Uri = a.Thumb200Uri,
                                Thumb320Uri = a.Thumb320Uri,
                                Id = a.Id,
                                ItemWidth = "205",
                                ItemHeight = "205"
                            });
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        ShowNokiaMusicApiError();
                    }
                    HideProgressIndicator("GetTopArtistsForGenre()");
                });
            }, id);
            ShowProgressIndicator("GetTopArtistsForGenre()");
        }

        /// <summary>
        /// Retrieves available mix groups from Nokia Music API.
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
                            App.ViewModel.MixGroups.Add(new MixGroupModel()
                                {
                                    Name = mg.Name,
                                    Id = mg.Id
                                });
                        }
                    }
					
                    if (response != null && response.Error != null)
                    {
                        ShowNokiaMusicApiError();
                    }
                    HideProgressIndicator("GetMixGroups()");
                });
            });
            ShowProgressIndicator("GetMixGroups()");
        }

        /// <summary>
        /// Retrieves available mixes in a selected mix group from Nokia Music API.
        /// </summary>
        /// <param name="id">Id of the mix group.</param>
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

                            App.ViewModel.Mixes.Add(new MixModel()
                            {
                                Name = m.Name,
                                ParentalAdvisory = parentalAdvisoryString,
                                Id = m.Id,
                                Thumb100Uri = m.Thumb100Uri,
                                Thumb200Uri = m.Thumb200Uri,
                                Thumb320Uri = m.Thumb320Uri,
                                ItemWidth = "205",
                                ItemHeight = "205"
                            });
                        }
                    }

                    if (response != null && response.Error != null)
                    {
                        ShowNokiaMusicApiError();
                    }
                    HideProgressIndicator("GetMixes()");
                });
            }, id);
            ShowProgressIndicator("GetMixes()");
        }

        /// <summary>
        /// Retrieves 30 products for a selected artist from Nokia Music API.
        /// </summary>
        /// <param name="id">Id of the artist.</param>
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

                            string categoryString = "Album";

                            string trackCountString = null;
                            if (p.TrackCount > 0)
                            {
                                trackCountString = p.TrackCount + " track";
                            }
                            if (p.TrackCount > 1)
                            {
                                trackCountString += "s";
                            }

                            if (p.Category == Category.Track)
                            {
                                categoryString = "Track";
                                App.ViewModel.TracksForArtist.Add(new ProductModel()
                                    {
                                        Performers = performersString,
                                        Name = p.Name,
                                        TakenFrom = takenFromString,
                                        Category = categoryString,
                                        Thumb100Uri = p.Thumb100Uri,
                                        Thumb200Uri = p.Thumb200Uri,
                                        Thumb320Uri = p.Thumb320Uri,
                                        Id = p.Id
                                    });
                            }
                            else if (p.Category == Category.Single)
                            {
                                categoryString = "Single";
                                App.ViewModel.SinglesForArtist.Add(new ProductModel()
                                    {
                                        Performers = performersString,
                                        Name = p.Name,
                                        TrackCount = trackCountString,
                                        Category = categoryString,
                                        Thumb100Uri = p.Thumb100Uri,
                                        Thumb200Uri = p.Thumb200Uri,
                                        Thumb320Uri = p.Thumb320Uri,
                                        Id = p.Id
                                    });
                            }
                            else
                            {
                                App.ViewModel.AlbumsForArtist.Add(new ProductModel()
                                    {
                                        Performers = performersString,
                                        Name = p.Name,
                                        TrackCount = trackCountString,
                                        Category = categoryString,
                                        Thumb100Uri = p.Thumb100Uri,
                                        Thumb200Uri = p.Thumb200Uri,
                                        Thumb320Uri = p.Thumb320Uri,
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
                        ShowNokiaMusicApiError();
                    }
                    HideProgressIndicator("GetProductsForArtist()");
                });
            }, id, null, 0, 30);
            ShowProgressIndicator("GetProductsForArtist()");
        }

        /// <summary>
        /// Retrieves similar artists for a selected artist from Nokia Music API.
        /// </summary>
        /// <param name="id">Id of the artist.</param>
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
                            App.ViewModel.SimilarForArtist.Add(new ArtistModel()
                            {
                                Name = a.Name,
                                Country = CountryCodes.CountryNameFromTwoLetter(a.Country),
                                Genres = a.Genres[0].Name,
                                Thumb100Uri = a.Thumb100Uri,
                                Thumb200Uri = a.Thumb200Uri,
                                Thumb320Uri = a.Thumb320Uri,
                                Id = a.Id,
                                ItemWidth = "205",
                                ItemHeight = "205"
                            });
                        }
                    }

                    if (App.ViewModel.SimilarForArtist.Count() > 0)
                    {
                        App.ViewModel.NoSimilarVisibility = Visibility.Collapsed;
                    }


                    if (response != null && response.Error != null)
                    {
                        ShowNokiaMusicApiError();
                    }
                    HideProgressIndicator("GetSimilarArtists()");
                });
            }, id);
        }

        /// <summary>
        /// Launches Nokia Music App to play a selected mix.
        /// </summary>
        /// <param name="id">Id of the mix.</param>
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
        /// Launches Nokia Music App to play a mix for a selected artist.
        /// </summary>
        /// <param name="artistName">Name of the artist.</param>
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
        /// Launches Nokia Music App to show information on a selected product.
        /// </summary>
        /// <param name="id">Id of the product.</param>
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
        /// Launches Nokia Music App to show information on a selected artist.
        /// </summary>
        /// <param name="id">id of the artist.</param>
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
        /// This method makes progress indicator visible.
        /// Provided text is added into an array of strings of which the
        /// latest text is shown in progress indicator.
        /// </summary>
        /// <param name="text">Text to show in progress indicator.</param>
        private void ShowProgressIndicator(string text)
        {
            progressIndicatorTexts.Add(text);
            App.ViewModel.ProgressIndicatorText = progressIndicatorTexts[progressIndicatorTexts.Count - 1];
            App.ViewModel.ProgressIndicatorVisible = true;
        }

        /// <summary>
        /// This method removes provided text from the array of strings of
        /// which the latest text is shown in progress indicator.
        /// Indicator is hidden if the array becomes empty. 
        /// </summary>
        /// <param name="text">Text to be removed from progress indicator.</param>
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

        /// <summary>
        /// Shows MessageBox with Nokia Music API error text
        /// </summary>
        void ShowNokiaMusicApiError()
        {
            MessageBox.Show("Nokia Music API error. Please ensure that the "
                          + "device is connected to Internet and restart "
                          + "the application.");
        }
    }
}
