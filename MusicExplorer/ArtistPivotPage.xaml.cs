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
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using MusicExplorer.Models;
using System.Windows.Media;

namespace MusicExplorer
{
    /// <summary>
    /// Page for artist specific information.
    /// </summary>
    public partial class ArtistPivotPage : PhoneApplicationPage
    {
        // Members
        private ApplicationBarIconButton prevButton;
        private ApplicationBarIconButton playPauseButton;
        private ApplicationBarIconButton nextButton;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ArtistPivotPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            SystemTray.SetOpacity(this, 0.01);
            CreateAppBar();
        }

        /// <summary>
        /// Formats the page according to artist.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
 	        base.OnNavigatedTo(e);
            MediaPlayer.ActiveSongChanged += MediaPlayer_ActiveSongChanged;
            TitleText.Title = App.ViewModel.SelectedArtist.Name.ToUpper();

            if (App.ViewModel.IsLocalArtist(App.ViewModel.SelectedArtist.Name))
            {
                PlayLocalSongsButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Clears out now playing info.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NowPlayingText.Text = "";
            NowPlayingText.Visibility = Visibility.Collapsed;
            ApplicationBar.IsVisible = false;
        }

        /// <summary>
        /// Now playing text is changed when active song changes.
        /// </summary>
        /// <param name="sender">Media player</param>
        /// <param name="e">Event arguments</param>
        void MediaPlayer_ActiveSongChanged(object sender, EventArgs e)
        {
            try
            {
                if (NowPlayingText.Visibility == Visibility.Visible)
                {
                    NowPlayingText.Text = MediaPlayer.Queue.ActiveSong.Artist.Name
                                        + " - "
                                        + MediaPlayer.Queue.ActiveSong.Name;
                }
            }
            catch (Exception)
            {
                // It seems that when an artist mix is played, data in 
                // MediaPlayer becomes invalid -> Exception is thrown.
                // There's no need to react to it however.
            }
        }

