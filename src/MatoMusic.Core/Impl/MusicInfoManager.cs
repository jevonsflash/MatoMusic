using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Models;
using MatoMusic.Infrastructure;
using static Microsoft.Maui.ApplicationModel.Permissions;
using System.Linq;

namespace MatoMusic.Core
{
    public partial class MusicInfoManager : IMusicInfoManager, ISingletonDependency
    {
        private const int MyFavouriteIndex = 1;
        private readonly IRepository<Queue, long> queueRepository;
        private readonly IRepository<PlaylistItem, long> playlistItemRepository;
        private readonly IRepository<Playlist, long> playlistRepository;
        private readonly IUnitOfWorkManager unitOfWorkManager;
        private readonly IMusicControlService _musicSystem;
        List<MusicInfo> _musicInfos;

        public MusicInfoManager(IRepository<Queue, long> queueRepository,
            IRepository<PlaylistItem, long> playlistItemRepository,
            IRepository<Playlist, long> playlistRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IMusicControlService musicControlService
            )
        {
            this.queueRepository = queueRepository;
            this.playlistItemRepository = playlistItemRepository;
            this.playlistRepository = playlistRepository;
            this.unitOfWorkManager = unitOfWorkManager;
            _musicSystem = musicControlService;
            _musicSystem.MusicInfoManager = this;

        }

        public async Task<bool> PermissionAuthorization<T>(T permission) where T : BasePermission
        {
            var status = await permission.CheckStatusAsync();

            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {

                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                return false;
            }

            if (permission.ShouldShowRationale())
            {
                // Prompt the user with additional information as to why the permission is needed
            }

            status = await permission.RequestAsync();

            return status == PermissionStatus.Granted;
        }

        public async Task<bool> MediaLibraryAuthorization()
        {
            var mediaPermission = await PermissionAuthorization(new Permissions.Media());
            var rdPermission = await PermissionAuthorization(new Permissions.StorageRead());
            var wtPermission = await PermissionAuthorization(new Permissions.StorageWrite());

            return mediaPermission && rdPermission && wtPermission;

            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var status2 = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            var status3 = await Permissions.CheckStatusAsync<Permissions.Media>();

            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                return false;
            }

            if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
            {
                // Prompt the user with additional information as to why the permission is needed
            }

            status = await Permissions.RequestAsync<Permissions.StorageRead>();
            return status == PermissionStatus.Granted;

        }

        public async Task<List<PlaylistInfo>> GetPlaylistInfo()
        {
            return await Task.Run(async () =>
            {
                List<Playlist> playlist = await this.GetPlaylist();
                var playlistInfo = playlist.Select(c => new PlaylistInfo()
                {
                    Id = c.Id,
                    GroupHeader = GetGroupHeader(c.Title),
                    Title = c.Title,
                    IsHidden = c.IsHidden,
                    IsRemovable = c.IsRemovable,
                    Musics = new ObservableCollection<MusicInfo>(GetPlaylistEntry(c.Id).Result)
                }).ToList();
                return playlistInfo;
            });
        }

        public partial Task ClearQueue();


        public partial Task<bool> CreatePlaylist(Playlist playlist);


        public partial Task<bool> CreatePlaylistEntry(MusicInfo musicInfo, long playlistId);


        public partial Task<bool> CreatePlaylistEntrys(List<MusicInfo> musics, long playlistId);


        public partial Task<bool> CreatePlaylistEntrys(MusicCollectionInfo musicCollectionInfo, long playlistId);


        public partial Task<bool> CreatePlaylistEntrysToMyFavourite(List<MusicInfo> musics);


        public partial Task<bool> CreatePlaylistEntrysToMyFavourite(MusicCollectionInfo musicCollectionInfo);


        public partial Task<bool> CreatePlaylistEntryToMyFavourite(MusicInfo musicInfo);


        public partial Task<bool> CreateQueueEntry(MusicInfo musicInfo);


        public partial Task<bool> CreateQueueEntrys(List<MusicInfo> musicInfos);


        public partial Task<bool> CreateQueueEntrys(MusicCollectionInfo musics);


        public partial Task<bool> DeleteMusicInfoFormQueueEntry(MusicInfo musicInfo);


        public partial Task<bool> DeleteMusicInfoFormQueueEntry(string musicTitle);


        public partial Task<bool> DeletePlaylist(long playlistId);


        public partial Task<bool> DeletePlaylist(Playlist playlist);


        public partial Task<bool> DeletePlaylistEntry(MusicInfo musicInfo, long playlistId);


        public partial Task<bool> DeletePlaylistEntry(string musicTitle, long playlistId);


        public partial Task<bool> DeletePlaylistEntryFromMyFavourite(MusicInfo musicInfo);


        public partial Task<InfoResult<List<AlbumInfo>>> GetAlbumInfos();


        public partial Task<AlphaGroupedObservableCollection<AlbumInfo>> GetAlphaGroupedAlbumInfo();


        public partial Task<AlphaGroupedObservableCollection<ArtistInfo>> GetAlphaGroupedArtistInfo();


        public partial Task<AlphaGroupedObservableCollection<MusicInfo>> GetAlphaGroupedMusicInfo();


        public partial Task<InfoResult<List<ArtistInfo>>> GetArtistInfos();


        public partial Task<bool> GetIsMyFavouriteContains(MusicInfo musicInfo);


        public partial Task<bool> GetIsMyFavouriteContains(string musicTitle);


        public partial Task<bool> GetIsPlaylistContains(MusicInfo musicInfo, long playlistId);


        public partial Task<bool> GetIsPlaylistContains(string musicTitle, long playlistId);


        public partial Task<bool> GetIsQueueContains(string musicTitle);


        public partial Task<InfoResult<List<MusicInfo>>> GetMusicInfos();


        public partial Task<List<Playlist>> GetPlaylist();


        public partial Task<List<MusicInfo>> GetPlaylistEntry(long playlistId);


        public partial Task<List<MusicInfo>> GetPlaylistEntryFormMyFavourite();


        public partial Task<List<MusicInfo>> GetQueueEntry();


        public partial Task<bool> InsertToEndQueueEntry(MusicInfo musicInfo);


        public partial Task<bool> InsertToEndQueueEntrys(List<MusicInfo> musicInfos);


        public partial Task<bool> InsertToNextQueueEntry(MusicInfo musicInfo, MusicInfo currentMusic);


        public partial void ReorderMyFavourite(MusicInfo oldMusicInfo, MusicInfo newMusicInfo);


        public partial void ReorderPlaylist(MusicInfo oldMusicInfo, MusicInfo newMusicInfo, long playlistId);


        public partial void ReorderQueue(MusicInfo oldMusicInfo, MusicInfo newMusicInfo);


        public partial Task<bool> UpdatePlaylist(Playlist playlist);

        private partial string GetGroupHeader(string title);


    }
}