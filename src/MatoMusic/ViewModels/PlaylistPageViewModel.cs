using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Abp.Dependency;
using Microsoft.Maui.Controls;
using MatoMusic.Core.Models;
using MatoMusic.Core.ViewModel;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Services;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Models.Entities;

namespace MatoMusic.ViewModels
{
    public class PlaylistPageViewModel : MusicRelatedViewModel
    {
        public PlaylistPageViewModel()
        {
            DeleteCommand = new Command(DeleteAction, c => true);
        }

        private async void Playlists_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var item = e.OldItems[0] as PlaylistInfo;

                if (await MusicInfoManager.DeletePlaylist(item.Id))
                {
                    CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_HasRemoved"), item.Title));

                }
                else
                {
                    CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_RemoveFailed"), item.Title));

                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var item = ObjectMapper.Map<Playlist>(e.NewItems[0] as PlaylistInfo);

                if (await MusicInfoManager.CreatePlaylist(item))
                {

                    CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_HasCreated"), item.Title));

                }
                else
                {
                    CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_AddFailed"), item.Title));

                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                var item = ObjectMapper.Map<Playlist>(e.NewItems[0] as PlaylistInfo);
                if (await MusicInfoManager.UpdatePlaylist(item))
                {

                    CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_HasEdit"), item.Title));

                }
                else
                {
                    CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_EditFailed"), item.Title));

                }

            }
        }


        public void CreateAction(object obj)
        {
            var playlistInfo = obj as PlaylistInfo;
            PlaylistHandler(playlistInfo, (e) =>
            {
                Playlists.Add(e);
            });
        }

        private async void PlaylistHandler(PlaylistInfo playlistInfo, Action<PlaylistInfo> handlerPlaylist)
        {
            if (playlistInfo != null && handlerPlaylist != null && playlistInfo.Title != "我最喜爱" && !string.IsNullOrEmpty(playlistInfo.Title))
            {
                var restul = await MusicInfoManager.GetPlaylist();
                if (!restul.Any(c => c.Title == playlistInfo.Title))
                {
                    handlerPlaylist.Invoke(playlistInfo);
                }
                else
                {
                    CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_AlreadyExists"), playlistInfo.Title));
                }
            }
            else
            {
                CommonHelper.ShowMsg(string.Format(L("Msg_Nameillegal")));

            }
        }

        public void EditAction(object obj)
        {
            var playlistInfo = obj as PlaylistInfo;

            PlaylistHandler(playlistInfo, (e) =>
            {
                var oldOne = Playlists.FirstOrDefault(c => c.Id == playlistInfo.Id);
                Playlists[Playlists.IndexOf(oldOne)] = playlistInfo;
            });
        }
        public void DeleteAction(object obj)
        {
            var playlistInfo = obj as PlaylistInfo;
            if (playlistInfo != null && playlistInfo.Title != "我最喜爱" && playlistInfo.Id != 0)
            {
                Playlists.Remove(playlistInfo);
            }
            else
            {
                CommonHelper.ShowMsg(string.Format("{0} {1}", L("Msg_RemoveFailed"), playlistInfo?.Title));

            }
        }

        private ObservableCollection<PlaylistInfo> _playlists;

        public ObservableCollection<PlaylistInfo> Playlists
        {
            get
            {
                if (_playlists == null)
                {

                    _playlists = new ObservableCollection<PlaylistInfo>();
                }
                return _playlists;

            }
            set
            {
                _playlists = value;

                RaisePropertyChanged();
            }
        }

        public async Task Init()
        {
            Playlists = new ObservableCollection<PlaylistInfo>(await InitPlaylist());
            Playlists.CollectionChanged += Playlists_CollectionChanged;
        }

        private async Task<List<PlaylistInfo>> InitPlaylist()
        {
            var restul = ObjectMapper.Map<List<PlaylistInfo>>(await MusicInfoManager.GetPlaylist());
            if (restul.Count == 0 || !restul.Any(c => c.Title == "我最喜爱"))
            {
                await MusicInfoManager.CreatePlaylist(new Playlist() { Id = 0, Title = "我最喜爱", IsHidden = false, IsRemovable = false });
                restul = ObjectMapper.Map<List<PlaylistInfo>>(await MusicInfoManager.GetPlaylist());
            }
            return restul;
        }




        public Command DeleteCommand { get; set; }

    }
}
