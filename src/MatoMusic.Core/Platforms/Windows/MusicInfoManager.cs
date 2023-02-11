using System.Collections.ObjectModel;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Models;
using MatoMusic.Infrastructure;
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


        /// <summary>
        /// 获取MusicInfo集合
        /// </summary>
        /// <returns></returns>
        public partial async Task<InfoResult<List<MusicInfo>>> GetMusicInfos()
        {
            List<MusicInfo> musicInfos;
            var result = false;
            if (await MediaLibraryAuthorization())
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
            if (await MediaLibraryAuthorization())
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
            if (await MediaLibraryAuthorization())
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