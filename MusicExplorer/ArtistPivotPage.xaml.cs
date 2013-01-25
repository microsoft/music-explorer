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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using MusicExplorer.ViewModels;
using Microsoft.Xna.Framework.Media;

namespace MusicExplorer
{
    public partial class ArtistPivotPage : PhoneApplicationPage
    {
        private ApplicationBarIconButton PrevButton;
        private ApplicationBarIconButton PlayPauseButton;
        private ApplicationBarIconButton NextButton;

        public ArtistPivotPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            SystemTray.SetOpacity(this, 0.01);

            CreateAppBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
 	        base.OnNavigatedTo(e);
            MediaPlayer.ActiveSongChanged += MediaPlayer_ActiveSongChanged;
            TitleText.Title = App.ViewModel.SelectedArtist.Name.ToUpper();

            if (App.ViewModel.IsLocalArtist(App.ViewModel.SelectedArtist.Name))
            {
                PlayLocalSongsButton.Visibility = Visibility.Visible;
            }

            if (MediaPlayer.State == MediaState.Playing)
            {
                NowPlayingText.Text = MediaPlayer.Queue.ActiveSong.Artist + " - " + MediaPlayer.Queue.ActiveSong.Name;
                PlayPauseButton.IconUri = new Uri("/Assets/transport.pause.png", UriKind.Relative);
                PlayPauseButton.Text = "Pause";
                ApplicationBar.IsVisible = true;
            }
        }

        void MediaPlayer_ActiveSongChanged(object sender, EventArgs e)
        {
            NowPlayingText.Text = MediaPlayer.Queue.ActiveSong.Artist + " - " + MediaPlayer.Queue.ActiveSong.Name;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NowPlayingText.Text = "";
        }


        void OnTrackSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductViewModel selected = (ProductViewModel)TracksList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchProduct(selected.Id);
                TracksList.SelectedItem = null;
            }
        }

        void OnAlbumSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductViewModel selected = (ProductViewModel)AlbumsList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchProduct(selected.Id);
                AlbumsList.SelectedItem = null;
            }
        }

        void OnSingleSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ProductViewModel selected = (ProductViewModel)SinglesList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchProduct(selected.Id);
                SinglesList.SelectedItem = null;
            }
        }

        void OnSimilarArtistsSelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ArtistViewModel selected = (ArtistViewModel)SimilarList.SelectedItem;
            if (selected != null)
            {
                App.MusicApi.LaunchArtist(selected.Id);
                SimilarList.SelectedItem = null;
            }
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            App.MusicApi.PlayLocalArtist(App.ViewModel.SelectedArtist.Name);
            ApplicationBar.IsVisible = true;
            NowPlayingText.Visibility = Visibility.Visible;
        }

        private void OnShowArtistClick(object sender, RoutedEventArgs e)
        {
            App.MusicApi.LaunchArtist(App.ViewModel.SelectedArtist.Id);
        }

        private void OnPlayMixClick(object sender, RoutedEventArgs e)
        {
            App.MusicApi.LaunchArtistMix(App.ViewModel.SelectedArtist.Name);
        }

        /// <summary>
        /// Event handler for clicking prev button.
        /// </summary>
        private void Prev_Click(object sender, EventArgs e)
        {
            MediaPlayer.MovePrevious();
        }

        /// <summary>
        /// Event handler for clicking play/pause button.
        /// </summary>
        private void PlayPause_Click(object sender, EventArgs e)
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Pause();
                PlayPauseButton.IconUri = new Uri("/Assets/transport.play.png", UriKind.Relative);
                PlayPauseButton.Text = "Play";
            }
            else
            {
                MediaPlayer.Resume();
                PlayPauseButton.IconUri = new Uri("/Assets/transport.pause.png", UriKind.Relative);
                PlayPauseButton.Text = "Pause";
            }
        }

        /// <summary>
        /// Event handler for clicking next button.
        /// </summary>
        private void Next_Click(object sender, EventArgs e)
        {
            MediaPlayer.MoveNext();
        }

        private void CreateAppBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 1.0;
            ApplicationBar.IsVisible = false;
            ApplicationBar.IsMenuEnabled = false;

            PrevButton = new ApplicationBarIconButton();
            PrevButton.IconUri = new Uri("/Assets/transport.rew.png", UriKind.Relative);
            PrevButton.Text = "prev";
            ApplicationBar.Buttons.Add(PrevButton);
            PrevButton.Click += new EventHandler(Prev_Click);

            PlayPauseButton = new ApplicationBarIconButton();
            PlayPauseButton.IconUri = new Uri("/Assets/transport.pause.png", UriKind.Relative);
            PlayPauseButton.Text = "pause";
            ApplicationBar.Buttons.Add(PlayPauseButton);
            PlayPauseButton.Click += new EventHandler(PlayPause_Click);

            NextButton = new ApplicationBarIconButton();
            NextButton.IconUri = new Uri("/Assets/transport.ff.png", UriKind.Relative);
            NextButton.Text = "next";
            ApplicationBar.Buttons.Add(NextButton);
            NextButton.Click += new EventHandler(Next_Click);
        }
    }
}