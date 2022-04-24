using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using MatoMusic.Common;
using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Models;
using MatoMusic.Core.ViewModel;
using MatoMusic.Infrastructure.Common;
using MatoMusic.Services;
using MatoMusic.ViewModel;


namespace MatoMusic.View
{

    public partial class MusicCollectionPage : ContentPageBase, ITransientDependency
    {
        public MusicCollectionPage(MusicCollectionInfo info)
        {
            InitializeComponent();
            this.BindingContext = new MusicCollectionPageViewModel(info);

        }


        public MusicFunctionManager MusicFunctionManager { get; set; }

        private NavigationPage detailPage;

        private PlaylistChoosePage _playlistChoosePage;


        private void MusicListView_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var musicInfo = e.SelectedItem as MusicInfo;
            if (musicInfo != null)
            {
                MusicInfoManager.InsertToEndQueueEntry(musicInfo);
                MusicRelatedService.CurrentMusic = musicInfo;
                (sender as ListView).SelectedItem = null;

            }
        }
        private async void MusicMoreButton_OnClicked(object sender, EventArgs e)
        {
            var musicInfo = (sender as BindableObject).BindingContext;
            var _mainMenuCellInfos = new List<MenuCellInfo>()
            {
                new MenuCellInfo() {Title = L("AddTo"), Code = "AddToPlaylist", Icon = "addto"},
                new MenuCellInfo() {Title = L("PlayNext"), Code = "NextPlay", Icon = "playnext"},
                new MenuCellInfo() {Title = L("AddToQueue2"), Code = "AddToQueue", Icon = "addtostack"},
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


        private async void Button_OnClicked(object sender, EventArgs e)
        {

            var context = this.BindingContext as MusicCollectionPageViewModel;
            var MenuCellInfo = (sender as BindableObject).BindingContext as MenuCellInfo;
            if (MenuCellInfo.Code == "Play")
            {
                if (context.MusicsCollectionInfo.Count != 0)
                {
                    await MusicInfoManager.ClearQueue();
                    var result = await MusicInfoManager.CreateQueueEntrys(context.MusicsCollectionInfo);
                    if (result)
                    {
                        var CurrentMusic = await MusicInfoManager.GetQueueEntry();
                        MusicRelatedService.CurrentMusic = CurrentMusic[0];
                        CommonHelper.ShowMsg("成功添加并播放");

                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AlreadyExists"));
                    }
                }

            }
            else if (MenuCellInfo.Code == "AddMusicCollectionToPlaylist")
            {

                _playlistChoosePage = new PlaylistChoosePage();
                _playlistChoosePage.OnFinished += async (o, c) =>
                {
                    if (c != null)
                    {
                        var result = await MusicInfoManager.CreatePlaylistEntrys(context.MusicsCollectionInfo as MusicCollectionInfo, c.PlaylistId);
                        if (result)
                        {
                            CommonHelper.ShowMsg(string.Format("{0},{1}", L("Msg_HasAdded"), c.Title));
                        }
                        else
                        {
                            CommonHelper.ShowMsg(L("Msg_AddFaild"));

                        }


                    }
                    await navigationService.HidePopupAsync(_playlistChoosePage);
                };
                await navigationService.ShowPopupAsync(_playlistChoosePage);

            }
            else if (MenuCellInfo.Code == "AddToFavourite")
            {
                var musicCollectionInfo = context.MusicsCollectionInfo as MusicCollectionInfo;
                if (musicCollectionInfo != null && musicCollectionInfo.Count != 0)
                {
                    var result = await MusicInfoManager.CreatePlaylistEntrysToMyFavourite(musicCollectionInfo);
                    if (result)
                    {
                        CommonHelper.ShowMsg(string.Format("{0}“我最喜爱”", L("Msg_HasAdded")));
                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AddFaild"));

                    }
                }
                else
                {
                    CommonHelper.ShowMsg("没有曲目");

                }
            }
            else if (MenuCellInfo.Code == "AddMusicCollectionToQueue")
            {
                var musicCollectionInfo = context.MusicsCollectionInfo as MusicCollectionInfo;
                if (musicCollectionInfo != null && musicCollectionInfo.Count != 0)
                {
                    var result = await MusicInfoManager.InsertToEndQueueEntrys(musicCollectionInfo.Musics
                        .ToList());
                    if (result)
                    {
                        CommonHelper.ShowMsg(L("Msg_HasAddedQueue2"));
                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AlreadyExists"));
                    }

                }
                else
                {
                    CommonHelper.ShowMsg("没有曲目");

                }
            }
        }

    }
}