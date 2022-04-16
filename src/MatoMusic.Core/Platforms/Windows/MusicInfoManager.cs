using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Models;
using MatoMusic.Core.ViewModel;
using MatoMusic.Infrastructure;
using Microsoft.International.Converters.PinYinConverter;
using Microsoft.Maui.Controls;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace MatoMusic.Core
{
    public partial class MusicInfoManager : IMusicInfoManager
    {
       
        private async Task<List<MusicInfo>> SetMusicListAsync(StorageFolder musicFolder = null)
        {

            var localSongs = new List<MusicInfo>();
            List<StorageFile> songfiles = new List<StorageFile>();
            if (musicFolder == null)
            {
                musicFolder = KnownFolders.MusicLibrary;
            }

            await GetLocalSongsAysnc(songfiles, musicFolder);
            localSongs = await PopulateSongListAsync(songfiles);

            return localSongs;

        }
        /// <summary>
        /// 该方法用于扫描某个文件夹下的歌曲文件
        /// </summary>
        /// <param name="songs">歌曲集合</param>
        /// <param name="parent">要扫描的文件夹</param>
        private async Task GetLocalSongsAysnc(List<StorageFile> songFiles, StorageFolder parent)
        {
            foreach (var item in await parent.GetFilesAsync())
            {
                if (item.FileType == ".mp3")
                    songFiles.Add(item);
            }
            foreach (var folder in await parent.GetFoldersAsync())
            {
                await GetLocalSongsAysnc(songFiles, folder);
            }
        }
        /// <summary>
        /// 该方法用于将songFiles里的文件转变为songs里的ViewModel
        /// </summary>
        /// <param name="localSongs">可显示的歌曲</param>
        /// <param name="songFiles">歌曲文件列表</param>
        /// <returns></returns>
        private async Task<List<MusicInfo>> PopulateSongListAsync(List<StorageFile> songFiles)
        {

            var localSongs = new List<MusicInfo>();
            int Id = 1;

            foreach (var file in songFiles)
            {
                MusicInfo song = new MusicInfo();

                // 1. 获取文件信息
                MusicProperties musicProperty = await file.Properties.GetMusicPropertiesAsync();
                if (!string.IsNullOrEmpty(musicProperty.Title))
                    song.Title = musicProperty.Title;
                else
                {
                    song.Title = file.DisplayName;
                }

                StorageItemThumbnail currentThumb = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 60, ThumbnailOptions.UseCurrentScale);

                // 2.将文件信息转换为数据模型

                string coverUri = "ms-appx:///Assets/Default/Default.jpg";

                song.Id = Id;
                song.Url = file.Path;
                song.GroupHeader = GetGroupHeader(song.Title);
                if (!string.IsNullOrEmpty(musicProperty.Artist))
                    song.Artist = musicProperty.Artist;
                else
                    song.Artist = "未知歌手";
                if (!string.IsNullOrEmpty(musicProperty.Album))
                    song.AlbumTitle = musicProperty.Album;
                else
                    song.AlbumTitle = "未知唱片";
                song.Duration = (ulong)musicProperty.Duration.TotalSeconds;


                //3. 添加至UI集合中

                var task01 = SaveImagesAsync(file, song);
                var result = await task01;
                var task02 = task01.ContinueWith((e) =>
                  {
                      if (result.IsSucess)
                      {
                          song.AlbumArtPath = result.Result;
                      }
                      else
                      {
                          song.AlbumArtPath = coverUri;

                      }
                  });

                Task.WaitAll(task01, task02);
                song.IsInitFinished = true;
                localSongs.Add(song);
                Id++;

            }
            return localSongs;
        }

        /// <summary>
        /// 获取分组包装好的MusicInfo集合
        /// </summary>
        /// <returns></returns>
        public partial async Task<AlphaGroupedObservableCollection<MusicInfo>> GetAlphaGroupedMusicInfo()
        {
            AlphaGroupedObservableCollection<MusicInfo> result = new AlphaGroupedObservableCollection<MusicInfo>();
            List<MusicInfo> list;
            var isSucc = await GetMusicInfos();
            if (!isSucc.IsSucess)
            {
                return result;
                //CommonHelper.ShowNoAuthorized();

            }
            list = isSucc.Result;
            list.ForEach(c =>
            {
                result.Add(c, c.GroupHeader);

            });
            result.Root = result.Root.Where(c => c.HasItems).ToList();
            return result;


        }
        /// <summary>
        /// 获取分组包装好的ArtistInfo集合
        /// </summary>
        /// <returns></returns>
        public partial async Task<AlphaGroupedObservableCollection<ArtistInfo>> GetAlphaGroupedArtistInfo()
        {
            AlphaGroupedObservableCollection<ArtistInfo> result = new AlphaGroupedObservableCollection<ArtistInfo>();
            List<ArtistInfo> list;
            var isSucc = await GetArtistInfos();
            if (!isSucc.IsSucess)
            {
                //CommonHelper.ShowNoAuthorized();

            }
            list = isSucc.Result;
            list.ForEach(c =>
            {
                result.Add(c, c.GroupHeader);

            });
            result.Root = result.Root.Where(c => c.HasItems).ToList();
            return result;


        }

        /// <summary>
        /// 获取分组包装好的AlbumInfo集合
        /// </summary>
        /// <returns></returns>
        public partial async Task<AlphaGroupedObservableCollection<AlbumInfo>> GetAlphaGroupedAlbumInfo()
        {
            AlphaGroupedObservableCollection<AlbumInfo> result = new AlphaGroupedObservableCollection<AlbumInfo>();
            List<AlbumInfo> list;
            var isSucc = await GetAlbumInfos();
            if (!isSucc.IsSucess)
            {
                //CommonHelper.ShowNoAuthorized();

            }
            list = isSucc.Result;
            list.ForEach(c =>
            {
                result.Add(c, c.GroupHeader);

            });
            result.Root = result.Root.Where(c => c.HasItems).ToList();
            return result;


        }

        private partial bool MediaLibraryAuthorization()
        {
            var result = true;
            //权限验证
            return result;
        }

        /// <summary>
        /// 获取MusicInfo集合
        /// </summary>
        /// <returns></returns>
        public partial async Task<InfoResult<List<MusicInfo>>> GetMusicInfos()
        {
            List<MusicInfo> musicInfos;
            var result = false;
            if (MediaLibraryAuthorization())
            {
                musicInfos = await SetMusicListAsync();
                result = true;
            }
            else
            {
                musicInfos = new List<MusicInfo>();
                result = false;
            }
            return new InfoResult<List<MusicInfo>>(result, musicInfos);

        }

        /// <summary>
        /// 获取AlbumInfo集合
        /// </summary>
        /// <returns></returns>

        public partial async Task<InfoResult<List<AlbumInfo>>> GetAlbumInfos()
        {
            List<AlbumInfo> albumInfo;
            var result = false;
            if (MediaLibraryAuthorization())
            {


                albumInfo = (from item in await SetMusicListAsync()

                             group item by item.AlbumTitle
                    into c
                             select new AlbumInfo()
                             {
                                 Title = c.Key,
                                 GroupHeader = GetGroupHeader(c.Key),
                                 Artist = c.FirstOrDefault().Artist,
                                 AlbumArtPath = c.FirstOrDefault().AlbumArtPath,
                                 Musics = new ObservableCollection<MusicInfo>(c.Select(d => new MusicInfo()
                                 {
                                     Id = d.Id,
                                     Title = d.Title,
                                     Duration = d.Duration,
                                     Url = d.Url,
                                     AlbumTitle = d.AlbumTitle,
                                     Artist = d.Artist,
                                     AlbumArtPath = d.AlbumArtPath,
                                     IsFavourite = GetIsMyFavouriteContains(d.Title).Result,
                                     IsInitFinished = true

                                 }))

                             }).ToList();
                result = true;

            }
            else
            {
                albumInfo = new List<AlbumInfo>();
                result = false;
            }
            return new InfoResult<List<AlbumInfo>>(result, albumInfo);

        }

        /// <summary>
        /// 获取ArtistInfo集合
        /// </summary>
        /// <returns></returns>
        public partial async Task<InfoResult<List<ArtistInfo>>> GetArtistInfos()
        {
            List<ArtistInfo> artistInfo;

            var result = false;
            if (MediaLibraryAuthorization())
            {


                artistInfo = (from item in await SetMusicListAsync()
                              group item by item.Artist
                    into c
                              select new ArtistInfo()
                              {
                                  Title = c.Key,
                                  AlbumArtPath = c.FirstOrDefault().AlbumArtPath,
                                  GroupHeader = GetGroupHeader(c.Key),
                                  Musics = new ObservableCollection<MusicInfo>(c.Select(d => new MusicInfo()
                                  {
                                      Id = d.Id,
                                      Title = d.Title,
                                      Duration = d.Duration,
                                      Url = d.Url,
                                      AlbumTitle = d.AlbumTitle,
                                      Artist = d.Artist,
                                      AlbumArtPath = d.AlbumArtPath,
                                      IsFavourite = GetIsMyFavouriteContains(d.Title).Result,
                                      IsInitFinished = true

                                  }))

                              }).ToList();
                result = true;

            }
            else
            {
                artistInfo = new List<ArtistInfo>();
                result = false;
            }
            return new InfoResult<List<ArtistInfo>>(result, artistInfo);
        }

        /// <summary>
        /// 将MusicInfo插入到队列
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        public partial async Task<bool> CreateQueueEntry(MusicInfo musicInfo)
        {
            var entry = new Queue(musicInfo.Title, 0, musicInfo.Id);
            var result = await queueRepository.InsertAndGetIdAsync(entry);
            return result > 0;
        }



        /// <summary>
        /// 将MusicInfo集合插入到队列中的末尾
        /// </summary>
        /// <param name="musicInfos"></param>
        /// <returns></returns>
        public partial async Task<bool> InsertToEndQueueEntrys(List<MusicInfo> musicInfos)
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
        public partial async Task<bool> CreateQueueEntrys(List<MusicInfo> musicInfos)
        {
            var entrys = musicInfos.Select(c => new Queue(c.Title, 0, c.Id));
            foreach (var entry in entrys)
            {
                await queueRepository.InsertAsync(entry);
            }
            return true;
        }

        /// <summary>
        /// 将MusicInfo集合插入到队列
        /// </summary>
        /// <param name="musics">需要进行操作的MusicInfo集合</param>
        /// <returns></returns>
        public partial async Task<bool> CreateQueueEntrys(MusicCollectionInfo musics)
        {
            return await CreateQueueEntrys(musics.Musics.ToList());
        }

        /// <summary>
        /// 从队列中读取MusicInfo
        /// </summary>
        /// <returns></returns>
        public partial async Task<List<MusicInfo>> GetQueueEntry()
        {
            var queueEntrys = await queueRepository.GetAllListAsync();
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
                from musicInfo in _musicInfos
                join queue in queueEntrys
                on musicInfo.Id equals queue.MusicInfoId
                orderby queue.Id
                select musicInfo;
            var musicInfoList = result.ToList();
            return musicInfoList;
        }

        /// <summary>
        /// 将MusicInfo插入到队列中的下一曲
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        public partial async Task<bool> InsertToNextQueueEntry(MusicInfo musicInfo, MusicInfo currentMusic)
        {
            var result = false;
            var isSuccessCreate = false;
            //如果没有则先创建
            if (!await GetIsQueueContains(musicInfo.Title))
            {
                isSuccessCreate = await CreateQueueEntry(musicInfo);
            }
            else
            {
                isSuccessCreate = true;
            }
            //确定包含后与下一曲交换位置
            if (isSuccessCreate)
            {
                var currnet = currentMusic;
                var next = _musicSystem.GetNextMusic(currnet, false);

                ReorderQueue(musicInfo, next);
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
        public partial async Task<bool> InsertToEndQueueEntry(MusicInfo musicInfo)
        {
            var result = false;
            var isSuccessCreate = false;
            //如果没有则先创建    
            var queueEntrys = await queueRepository.GetAllListAsync();
            var queueEntry = queueEntrys.FirstOrDefault(c => c.MusicTitle == musicInfo.Title);

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
        public partial async Task<bool> GetIsQueueContains(string musicTitle)
        {
            var queueEntrys = await queueRepository.FirstOrDefaultAsync(c => c.MusicTitle == musicTitle);
            return queueEntrys is not null;
        }

        /// <summary>
        /// 从队列中删除指定MusicInfo
        /// </summary>
        /// <param name="musicTitle"></param>
        /// <returns></returns>
        public partial async Task<bool> DeleteMusicInfoFormQueueEntry(string musicTitle)
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
        public partial async Task<bool> DeleteMusicInfoFormQueueEntry(MusicInfo musicInfo)
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
        public partial void ReorderQueue(MusicInfo oldMusicInfo, MusicInfo newMusicInfo)
        {

        }

        /// <summary>
        /// 从队列中清除所有MusicInfo
        /// </summary>
        public partial async Task ClearQueue()
        {
            await queueRepository.DeleteAsync(c => true);

        }

        /// <summary>
        /// 将MusicInfo插入到歌单
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        public partial async Task<bool> CreatePlaylistEntry(MusicInfo musicInfo, long playlistId)
        {
            var entry = new PlaylistItem(playlistId, musicInfo.Title, 0);
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
        public partial async Task<bool> CreatePlaylistEntrys(List<MusicInfo> musics, long playlistId)
        {
            var entrys = musics.Select(c => new PlaylistItem(playlistId, c.Title, 0));
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
        public partial async Task<bool> CreatePlaylistEntrys(MusicCollectionInfo musicCollectionInfo, long playlistId)
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
        public partial async Task<bool> DeletePlaylistEntry(string musicTitle, long playlistId)
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
        public partial async Task<bool> DeletePlaylistEntry(MusicInfo musicInfo, long playlistId)
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
        public partial async Task<bool> CreatePlaylistEntryToMyFavourite(MusicInfo musicInfo)
        {
            var result = await CreatePlaylistEntry(musicInfo, MyFavouriteIndex);
            return result;

        }

        /// <summary>
        /// 将MusicInfo集合插入到“我最喜爱”
        /// </summary>
        /// <param name="musics"></param>
        /// <returns></returns>
        public partial async Task<bool> CreatePlaylistEntrysToMyFavourite(List<MusicInfo> musics)
        {
            var result = await CreatePlaylistEntrys(musics, MyFavouriteIndex);
            return result;
        }

        /// <summary>
        /// 将MusicInfo集合插入到“我最喜爱”
        /// </summary>
        /// <param name="musicCollectionInfo"></param>
        /// <returns></returns>
        public partial async Task<bool> CreatePlaylistEntrysToMyFavourite(MusicCollectionInfo musicCollectionInfo)
        {
            var result = await CreatePlaylistEntrys(musicCollectionInfo, MyFavouriteIndex);
            return result;
        }



        /// <summary>
        /// 从“我最喜爱”中删除MusicInfo
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        public partial async Task<bool> DeletePlaylistEntryFromMyFavourite(MusicInfo musicInfo)
        {
            return await DeletePlaylistEntry(musicInfo, MyFavouriteIndex);
        }

        /// <summary>
        /// 从歌单中读取MusicInfo
        /// </summary>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        public partial async Task<List<MusicInfo>> GetPlaylistEntry(long playlistId)
        {
            var currentPlaylistEntrie = await playlistItemRepository.GetAllListAsync(c => c.PlaylistId == playlistId);
            List<MusicInfo> musicInfos;

            var isSucc = await GetMusicInfos();
            if (!isSucc.IsSucess)
            {
                //CommonHelper.ShowNoAuthorized();

            }
            musicInfos = isSucc.Result;

            var result = from item
                in musicInfos
                         where (from c
                             in currentPlaylistEntrie
                                select c.MusicTitle).Contains(item.Title)
                         orderby item.Id
                         select item;
            return result.ToList();

        }

        /// <summary>
        /// 从“我最喜爱”中读取MusicInfo
        /// </summary>
        /// <returns></returns>
        public partial async Task<List<MusicInfo>> GetPlaylistEntryFormMyFavourite()
        {
            return await GetPlaylistEntry(MyFavouriteIndex);
        }

        /// <summary>
        /// 返回一个值表明一个Title是否包含在某个歌单中
        /// </summary>
        /// <param name="musicTitle">music标题</param>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        public partial async Task<bool> GetIsPlaylistContains(string musicTitle, long playlistId)
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
        public partial async Task<bool> GetIsPlaylistContains(MusicInfo musicInfo, long playlistId)
        {
            return await GetIsPlaylistContains(musicInfo.Title, playlistId);
        }

        /// <summary>
        /// 返回一个值表明一个Title是否包含在"我最喜爱"中
        /// </summary>
        /// <param name="musicTitle">music标题</param>
        /// <returns></returns>
        public partial async Task<bool> GetIsMyFavouriteContains(string musicTitle)
        {
            return await GetIsPlaylistContains(musicTitle, MyFavouriteIndex);

        }

        /// <summary>
        ///  返回一个值表明一个MusicInfo是否包含在"我最喜爱"中
        /// </summary>
        /// <param name="musicInfo">musicInfo对象</param>
        /// <returns></returns>
        public partial async Task<bool> GetIsMyFavouriteContains(MusicInfo musicInfo)
        {
            return await GetIsPlaylistContains(musicInfo, MyFavouriteIndex);

        }

        /// <summary>
        /// 交换某歌单中两个MusicInfo的位置
        /// </summary>
        /// <param name="oldMusicInfo"></param>
        /// <param name="newMusicInfo"></param>
        /// <param name="playlistId"></param>
        public partial void ReorderPlaylist(MusicInfo oldMusicInfo, MusicInfo newMusicInfo, long playlistId)
        {
        }
        /// <summary>
        /// 交换"我最喜爱"中两个MusicInfo的位置
        /// </summary>
        /// <param name="oldMusicInfo"></param>
        /// <param name="newMusicInfo"></param>
        public partial void ReorderMyFavourite(MusicInfo oldMusicInfo, MusicInfo newMusicInfo)
        {
            ReorderPlaylist(oldMusicInfo, newMusicInfo, MyFavouriteIndex);
        }
        /// <summary>
        /// 获取Playlist
        /// </summary>
        /// <returns></returns>
        public partial async Task<List<Playlist>> GetPlaylist()
        {
            return await playlistRepository.GetAllListAsync();
        }


        /// <summary>
        /// 创建Playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public partial async Task<bool> CreatePlaylist(Playlist playlist)
        {
            var result = await playlistRepository.InsertAndGetIdAsync(playlist);
            return result > 0;
        }

        /// <summary>
        /// 更新Playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public partial async Task<bool> UpdatePlaylist(Playlist playlist)
        {
            var result = await playlistRepository.InsertOrUpdateAndGetIdAsync(playlist);
            return result > 0;
        }

        /// <summary>
        /// 根据Id删除Playlist
        /// </summary>
        /// <param name="playlistId"></param>
        /// <returns></returns>
        public partial async Task<bool> DeletePlaylist(long playlistId)
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
        public partial async Task<bool> DeletePlaylist(Playlist playlist)
        {
            return await DeletePlaylist(playlist.Id);

        }

        /// <summary>
        /// 获取一个字符串的标题头
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private partial string GetGroupHeader(string title)
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


        private async Task<InfoResult<string>> SaveImagesAsync(StorageFile file, MusicInfo mediafile)
        {
            var FileName = string.Empty;
            var albumArt = AlbumArtFileExists(mediafile, out FileName);
            if (!albumArt)
            {
                var albumartFolder = ApplicationData.Current.LocalCacheFolder;

                return new InfoResult<string>(true, albumartFolder.Path + FileName);
            }

            try
            {
                using (StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 300, ThumbnailOptions.UseCurrentScale))
                {
                    if (thumbnail == null)
                    {
                        return new InfoResult<string>(false);
                    }

                    switch (thumbnail.Type)
                    {
                        case ThumbnailType.Image:
                            var albumart = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(FileName, CreationCollisionOption.FailIfExists);
                            IBuffer buf;
                            Windows.Storage.Streams.Buffer inputBuffer = new Windows.Storage.Streams.Buffer(1024);
                            using (IRandomAccessStream albumstream = await albumart.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                while ((buf = await thumbnail.ReadAsync(inputBuffer, inputBuffer.Capacity, InputStreamOptions.None)).Length > 0)
                                {
                                    await albumstream.WriteAsync(buf);
                                }

                                return new InfoResult<string>(true, albumart.Path);
                            }

                        //case ThumbnailType.Icon:
                        //    using (TagLib.File tagFile = TagLib.File.Create(new SimpleFileAbstraction(file), TagLib.ReadStyle.Average))
                        //    {
                        //        if (tagFile.Tag.Pictures.Length >= 1)
                        //        {
                        //            var image = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(@"AlbumArts\" + albumArt.FileName + ".jpg", CreationCollisionOption.FailIfExists);

                        //            using (var albumstream = await image.OpenStreamForWriteAsync())
                        //            {
                        //                await albumstream.WriteAsync(tagFile.Tag.Pictures[0].Data.Data, 0, tagFile.Tag.Pictures[0].Data.Data.Length);
                        //            }
                        //            return true;
                        //        }
                        //    }
                        //  break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return new InfoResult<string>(false);
        }
        private bool AlbumArtFileExists(MusicInfo file, out string FilePath)
        {
            var albumartFolder = ApplicationData.Current.LocalCacheFolder;
            var md5Path = (file.AlbumTitle + file.Artist).ToLower();
            var path = @"\AlbumArts\" + md5Path + ".jpg";
            FilePath = path;
            if (!System.IO.File.Exists(albumartFolder.Path + path))
            {
                //var albumart = await albumartFolder.CreateFileAsync(@"AlbumArts\" + md5Path + ".jpg", CreationCollisionOption.FailIfExists).AsTask().ConfigureAwait(false);
                return true;
            }
            return false;
        }


    }
}