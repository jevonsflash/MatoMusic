using MediaPlayer;
using System.Collections.ObjectModel;
using MatoMusic.Infrastructure;
using MatoMusic.Core.Models;
using MatoMusic.Core.Interfaces;

namespace MatoMusic.Core
{
    public partial class MusicInfoManager : IMusicInfoManager
    {
        private MPMediaQuery _mediaQuery;

        public MPMediaQuery MediaQuery
        {
            get
            {
                if (_mediaQuery == null)
                {
                    _mediaQuery = new MPMediaQuery();
                }
                return _mediaQuery;
            }
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
                musicInfos = await Task.Run(() =>
                {
                    var Infos = (from item in MediaQuery.Items
                                 where item.MediaType == MPMediaType.Music
                                 select new MusicInfo()
                                 {
                                     Id = (int)item.PersistentID,
                                     Title = item.Title,
                                     Url = item.AssetURL.ToString(),
                                     Duration = Convert.ToUInt64(item.PlaybackDuration),

                                     AlbumTitle = item.AlbumTitle,
                                     Artist = item.Artist,
                                     AlbumArt = GetAlbumArtSource(item),
                                     GroupHeader = GetGroupHeader(item.Title),
                                     IsFavourite = GetIsMyFavouriteContains(item.Title).Result,
                                     IsInitFinished = true

                                 }).ToList();
                    return Infos;
                });

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

                albumInfo = await Task.Run(() =>
                {

                    var info = (from item in MediaQuery.Items
                                where item.MediaType == MPMediaType.Music
                                group item by item.AlbumTitle
                          into c
                                select new AlbumInfo()
                                {
                                    Title = c.Key,
                                    GroupHeader = GetGroupHeader(c.Key),

                                    AlbumArt = GetAlbumArtSource(c.FirstOrDefault()),
                                    Musics = new ObservableCollection<MusicInfo>(c.Select(d => new MusicInfo()
                                    {
                                        Id = (int)d.PersistentID,
                                        Title = d.Title,
                                        Url = d.AssetURL.ToString(),
                                        Duration = Convert.ToUInt64(d.PlaybackDuration),
                                        AlbumTitle = d.AlbumTitle,
                                        Artist = d.Artist,
                                        AlbumArt = GetAlbumArtSource(d),
                                        IsFavourite = GetIsMyFavouriteContains(d.Title).Result,
                                        IsInitFinished = true

                                    }))

                                }).ToList();
                    return info;
                });

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
                artistInfo = await Task.Run(() =>
                {


                    var Info = (from item in MediaQuery.Items
                                where item.MediaType == MPMediaType.Music
                                group item by item.Artist
                        into c
                                select new ArtistInfo()
                                {
                                    Title = c.Key,
                                    GroupHeader = GetGroupHeader(c.Key),
                                    Musics = new ObservableCollection<MusicInfo>(c.Select(d => new MusicInfo()
                                    {
                                        Id = (int)d.PersistentID,
                                        Title = d.Title,
                                        Duration = Convert.ToUInt64(d.PlaybackDuration),
                                        Url = d.AssetURL.ToString(),
                                        AlbumTitle = d.AlbumTitle,
                                        Artist = d.Artist,
                                        AlbumArt = GetAlbumArtSource(d),
                                        IsFavourite = GetIsMyFavouriteContains(d.Title).Result,
                                        IsInitFinished = true

                                    }))

                                }).ToList();
                    return Info;
                });

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
        /// 获取专辑封面Source
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private ImageSource GetAlbumArtSource(MPMediaItem item)
        {
            var _MPMediaItemArtwork = item.Artwork;
            if (_MPMediaItemArtwork != null)
            {
                var _UIImage = _MPMediaItemArtwork.ImageWithSize(new CoreGraphics.CGSize(200, 200));
                var result = ImageSource.FromStream(() => _UIImage.AsPNG().AsStream());
                return result;
            }
            else
            {
                return null;
            }
        }


    }
}