        /// <summary>
        /// Launches Nokia Music Application to the selected product view.
        /// </summary>
        /// <param name="sender">LongListSelector - list of tracks</param>
        /// <param name="e">Event arguments</param>
        void OnTrackSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductModel selected = (ProductModel)TracksList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchProduct(selected.Id);
                TracksList.SelectedItem = null;
            }
        }

        /// <summary>
        /// Launches Nokia Music Application to the selected product view.
        /// </summary>
        /// <param name="sender">LongListSelector - list of albums</param>
        /// <param name="e">Event arguments</param>
        void OnAlbumSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductModel selected = (ProductModel)AlbumsList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchProduct(selected.Id);
                AlbumsList.SelectedItem = null;
            }
        }

        /// <summary>
        /// Launches Nokia Music Application to the selected product view.
        /// </summary>
        /// <param name="sender">LongListSelector - list of singles</param>
        /// <param name="e">Event arguments</param>
        void OnSingleSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductModel selected = (ProductModel)SinglesList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchProduct(selected.Id);
                SinglesList.SelectedItem = null;
            }
        }

        /// <summary>
        /// Launches Nokia Music Application to the selected artist view.
        /// </summary>
        /// <param name="sender">LongListSelector - list of similar artists</param>
        /// <param name="e">Event arguments</param>
        void OnSimilarArtistsSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ArtistModel selected = (ArtistModel)SimilarList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchArtist(selected.Id);
                SimilarList.SelectedItem = null;
            }
        }

        /// <summary>
        /// Shuffles local artist's songs and starts playback.
        /// Playback controls are shown along with Now Playing information.
        /// </summary>
        /// <param name="sender">Shuffle and play local tracks button</param>
        /// <param name="e">Event arguments</param>
        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            ShuffleAndPlayLocalArtist(App.ViewModel.SelectedArtist.Name);
            ApplicationBar.IsVisible = true;
            DetailsPivotItem.Margin = new Thickness(12, 28, 12, 70);
            TracksPivotItem.Margin = new Thickness(12, 28, 12, 70);
            AlbumsPivotItem.Margin = new Thickness(12, 28, 12, 70);
            SinglesPivotItem.Margin = new Thickness(12, 28, 12, 70);
            SimilarPivotItem.Margin = new Thickness(12, 28, 12, 70);
            NowPlayingText.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Launches Nokia Music Application to the artist view.
        /// </summary>
        /// <param name="sender">Show artist in Nokia Music button</param>
        /// <param name="e">Event arguments</param>
        private void OnShowArtistClick(object sender, RoutedEventArgs e)
        {
            App.MusicApi.LaunchArtist(App.ViewModel.SelectedArtist.Id);
        }

        /// <summary>
        /// Stops current playback.
        /// Launches Nokia Music Application to play artist mix.
        /// </summary>
        /// <param name="sender">Play artist mix in Nokia Music button</param>
        /// <param name="e">Event arguments</param>
        private void OnPlayMixClick(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            playPauseButton.IconUri = new Uri("/Assets/transport.play.png", UriKind.Relative);
            playPauseButton.Text = "Play";
            App.MusicApi.LaunchArtistMix(App.ViewModel.SelectedArtist.Name);
        }

        /// <summary>
        /// Starts playing previous track.
        /// </summary>
        /// <param name="sender">prev button</param>
        /// <param name="e">Event arguments</param>
        private void Prev_Click(object sender, EventArgs e)
        {
            MediaPlayer.MovePrevious();
        }

        /// <summary>
        /// Pauses / Resumes playback of current track.
        /// </summary>
        /// <param name="sender">play/pause button</param>
        /// <param name="e">Event arguments</param>
        private void PlayPause_Click(object sender, EventArgs e)
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Pause();
                playPauseButton.IconUri = new Uri("/Assets/transport.play.png", UriKind.Relative);
                playPauseButton.Text = "Play";
            }
            else
            {
                MediaPlayer.Resume();
                playPauseButton.IconUri = new Uri("/Assets/transport.pause.png", UriKind.Relative);
                playPauseButton.Text = "Pause";
            }
        }

        /// <summary>
        /// Starts playing next track.
        /// </summary>
        /// <param name="sender">next button</param>
        /// <param name="e">Event arguments</param>
        private void Next_Click(object sender, EventArgs e)
        {
            MediaPlayer.MoveNext();
        }

        /// <summary>
        /// Shuffles the songs of a local artist and starts playback
        /// </summary>
        /// <param name="localArtistName">Name of the artist</param>
        public void ShuffleAndPlayLocalArtist(string localArtistName)
        {
            Microsoft.Xna.Framework.Media.MediaLibrary lib =
                new Microsoft.Xna.Framework.Media.MediaLibrary();

            for (int i = 0; i < lib.Artists.Count; i++)
            {
                if (localArtistName == lib.Artists[i].Name)
                {
                    // generate a random track index
                    Random rand = new Random();
                    int track = rand.Next(0, lib.Artists[i].Songs.Count);

                    Microsoft.Xna.Framework.Media.SongCollection songCollection = lib.Artists[i].Songs;
                    Microsoft.Xna.Framework.Media.MediaPlayer.Play(songCollection, track);
                    Microsoft.Xna.Framework.Media.MediaPlayer.IsShuffled = true;
                    Microsoft.Xna.Framework.FrameworkDispatcher.Update();
                    break;
                }
            }
        }

        /// <summary>
        /// App bar needs to be created in c# code because of a need
        /// to dynamically change text and icons in the buttons.
        /// </summary>
        private void CreateAppBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 0.99;
            ApplicationBar.IsVisible = false;
            ApplicationBar.IsMenuEnabled = false;
            ApplicationBar.BackgroundColor = Color.FromArgb(255, 20, 20, 20);
            ApplicationBar.ForegroundColor = Color.FromArgb(255, 255, 255, 255);

            prevButton = new ApplicationBarIconButton();
            prevButton.IconUri = new Uri("/Assets/transport.rew.png", UriKind.Relative);
            prevButton.Text = "prev";
            ApplicationBar.Buttons.Add(prevButton);
            prevButton.Click += new EventHandler(Prev_Click);

            playPauseButton = new ApplicationBarIconButton();
            playPauseButton.IconUri = new Uri("/Assets/transport.pause.png", UriKind.Relative);
            playPauseButton.Text = "pause";
            ApplicationBar.Buttons.Add(playPauseButton);
            playPauseButton.Click += new EventHandler(PlayPause_Click);

            nextButton = new ApplicationBarIconButton();
            nextButton.IconUri = new Uri("/Assets/transport.ff.png", UriKind.Relative);
            nextButton.Text = "next";
            ApplicationBar.Buttons.Add(nextButton);
            nextButton.Click += new EventHandler(Next_Click);
        }
    }
}