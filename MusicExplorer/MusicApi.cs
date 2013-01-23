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

namespace MusicExplorer
{
    /// <summary>
    /// 
    /// </summary>
    public class MusicApi
    {
        // Members
        private MusicClient client =  null;
        private Collection<Artist> localArtists = new Collection<Artist>();
        private bool initialized = false;
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
            client = new MusicClient("v0Bh5kUAzCDp7PVS4kKr", "OfItN9r8E5QHfNO5mw-rEg", countryCode);
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
                        m.Country = response.Result[0].Country;
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

                    localAudioResponses++;
                    GetArtistInfoForLocalAudio();

                    if (localAudioResponses == App.ViewModel.LocalAudio.Count)
                    {
                        // Request recommendations after receiving info for all local artists
                        FetchRecommendations();
                    }
                });
            }, m.Name);
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

                            // don't recommend artists already stored in device
                            foreach (ArtistViewModel localArtist in App.ViewModel.LocalAudio)
                            {
                                if (localArtist.Name == a.Name)
                                {
                                    handled = true;
                                }

                                break;
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
                                            Country = a.Country,
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
                                            Country = a.Country,
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

                    recommendResponses++;
                    FetchRecommendations();
                });
            }, localArtists[recommendResponses].Id);
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
                                        Country = a.Country,
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
                                        Country = a.Country,
                                        Genres = a.Genres[0].Name,
                                        ThumbUri = new Uri("/Assets/thumb_100_placeholder.png",
                                                           UriKind.Relative),
                                        Id = a.Id
                                    });
                            }
                        }
                    }
                });
            });
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
                });
            }, Category.Album);
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
                            App.ViewModel.Genres.Add(new GenreViewModel()
                                {
                                    Name = g.Name,
                                    Id = g.Id
                                });
                        }
                    }
                });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
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
                                        Country = a.Country,
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
                                        Country = a.Country,
                                        Genres = a.Genres[0].Name,
                                        ThumbUri = new Uri("/Assets/thumb_100_placeholder.png",
                                                           UriKind.Relative),
                                        Id = a.Id
                                    });
                            }
                        }
                    }
                });
            }, id);
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
                });
            });
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
                });
            }, id);
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

            client.GetArtistProducts((ListResponse<Product> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        App.ViewModel.TracksForArtist.Clear();
                        App.ViewModel.AlbumsForArtist.Clear();
                        App.ViewModel.SinglesForArtist.Clear();

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
                    }
                });
            }, id);
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

            client.GetSimilarArtists((ListResponse<Artist> response) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Use results
                    if (response != null && response.Result != null && response.Result.Count > 0)
                    {
                        App.ViewModel.SimilarForArtist.Clear();

                        foreach (Artist a in response.Result)
                        {
                            if (a.Thumb100Uri != null)
                            {
                                App.ViewModel.SimilarForArtist.Add(new ArtistViewModel()
                                    {
                                        Name = a.Name,
                                        Country = a.Country,
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
                                        Country = a.Country,
                                        Genres = a.Genres[0].Name,
                                        ThumbUri = new Uri("/Assets/thumb_100_placeholder.png",
                                                           UriKind.Relative),
                                        Id = a.Id
                                    });
                            }
                        }
                    }
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
                    Microsoft.Xna.Framework.Media.SongCollection songCollection = lib.Artists[i].Songs;
                    Microsoft.Xna.Framework.Media.MediaPlayer.Play(songCollection);
                    Microsoft.Xna.Framework.FrameworkDispatcher.Update();
                    break;
                }
            }
        }
    }
}
