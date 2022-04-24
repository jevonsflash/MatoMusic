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

namespace MatoMusic.ViewModel
{
    public class PlaylistEntryPageViewModel : MusicCollectionPageViewModel
    {


        public PlaylistEntryPageViewModel(PlaylistInfo playlist) : base(playlist)
        {
            MusicsCollectionInfo = playlist;
            (MusicsCollectionInfo.Musics as ObservableCollection<MusicInfo>).CollectionChanged += Musics_CollectionChanged;
            this.PropertyChanged += PlaylistEntryPageViewModel_PropertyChanged;

        }
        public PlaylistEntryPageViewModel(PlaylistInfo playlist,List<MenuCellInfo> menus) : base(playlist,menus)
        {
            MusicsCollectionInfo = playlist;
            (MusicsCollectionInfo.Musics as ObservableCollection<MusicInfo>).CollectionChanged += Musics_CollectionChanged;
            this.PropertyChanged += PlaylistEntryPageViewModel_PropertyChanged;

        }

        private void PlaylistEntryPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MusicsCollectionInfo))
            {
                Core.MusicInfoManager.UpdatePlaylist(this.MusicsCollectionInfo as PlaylistInfo);

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
    }
}
