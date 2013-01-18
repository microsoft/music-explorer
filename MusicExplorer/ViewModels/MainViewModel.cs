/*
 * Copyright © 2012 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicExplorer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.LocalAudio = new ObservableCollection<ArtistViewModel>();
            this.Recommendations = new ObservableCollection<ArtistViewModel>();
            this.NewReleases = new ObservableCollection<ProductViewModel>();
            this.TopArtists = new ObservableCollection<ArtistViewModel>();
            this.TracksForArtist = new ObservableCollection<ProductViewModel>();
            this.AlbumsForArtist = new ObservableCollection<ProductViewModel>();
            this.SinglesForArtist = new ObservableCollection<ProductViewModel>();
            this.SimilarForArtist = new ObservableCollection<ArtistViewModel>();
            this.Genres = new ObservableCollection<GenreViewModel>();
            this.TopArtistsForGenre = new ObservableCollection<ArtistViewModel>();
            this.MixGroups = new ObservableCollection<MixGroupViewModel>();
            this.Mixes = new ObservableCollection<MixViewModel>();
            this._selectedArtist = null;
            this._selectedGenre = "";

            // Insert a place holder for title text
            this.LocalAudio.Add(new ArtistViewModel() { Name = "MusicExplorerTitlePlaceholder", ProportionalHeight = "100", ProportionalWidth = "400" });
        }

        /// <summary>
        /// A collection for ArtistViewModel objects.
        /// </summary>
        public ObservableCollection<ArtistViewModel> LocalAudio { get; private set; }

        /// <summary>
        /// A collection for ArtistViewModel objects.
        /// </summary>
        public ObservableCollection<ArtistViewModel> Recommendations { get; private set; }

        /// <summary>
        /// A collection for ArtistViewModel objects.
        /// </summary>
        public ObservableCollection<ArtistViewModel> TopArtists { get; private set; }

        /// <summary>
        /// A collection for ProductViewModel objects.
        /// </summary>
        public ObservableCollection<ProductViewModel> TracksForArtist { get; private set; }

        /// <summary>
        /// A collection for ProductViewModel objects.
        /// </summary>
        public ObservableCollection<ProductViewModel> AlbumsForArtist { get; private set; }

        /// <summary>
        /// A collection for ProductViewModel objects.
        /// </summary>
        public ObservableCollection<ProductViewModel> SinglesForArtist { get; private set; }

        /// <summary>
        /// A collection for ArtistViewModel objects.
        /// </summary>
        public ObservableCollection<ArtistViewModel> SimilarForArtist { get; private set; }

        /// <summary>
        /// A collection for ProductViewModel objects.
        /// </summary>
        public ObservableCollection<ProductViewModel> NewReleases { get; private set; }

        /// <summary>
        /// A collection for GenreViewModel objects.
        /// </summary>
        public ObservableCollection<GenreViewModel> Genres { get; private set; }

        /// <summary>
        /// A collection for ArtistViewModel objects.
        /// </summary>
        public ObservableCollection<ArtistViewModel> TopArtistsForGenre { get; private set; }

        /// <summary>
        /// A collection for MixGroupViewModel objects.
        /// </summary>
        public ObservableCollection<MixGroupViewModel> MixGroups { get; private set; }

        /// <summary>
        /// A collection for MixViewModel objects.
        /// </summary>
        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        private ArtistViewModel _selectedArtist;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public ArtistViewModel SelectedArtist
        {
            get
            {
                return _selectedArtist;
            }
            set
            {
                if (value != _selectedArtist)
                {
                    _selectedArtist = value;
                    NotifyPropertyChanged("SelectedArtist");
                }
            }
        }

        private string _selectedGenre;
        /// <summary>
        /// MainViewModel's SelectedGenre property; this property is used in the TopArtistsForGenre page to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string SelectedGenre
        {
            get
            {
                return _selectedGenre;
            }
            set
            {
                if (value != _selectedGenre)
                {
                    _selectedGenre = value;
                    NotifyPropertyChanged("SelectedGenre");
                }
            }
        }

        /// <summary>
        /// MainViewModel's IsDataLoaded property;
        /// </summary>
        /// <returns></returns>
        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Loads local audio information and creates a ViewModel for local artists to be shown in Favourites view.
        /// </summary>
        public void LoadData()
        {
            MediaLibrary lib = new MediaLibrary();
            int totalTrackCount = 0;
            int totalArtistCount = 0;

            foreach (Artist a in lib.Artists)// Song s in lib.Songs)
            {
                if (a.Songs.Count == 0) continue; // Unknown artist with 0 tracks encountered
                string artist = a.Name;
                int trackCount = a.Songs.Count;
                int playCount = 0;

                // check the play count of artist's tracks
                foreach (Song s in a.Songs)
                {
                    playCount += s.PlayCount;
                }

                // search correct index for artist based on artist's play count
                bool artistAdded = false;
                for (int i = 1; i < LocalAudio.Count; i++)
                {
                    if (Convert.ToInt16(LocalAudio[i].PlayCount) < playCount)
                    {
                        this.LocalAudio.Insert(i, new ArtistViewModel() { Name = artist, TrackCount = Convert.ToString(trackCount), PlayCount = Convert.ToString(playCount) });
                        artistAdded = true;
                        break;
                    }
                }
                if (artistAdded == false)
                {
                    this.LocalAudio.Add(new ArtistViewModel() { Name = artist, TrackCount = Convert.ToString(trackCount), PlayCount = Convert.ToString(playCount) });
                }

                totalTrackCount += trackCount;
                totalArtistCount++;
            }

            // Continue with only the top 20 favourite artists
            int removeIndex = App.ViewModel.LocalAudio.Count - 1;
            while (removeIndex > 20)
            {
                App.ViewModel.LocalAudio.RemoveAt(removeIndex);
                removeIndex--;
            }

            foreach (ArtistViewModel m in App.ViewModel.LocalAudio)
            {
                // Divide local artists into two "size categories"
                if (m.Name == "MusicExplorerTitlePlaceholder") continue;
                int artistsWithMoreTracks = 0;
                int trackCount = Convert.ToInt16(m.TrackCount);
                for (int i = 0; i < App.ViewModel.LocalAudio.Count; i++)
                {
                    if (Convert.ToInt16(App.ViewModel.LocalAudio[i].TrackCount) > trackCount)
                        artistsWithMoreTracks++;
                }
                double artistRelation = (double)artistsWithMoreTracks / (double)totalArtistCount;

                if (artistRelation < 0.5)
                {
                    m.ProportionalHeight = "200";
                    m.ProportionalWidth = "206";
                }
                else
                {
                    m.ProportionalHeight = "100";
                    m.ProportionalWidth = "206";
                }
            }

            this.IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
