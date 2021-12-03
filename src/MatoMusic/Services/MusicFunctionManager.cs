using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Dependency;
using Abp.Domain.Services;
using MatoMusic.Common;
using MatoMusic.Core;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Models;
using MatoMusic.Core.Services;
using MatoMusic.Core.ViewModel;
using Microsoft.Maui.Controls;

namespace MatoMusic.Services
{
    public class MusicFunctionManager : AbpServiceBase, ISingletonDependency
    {
        private PlaylistChoosePage _playlistChoosePage;
        private readonly NavigationService navigationService;
        private readonly IMusicInfoManager musicInfoManager;
        private readonly MusicRelatedService musicRelatedViewModel;


        public MusicFunctionManager(
            NavigationService navigationService,
            IMusicInfoManager musicInfoManager,
            MusicRelatedService musicRelatedViewModel
            )
        {
            this.navigationService = navigationService;
            this.musicInfoManager = musicInfoManager;
            this.musicRelatedViewModel = musicRelatedViewModel;
        }

        /// <summary>
        /// 完成菜单选项时调用
        /// </summary>
        /// <param name="musicFunctionEventArgs"></param>
        public async void OnMusicFunctionFinished(MusicFunctionEventArgs musicFunctionEventArgs)
        {
            if (musicFunctionEventArgs.MusicInfo == null)
            {
                return;
            }
            //this.Popup.HidePopup();
            await navigationService.PopToRootAsync();
            if (musicFunctionEventArgs.MenuCellInfo.Code == "AddToPlaylist")
            {
                _playlistChoosePage = new PlaylistChoosePage();
                _playlistChoosePage.OnFinished += async (o, c) =>
                {
                    if (c != null)
                    {
                        var result = await musicInfoManager.CreatePlaylistEntry((musicFunctionEventArgs.MusicInfo as MusicInfo), c.Id);
                        if (result)
                        {
                            CommonHelper.ShowMsg(string.Format("{0}{1}", L("Msg_HasAdded"), c.Title));
                        }
                        else
                        {
                            CommonHelper.ShowMsg(L("Msg_AddFaild"));
                        }

                    }
                    await navigationService.PopAsync();
                };
                await navigationService.PushAsync(_playlistChoosePage);
            }
            else if (musicFunctionEventArgs.MenuCellInfo.Code == "NextPlay")
            {
                var result = await musicInfoManager.InsertToNextQueueEntry(musicFunctionEventArgs.MusicInfo as MusicInfo, this.musicRelatedViewModel.CurrentMusic);
                if (result)
                {
                    CommonHelper.ShowMsg(string.Format("{0},{1}", L("PlayNext"), musicFunctionEventArgs.MusicInfo.Title));
                    await musicRelatedViewModel.RebuildMusicInfos();
                }
                else
                {
                    CommonHelper.ShowMsg(L("Msg_AddFaild"));
                }
            }
            else if (musicFunctionEventArgs.MenuCellInfo.Code == "AddToQueue")
            {
                var result = await musicInfoManager.InsertToEndQueueEntry(musicFunctionEventArgs.MusicInfo as MusicInfo);
                if (result)
                {
                    CommonHelper.ShowMsg(L("Msg_HasAddedQueue"));
                    await musicRelatedViewModel.RebuildMusicInfos();
                }
                else
                {
                    CommonHelper.ShowMsg(L("Msg_AddFaild"));
                }
            }

            else if (musicFunctionEventArgs.MenuCellInfo.Code == "GoAlbumPage")
            {
                List<AlbumInfo> list;
                var isSucc = await musicInfoManager.GetAlbumInfos();
                if (!isSucc.IsSucess)
                {
                    CommonHelper.ShowNoAuthorized();

                }
                list = isSucc.Result;
                var albumInfo = list.Find(c => c.Title == (musicFunctionEventArgs.MusicInfo as MusicInfo).AlbumTitle);
                navigationService.GoNavigate("MusicCollectionPage", new object[] { albumInfo });
            }
            else if (musicFunctionEventArgs.MenuCellInfo.Code == "GoArtistPage")
            {
                List<ArtistInfo> list;
                var isSucc = await musicInfoManager.GetArtistInfos();
                if (!isSucc.IsSucess)
                {
                    CommonHelper.ShowNoAuthorized();

                }
                list = isSucc.Result;
                var artistInfo = list.Find(c => c.Title == (musicFunctionEventArgs.MusicInfo as MusicInfo).Artist);
                navigationService.GoNavigate("MusicCollectionPage", new object[] { artistInfo });
            }
            else if (musicFunctionEventArgs.MenuCellInfo.Code == "AddMusicCollectionToPlaylist")
            {

                _playlistChoosePage = new PlaylistChoosePage();
                _playlistChoosePage.OnFinished += async (o, c) =>
                {
                    if (c != null)
                    {
                        var result = await musicInfoManager.CreatePlaylistEntrys(musicFunctionEventArgs.MusicInfo as MusicCollectionInfo, c.Id);
                        if (result)
                        {
                            CommonHelper.ShowMsg(string.Format("{0}{1}", L("Msg_HasAdded"), c.Title));
                        }
                        else
                        {
                            CommonHelper.ShowMsg(L("Msg_AddFaild"));
                        }
                    }
                    await navigationService.PopAsync();
                };

                await navigationService.PushAsync(_playlistChoosePage);

            }
            else if (musicFunctionEventArgs.MenuCellInfo.Code == "AddToFavourite")
            {
                var musicCollectionInfo = musicFunctionEventArgs.MusicInfo as MusicCollectionInfo;
                if (musicCollectionInfo != null)
                {
                    var result = await musicInfoManager.CreatePlaylistEntrysToMyFavourite(musicCollectionInfo);
                    if (result)
                    {
                        CommonHelper.ShowMsg(string.Format("{0}“我最喜爱”", L("Msg_HasAdded")));
                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AddFaild"));
                    }
                }
            }
            else if (musicFunctionEventArgs.MenuCellInfo.Code == "AddMusicCollectionToQueue")
            {
                var musicCollectionInfo = musicFunctionEventArgs.MusicInfo as MusicCollectionInfo;
                if (musicCollectionInfo != null)
                {
                    var result = await musicInfoManager.InsertToEndQueueEntrys(musicCollectionInfo.Musics
                        .ToList());

                    if (result)
                    {
                        await musicRelatedViewModel.RebuildMusicInfos();

                        CommonHelper.ShowMsg(L("Msg_HasAddedQueue2"));

                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AlreadyExists"));
                    }
                }
            }

            else if (musicFunctionEventArgs.MenuCellInfo.Code == "Play")
            {
                var musicCollectionInfo = musicFunctionEventArgs.MusicInfo as MusicCollectionInfo;
                if (musicCollectionInfo.Count != 0)
                {
                    await musicInfoManager.ClearQueue();
                    var result = await musicInfoManager.CreateQueueEntrys(musicCollectionInfo);
                    if (result)
                    {
                        await musicRelatedViewModel.RebuildMusicInfos();

                        var CurrentMusic = await musicInfoManager.GetQueueEntry();
                        musicRelatedViewModel.CurrentMusic = CurrentMusic[0];

                        CommonHelper.ShowMsg("成功添加并播放");

                    }
                    else
                    {
                        CommonHelper.ShowMsg(L("Msg_AlreadyExists"));
                    }
                }

            }

        }

        /// <summary>
        /// 判断菜单类型
        /// </summary>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public string GetCollectionType(object bindingContext)
        {
            var result = "";
            if (bindingContext is ArtistInfo)
            {
                result = L("Artists");
            }
            else if (bindingContext is AlbumInfo)
            {
                result = L("Albums");
            }
            else
            {
                result = L("Playlist");
            }
            return result;
        }

    }
}
