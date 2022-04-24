using Abp.Dependency;
using MatoMusic;
using MatoMusic.Common;
using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Models;
using MatoMusic.Infrastructure.Common;
using MatoMusic.Services;
using ProjectMato.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MatoMusic.View
{


    public partial class SearchPage : ContentPageBase, ITransientDependency
    {
        private NavigationPage detailPage;

        public MusicFunctionManager MusicFunctionManager { get; set; }
        public SearchPage(SearchPageViewModel searchPageViewModel)
        {
            InitializeComponent();
            this.BindingContext = searchPageViewModel;

        }

        private async void AutoCompleteView_OnSelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            var musicInfo = e.SelectedItem as MusicInfo;
            if (musicInfo != null)
            {
                await MusicInfoManager.InsertToEndQueueEntry(musicInfo);
                MusicRelatedService.CurrentMusic = musicInfo;
                await navigationService.GoPageAsync("NowPlayingPage");
            }

        }


        private async void MoreButton_OnClicked(object sender, EventArgs e)
        {
            var musicInfo = (sender as BindableObject).BindingContext;
            var _mainMenuCellInfos = new List<MenuCellInfo>()
            {
                new MenuCellInfo() {Title = L("AddTo"), Code = "AddToPlaylist", Icon = "\uf083"},
                new MenuCellInfo() {Title = L("PlayNext"), Code = "NextPlay", Icon = "\uf083"},
                new MenuCellInfo() {Title = L("AddToQueue2"), Code = "AddToQueue", Icon = "\uf083"},
                new MenuCellInfo()
                {
                    Title = (musicInfo as MusicInfo).Artist,
                    Code = "GoArtistPage",
                    Icon = "microphone2"
                },
                new MenuCellInfo()
                {
                    Title = (musicInfo as MusicInfo).AlbumTitle,
                    Code = "GoAlbumPage",
                    Icon = "cd2"
                },


            };
            var _musicFunctionPage = new MusicFunctionPage(musicInfo as IBasicInfo, _mainMenuCellInfos);
            _musicFunctionPage.OnFinished += _musicFunctionPage_OnFinished;

            await navigationService.ShowPopupAsync(_musicFunctionPage);


        }

        private void _musicFunctionPage_OnFinished(object sender, MusicFunctionEventArgs e)
        {
            MusicFunctionManager.OnMusicFunctionFinished(e);
        }

    }
}
