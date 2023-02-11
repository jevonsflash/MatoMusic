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
using MatoMusic.Core.Models.Entities;
using Abp.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.International.Converters.PinYinConverter;
using System.Text.RegularExpressions;

namespace MatoMusic.Core
{
    public partial class MusicInfoManager : IMusicInfoManager, ISingletonDependency
    {
        private const int MyFavouriteIndex = 1;
        private readonly IRepository<Queue, long> queueRepository;
        private readonly IRepository<PlaylistItem, long> playlistItemRepository;
        private readonly IRepository<Playlist, long> playlistRepository;
        private readonly IUnitOfWorkManager unitOfWorkManager;
        List<MusicInfo> _musicInfos;

        public MusicInfoManager(IRepository<Queue, long> queueRepository,
            IRepository<PlaylistItem, long> playlistItemRepository,
            IRepository<Playlist, long> playlistRepository,
            IUnitOfWorkManager unitOfWorkManager
            )
        {
            this.queueRepository = queueRepository;
            this.playlistItemRepository = playlistItemRepository;
            this.playlistRepository = playlistRepository;
            this.unitOfWorkManager = unitOfWorkManager;
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



        public partial Task<AlphaGroupedObservableCollection<AlbumInfo>> GetAlphaGroupedAlbumInfo();


        public partial Task<AlphaGroupedObservableCollection<ArtistInfo>> GetAlphaGroupedArtistInfo();


        public partial Task<AlphaGroupedObservableCollection<MusicInfo>> GetAlphaGroupedMusicInfo();

        public partial Task<InfoResult<List<MusicInfo>>> GetMusicInfos();

        public partial Task<InfoResult<List<ArtistInfo>>> GetArtistInfos();
        public partial Task<InfoResult<List<AlbumInfo>>> GetAlbumInfos();

        /// <summary>
        /// 将MusicInfo插入到队列
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> CreateQueueEntry(MusicInfo musicInfo)
        {
            var lastItemRank = queueRepository.GetAll().OrderBy(c => c.Rank).Select(c => c.Rank).LastOrDefault();
            var entry = new Queue(musicInfo.Title, lastItemRank, musicInfo.Id);
            var result = await queueRepository.InsertAndGetIdAsync(entry);
            return result > 0;
        }



        /// <summary>
        /// 将MusicInfo集合插入到队列中的末尾
        /// </summary>
        /// <param name="musicInfos"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> InsertToEndQueueEntrys(List<MusicInfo> musicInfos)
        {
            //var rankValue = 0;
            var sortedMusicInfos = musicInfos.Except(await GetQueueEntry(), new MusicInfoComparer()).ToList();
            var result = await CreateQueueEntrys(sortedMusicInfos);
            return result;
        }

        /// <summary>
        /// 将MusicInfo集合插入到队列
        /// </summary>
        /// <param name="musicInfos">需要进行操作的MusicInfo集合</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> CreateQueueEntrys(List<MusicInfo> musicInfos)
        {
            var lastItemRank = queueRepository.GetAll().OrderBy(c => c.Rank).Select(c => c.Rank).LastOrDefault();
            var entrys = new List<Queue>();
            foreach (var music in musicInfos)
            {
                var entry = new Queue(music.Title, lastItemRank, music.Id);
                lastItemRank++;
                entrys.Add(entry);
            }
            await queueRepository.GetDbContext().AddRangeAsync(entrys);
            return true;
        }

        /// <summary>
        /// 将MusicInfo集合插入到队列
        /// </summary>
        /// <param name="musics">需要进行操作的MusicInfo集合</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> CreateQueueEntrys(MusicCollectionInfo musics)
        {
            return await CreateQueueEntrys(musics.Musics.ToList());
        }

        /// <summary>
        /// 从队列中读取MusicInfo
        /// </summary>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<List<MusicInfo>> GetQueueEntry()
        {
            var queueEntrys = await queueRepository.GetAll().ToListAsync();
            if (_musicInfos == null || _musicInfos.Count == 0)
            {
                var isSucc = await GetMusicInfos();
                if (!isSucc.IsSucess)
                {
                    //CommonHelper.ShowNoAuthorized();
                }
                _musicInfos = isSucc.Result;

            }
            var result =
                from queue in queueEntrys
                join musicInfo in _musicInfos
                on queue.MusicInfoId equals musicInfo.Id
                orderby queue.Rank
                select musicInfo;
            return result.ToList();

        }

        /// <summary>
        /// 将MusicInfo插入到队列中的下一曲
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> InsertToNextQueueEntry(MusicInfo musicInfo, MusicInfo currentMusic)
        {
            var result = false;
            var isSuccessCreate = false;
            //如果没有则先创建
            if (!await GetIsQueueContains(musicInfo.Title))
            {
                isSuccessCreate = await CreateQueueEntry(musicInfo);
                await unitOfWorkManager.Current.SaveChangesAsync();
            }
            else
            {
                isSuccessCreate = true;
            }
            //确定包含后与下一曲交换位置
            if (isSuccessCreate)
            {
                var current = currentMusic;
                Queue newMusic = null;
                var lastItem = await queueRepository.FirstOrDefaultAsync(c => c.MusicTitle==current.Title);
                if (lastItem!=null)
                {
                    newMusic = await queueRepository.FirstOrDefaultAsync(c => c.Rank==lastItem.Rank+1);
                }

                var oldMusic = await queueRepository.FirstOrDefaultAsync(c => c.MusicTitle==musicInfo.Title);

                if (oldMusic ==null || newMusic==null)
                {
                    return true;
                }
                var oldRank = oldMusic.Rank;
                oldMusic.Rank=newMusic.Rank;
                newMusic.Rank=oldRank;
                queueRepository.Update(oldMusic);
                queueRepository.Update(newMusic);

                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 将MusicInfo插入到队列中的末尾
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> InsertToEndQueueEntry(MusicInfo musicInfo)
        {
            var result = false;
            var isSuccessCreate = false;
            //如果没有则先创建    
            var queueEntry = queueRepository.FirstOrDefault(c => c.MusicTitle == musicInfo.Title);

            if (queueEntry == null)
            {
                isSuccessCreate = await CreateQueueEntry(musicInfo);
            }
            else
            {
                await DeleteMusicInfoFormQueueEntry(queueEntry.MusicTitle);
                isSuccessCreate = await CreateQueueEntry(musicInfo);
            }

            return isSuccessCreate;
        }


        /// <summary>
        /// 返回一个值表明一个Title是否包含在队列中
        /// </summary>
        /// <param name="musicTitle">music标题</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> GetIsQueueContains(string musicTitle)
        {
            var queueEntrys = await queueRepository.FirstOrDefaultAsync(c => c.MusicTitle == musicTitle);
            return queueEntrys is not null;
        }

        /// <summary>
        /// 从队列中删除指定MusicInfo
        /// </summary>
        /// <param name="musicTitle"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> DeleteMusicInfoFormQueueEntry(string musicTitle)
        {
            var entry = await queueRepository.FirstOrDefaultAsync(c => c.MusicTitle == musicTitle);
            if (entry == null) return false;
            await queueRepository.DeleteAsync(entry);
            return true;
        }

        /// <summary>
        /// 从队列中删除指定MusicInfo
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> DeleteMusicInfoFormQueueEntry(MusicInfo musicInfo)
        {
            var musicTitle = musicInfo.Title;
            var entry = await queueRepository.FirstOrDefaultAsync(c => c.MusicTitle == musicTitle);
            if (entry == null) return false;
            await queueRepository.DeleteAsync(entry);
            return true;
        }

        /// <summary>
        /// 交换队列中两个MusicInfo的位置
        /// </summary>
        /// <param name="oldMusicInfo"></param>
        /// <param name="newMusicInfo"></param>
        [UnitOfWork]
        public void ReorderQueue(MusicInfo oldMusicInfo, MusicInfo newMusicInfo)
        {
            var oldMusic = queueRepository.FirstOrDefault(c => c.MusicTitle==oldMusicInfo.Title);
            var newMusic = queueRepository.FirstOrDefault(c => c.MusicTitle==newMusicInfo.Title);
            if (oldMusic ==null || newMusic==null)
            {
                return;
            }
            var oldRank = oldMusic.Rank;
            oldMusic.Rank=newMusic.Rank;
            newMusic.Rank=oldRank;
            queueRepository.Update(oldMusic);
            queueRepository.Update(newMusic);
        }

        /// <summary>
        /// 从队列中清除所有MusicInfo
        /// </summary>
        [UnitOfWork]
        public async Task ClearQueue()
        {
            await queueRepository.DeleteAsync(c => true);

        }

        /// <summary>
        /// 将MusicInfo插入到歌单
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> CreatePlaylistEntry(MusicInfo musicInfo, long playlistId)
        {
            var lastItemRank = playlistItemRepository.GetAll().OrderBy(c => c.Rank).Select(c => c.Rank).LastOrDefault();
            var entry = new PlaylistItem(playlistId, musicInfo.Id, musicInfo.Title, lastItemRank);
            var result = await playlistItemRepository.InsertAndGetIdAsync(entry) > 0;
            if (result)
            {
                if (playlistId == MyFavouriteIndex)
                {
                    musicInfo.SetFavourite(true, false);
                }
            }
            return result;
        }

        /// <summary>
        /// 将MusicInfo集合插入到歌单
        /// </summary>
        /// <param name="musics"></param>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> CreatePlaylistEntrys(List<MusicInfo> musics, long playlistId)
        {
            var lastItemRank = playlistItemRepository.GetAll().OrderBy(c => c.Rank).Select(c => c.Rank).LastOrDefault();
            var entrys = new List<PlaylistItem>();
            foreach (var music in musics)
            {
                var entry = new PlaylistItem(playlistId, music.Id, music.Title, lastItemRank);
                lastItemRank++;
                entrys.Add(entry);

            }

            await playlistItemRepository.GetDbContext().AddRangeAsync(entrys);

            if (playlistId == MyFavouriteIndex)
            {
                foreach (var musicInfo in musics)
                {
                    musicInfo.SetFavourite(true, false);
                }
            }

            return true;
        }

        /// <summary>
        /// 将MusicInfo集合插入到歌单
        /// </summary>
        /// <param name="musicCollectionInfo"></param>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> CreatePlaylistEntrys(MusicCollectionInfo musicCollectionInfo, long playlistId)
        {
            var result = await CreatePlaylistEntrys(musicCollectionInfo.Musics.ToList(), playlistId);
            return result;
        }


        /// <summary>
        /// 从歌单中删除MusicInfo根据指定的Title
        /// </summary>
        /// <param name="musicTitle"></param>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> DeletePlaylistEntry(string musicTitle, long playlistId)
        {
            var entry = await playlistItemRepository.FirstOrDefaultAsync(c => c.PlaylistId == playlistId && c.MusicTitle == musicTitle);
            if (entry != null)
            {
                await playlistItemRepository.DeleteAsync(entry);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 从歌单中删除MusicInfo
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> DeletePlaylistEntry(MusicInfo musicInfo, long playlistId)
        {
            var result = await DeletePlaylistEntry(musicInfo.Title, playlistId);
            if (result)
            {
                musicInfo.SetFavourite(false, false);
            }
            return result;
        }


        /// <summary>
        /// 将MusicInfo插入到“我最喜爱”歌单
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> CreatePlaylistEntryToMyFavourite(MusicInfo musicInfo)
        {
            var result = await CreatePlaylistEntry(musicInfo, MyFavouriteIndex);
            return result;

        }

        /// <summary>
        /// 将MusicInfo集合插入到“我最喜爱”
        /// </summary>
        /// <param name="musics"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> CreatePlaylistEntrysToMyFavourite(List<MusicInfo> musics)
        {
            var result = await CreatePlaylistEntrys(musics, MyFavouriteIndex);
            return result;
        }

        /// <summary>
        /// 将MusicInfo集合插入到“我最喜爱”
        /// </summary>
        /// <param name="musicCollectionInfo"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> CreatePlaylistEntrysToMyFavourite(MusicCollectionInfo musicCollectionInfo)
        {
            var result = await CreatePlaylistEntrys(musicCollectionInfo, MyFavouriteIndex);
            return result;
        }



        /// <summary>
        /// 从“我最喜爱”中删除MusicInfo
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> DeletePlaylistEntryFromMyFavourite(MusicInfo musicInfo)
        {
            return await DeletePlaylistEntry(musicInfo, MyFavouriteIndex);
        }

        /// <summary>
        /// 从歌单中读取MusicInfo
        /// </summary>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<List<MusicInfo>> GetPlaylistEntry(long playlistId)
        {
            var currentPlaylistEntries = await playlistItemRepository.GetAllListAsync(c => c.PlaylistId == playlistId);
            List<MusicInfo> musicInfos;

            var isSucc = await GetMusicInfos();
            if (!isSucc.IsSucess)
            {
                //CommonHelper.ShowNoAuthorized();

            }
            musicInfos = isSucc.Result;

            var result =
                from currentPlaylistEntry in currentPlaylistEntries
                join musicInfo in _musicInfos
                on currentPlaylistEntry.MusicInfoId equals musicInfo.Id
                orderby currentPlaylistEntry.Rank
                select musicInfo;
            return result.ToList();

        }

        /// <summary>
        /// 从“我最喜爱”中读取MusicInfo
        /// </summary>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<List<MusicInfo>> GetPlaylistEntryFormMyFavourite()
        {
            return await GetPlaylistEntry(MyFavouriteIndex);
        }

        /// <summary>
        /// 返回一个值表明一个Title是否包含在某个歌单中
        /// </summary>
        /// <param name="musicTitle">music标题</param>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> GetIsPlaylistContains(string musicTitle, long playlistId)
        {
            var result = await playlistItemRepository.FirstOrDefaultAsync(c => c.MusicTitle == musicTitle && c.PlaylistId == playlistId);
            return result is not null;

        }

        /// <summary>
        ///  返回一个值表明一个MusicInfo是否包含在某个歌单中
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> GetIsPlaylistContains(MusicInfo musicInfo, long playlistId)
        {
            return await GetIsPlaylistContains(musicInfo.Title, playlistId);
        }

        /// <summary>
        /// 返回一个值表明一个Title是否包含在"我最喜爱"中
        /// </summary>
        /// <param name="musicTitle">music标题</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> GetIsMyFavouriteContains(string musicTitle)
        {
            return await GetIsPlaylistContains(musicTitle, MyFavouriteIndex);

        }

        /// <summary>
        ///  返回一个值表明一个MusicInfo是否包含在"我最喜爱"中
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> GetIsMyFavouriteContains(MusicInfo musicInfo)
        {
            return await GetIsPlaylistContains(musicInfo, MyFavouriteIndex);

        }

        /// <summary>
        /// 交换某歌单中两个MusicInfo的位置
        /// </summary>
        /// <param name="oldMusicInfo"></param>
        /// <param name="newMusicInfo"></param>
        /// <param name="playlistId"></param>
        [UnitOfWork]
        public void ReorderPlaylist(MusicInfo oldMusicInfo, MusicInfo newMusicInfo, long playlistId)
        {
            var oldMusic = playlistItemRepository.GetAll().Where(c => c.PlaylistId==playlistId).FirstOrDefault(c => c.MusicTitle==oldMusicInfo.Title);
            var newMusic = playlistItemRepository.GetAll().Where(c => c.PlaylistId==playlistId).FirstOrDefault(c => c.MusicTitle==newMusicInfo.Title);
            if (oldMusic ==null || newMusic==null)
            {
                return;
            }

            var oldRank = oldMusic.Rank;
            oldMusic.Rank=newMusic.Rank;
            newMusic.Rank=oldRank;
            playlistItemRepository.Update(oldMusic);
            playlistItemRepository.Update(newMusic);
        }
        /// <summary>
        /// 交换"我最喜爱"中两个MusicInfo的位置
        /// </summary>
        /// <param name="oldMusicInfo"></param>
        /// <param name="newMusicInfo"></param>
        [UnitOfWork]
        public void ReorderMyFavourite(MusicInfo oldMusicInfo, MusicInfo newMusicInfo)
        {
            ReorderPlaylist(oldMusicInfo, newMusicInfo, MyFavouriteIndex);
        }
        /// <summary>
        /// 获取Playlist
        /// </summary>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<List<Playlist>> GetPlaylist()
        {
            return await playlistRepository.GetAllListAsync();

        }


        /// <summary>
        /// 创建Playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> CreatePlaylist(Playlist playlist)
        {
            var result = await playlistRepository.InsertAndGetIdAsync(playlist);
            return result > 0;
        }

        /// <summary>
        /// 更新Playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> UpdatePlaylist(Playlist playlist)
        {
            var result = await playlistRepository.InsertOrUpdateAndGetIdAsync(playlist);
            return result > 0;
        }

        /// <summary>
        /// 根据Id删除Playlist
        /// </summary>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> DeletePlaylist(long playlistId)
        {
            await playlistItemRepository.DeleteAsync(c => c.PlaylistId == playlistId);
            await playlistRepository.DeleteAsync(playlistId);
            return true;
        }

        /// <summary>
        /// 根据Playlist删除Playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        [UnitOfWork]
        public async Task<bool> DeletePlaylist(Playlist playlist)
        {
            return await DeletePlaylist(playlist.Id);

        }

        /// <summary>
        /// 获取一个字符串的标题头
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private string GetGroupHeader(string title)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(title))
            {


                if (Regex.IsMatch(title.Substring(0, 1), @"^[\u4e00-\u9fa5]+$"))
                {
                    try
                    {
                        var chinese = new ChineseChar(title.First());
                        result = chinese.Pinyins[0].Substring(0, 1);
                    }
                    catch (Exception ex)
                    {
                        return string.Empty;
                    }

                }
                else
                {
                    result = title.Substring(0, 1);
                }
            }
            return result;

        }


    }
}