using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MatoMusic.Core;
using MatoMusic.Core.Models;
using MatoMusic.Core.Models.Entities;
using Abp.Dependency;
using MatoMusic.Core.Interfaces;

namespace MatoMusic.ViewModel
{
    public class PlaylistEntryPageViewModel : MusicCollectionPageViewModel, ITransientDependency
    {
        public PlaylistEntryPageViewModel(PlaylistInfo playlist) : base(playlist)
        {
            MusicsCollectionInfo = playlist;
            Init();

        }
        public PlaylistEntryPageViewModel(PlaylistInfo playlist, List<MenuCellInfo> menus) : base(playlist, menus)
        {
            MusicsCollectionInfo = playlist;
            Init();
        }
        public async void Init()
        {

            var musics = await MusicInfoManager.GetPlaylistEntry(MusicsCollectionInfo.Id);
            MusicsCollectionInfo.Musics=new ObservableCollection<MusicInfo>(musics);
            MusicsCollectionInfo.Musics.CollectionChanged += Musics_CollectionChanged;
            this.PropertyChanged += PlaylistEntryPageViewModel_PropertyChanged;
        }

        private async void PlaylistEntryPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MusicsCollectionInfo))
            {
                var model = this.MusicsCollectionInfo as PlaylistInfo;
                var entity = ObjectMapper.Map<Playlist>(model);
                await MusicInfoManager.UpdatePlaylist(entity);

            }
        }

        public void DeleteAction(object obj)
        {
            var musicInfo = obj as MusicInfo;
            this.MusicsCollectionInfo.Musics.Remove(musicInfo);
        }

        private void Musics_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                var oldIndex = e.OldStartingIndex;
                var newIndex = e.NewStartingIndex;
                MusicInfoManager.ReorderPlaylist(MusicsCollectionInfo.Musics[oldIndex], MusicsCollectionInfo.Musics[newIndex], MusicsCollectionInfo.Id);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                MusicInfoManager.DeletePlaylistEntry(e.OldItems[0] as MusicInfo, MusicsCollectionInfo.Id);
            }


        }

        public IMusicInfoManager MusicInfoManager => IocManager.Instance.Resolve<IMusicInfoManager>();

    }
}
