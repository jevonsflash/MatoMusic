﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using MatoMusic.Core.Models;
using MatoMusic.Infrastructure;

namespace MatoMusic.Core
{
    public interface IMusicInfoManager: ISingletonDependency
    {
        Task ClearQueue();
        Task<bool> CreatePlaylist(Playlist playlist);
        Task<bool> CreatePlaylistEntry(MusicInfo musicInfo, int playlistId);
        Task<bool> CreatePlaylistEntrys(List<MusicInfo> musics, int playlistId);
        Task<bool> CreatePlaylistEntrys(MusicCollectionInfo musicCollectionInfo, int playlistId);
        Task<bool> CreatePlaylistEntrysToMyFavourite(List<MusicInfo> musics);
        Task<bool> CreatePlaylistEntrysToMyFavourite(MusicCollectionInfo musicCollectionInfo);
        Task<bool> CreatePlaylistEntryToMyFavourite(MusicInfo musicInfo);
        Task<bool> CreateQueueEntry(MusicInfo musicInfo);
        Task<bool> CreateQueueEntrys(List<MusicInfo> musicInfos);
        Task<bool> CreateQueueEntrys(MusicCollectionInfo musics);
        Task<bool> DeleteMusicInfoFormQueueEntry(MusicInfo musicInfo);
        Task<bool> DeleteMusicInfoFormQueueEntry(string musicTitle);
        Task<bool> DeletePlaylist(long playlistId);
        Task<bool> DeletePlaylist(Playlist playlist);
        Task<bool> DeletePlaylistEntry(MusicInfo musicInfo, int playlistId);
        Task<bool> DeletePlaylistEntry(string musicTitle, int playlistId);
        Task<bool> DeletePlaylistEntryFromMyFavourite(MusicInfo musicInfo);
        Task<InfoResult<List<AlbumInfo>>> GetAlbumInfos();
        Task<AlphaGroupedObservableCollection<AlbumInfo>> GetAlphaGroupedAlbumInfo();
        Task<AlphaGroupedObservableCollection<ArtistInfo>> GetAlphaGroupedArtistInfo();
        Task<AlphaGroupedObservableCollection<MusicInfo>> GetAlphaGroupedMusicInfo();
        Task<InfoResult<List<ArtistInfo>>> GetArtistInfos();
        Task<bool> GetIsMyFavouriteContains(MusicInfo musicInfo);
        Task<bool> GetIsMyFavouriteContains(string musicTitle);
        Task<bool> GetIsPlaylistContains(MusicInfo musicInfo, int playlistId);
        Task<bool> GetIsPlaylistContains(string musicTitle, int playlistId);
        Task<bool> GetIsQueueContains(string musicTitle);
        Task<InfoResult<List<MusicInfo>>> GetMusicInfos();
        Task<List<Playlist>> GetPlaylist();
        Task<List<MusicInfo>> GetPlaylistEntry(int playlistId);
        Task<List<MusicInfo>> GetPlaylistEntryFormMyFavourite();
        Task<List<MusicInfo>> GetQueueEntry();
        Task<bool> InsertToEndQueueEntry(MusicInfo musicInfo);
        Task<bool> InsertToEndQueueEntrys(List<MusicInfo> musicInfos);
        Task<bool> InsertToNextQueueEntry(MusicInfo musicInfo);
        void ReorderMyFavourite(MusicInfo oldMusicInfo, MusicInfo newMusicInfo);
        void ReorderPlaylist(MusicInfo oldMusicInfo, MusicInfo newMusicInfo, int playlistId);
        void ReorderQueue(MusicInfo oldMusicInfo, MusicInfo newMusicInfo);
        Task<bool> UpdatePlaylist(Playlist playlist);
    }
}