﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Abp.Dependency;
using Abp.Localization;
using MatoMusic.Common;
using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Models;
using MatoMusic.Infrastructure.Common;
using MatoMusic.Services;
using MatoMusic.ViewModels;
using Microsoft.Maui.Controls;


namespace MatoMusic
{

    public partial class AlbumPage : ContentPageBase, ITransientDependency
    {
        private readonly MusicFunctionManager musicFunctionManager;


        public AlbumPage(LibraryPageViewModel libraryPageViewModel,
             MusicFunctionManager musicFunctionManager)
        {
            this.musicFunctionManager = musicFunctionManager;
            InitializeComponent();
            this.BindingContext = libraryPageViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await (this.BindingContext as LibraryPageViewModel).Init();
        }


        private async void ListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is ArtistInfo)
            {
                var artistInfo = e.SelectedItem as ArtistInfo;
                await navigationService.PushAsync("MusicCollectionPage", new object[] { artistInfo });

            }
            else if (e.SelectedItem is AlbumInfo)
            {
                var albumInfo = e.SelectedItem as AlbumInfo;
               await navigationService.PushAsync("MusicCollectionPage", new object[] { albumInfo });
            }
                (sender as ListView).SelectedItem = null;
        }

        private void MusicListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var musicInfo = e.SelectedItem as MusicInfo;
            if (musicInfo != null)
            {
                (this.BindingContext as LibraryPageViewModel).ChangeMusic(musicInfo);
            }
        }


        private void _musicFunctionPage_OnFinished(object sender, MusicFunctionEventArgs e)
        {
            musicFunctionManager.OnMusicFunctionFinished(e);
        }

        private async void AlbumMoreButton_OnClicked(object sender, EventArgs e)
        {

            var _mainMenuCellInfos = new List<MenuCellInfo>()
            {

                new MenuCellInfo() {Title = string.Format("{0}{1}",L("PlayThis"),L("Albums")), Code = "Play", Icon = ""},
                new MenuCellInfo() {Title = L("AddToQueue2"), Code = "AddMusicCollectionToQueue", Icon = ""},
                new MenuCellInfo() {Title = L("AddTo"), Code = "AddMusicCollectionToPlaylist", Icon = ""},
                new MenuCellInfo() {Title = L("AddToFavourite"), Code = "AddToFavourite", Icon = ""}

            };
            var musicInfo = (sender as BindableObject).BindingContext;

            var _musicFunctionPage = new MusicFunctionPage(musicInfo as IBasicInfo, _mainMenuCellInfos);
            _musicFunctionPage.OnFinished += _musicFunctionPage_OnFinished;

            await navigationService.ShowPopupAsync(_musicFunctionPage);

        }


        private async void SearchButton_OnClicked(object sender, EventArgs e)
        {
           //await navigationService.GoPageAsync("SearchPage");
        }

    }

}

