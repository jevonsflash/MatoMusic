using System.Collections.ObjectModel;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.Provider;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Models;
using MatoMusic.Infrastructure;
using Application = Android.App.Application;

namespace MatoMusic.Core
{
    public partial class MusicInfoManager : IMusicInfoManager
    {
        private static string[] _mediaProjections =
     {
            MediaStore.Audio.Media.InterfaceConsts.Id,
            MediaStore.Audio.Media.InterfaceConsts.Artist,
            MediaStore.Audio.Media.InterfaceConsts.Album,
            MediaStore.Audio.Media.InterfaceConsts.Title,
            MediaStore.Audio.Media.InterfaceConsts.Duration,
            MediaStore.Audio.Media.InterfaceConsts.Data,
            MediaStore.Audio.Media.InterfaceConsts.IsMusic,
            MediaStore.Audio.Media.InterfaceConsts.AlbumId
        };

        private static string[] _genresProjections =
        {
            MediaStore.Audio.Genres.InterfaceConsts.Name,
            MediaStore.Audio.Genres.InterfaceConsts.Id
        };

        private static string[] _albumProjections =
        {
            MediaStore.Audio.Albums.InterfaceConsts.Id,
            MediaStore.Audio.Albums.InterfaceConsts.AlbumArt
        };
        public IList<MusicInfo> GetAllSongs()
        {

            IList<MusicInfo> songs = new ObservableCollection<MusicInfo>();
            ICursor mediaCursor, genreCursor, albumCursor;

            mediaCursor = Application.Context.ContentResolver.Query(
                MediaStore.Audio.Media.ExternalContentUri,
                _mediaProjections, null, null,
                MediaStore.Audio.Media.InterfaceConsts.TitleKey);

            int artistColumn = mediaCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Artist);
            int albumColumn = mediaCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Album);
            int titleColumn = mediaCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Title);
            int durationColumn = mediaCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Duration);
            int uriColumn = mediaCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Data);
            int idColumn = mediaCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Id);
            int isMusicColumn = mediaCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.IsMusic);
            int albumIdColumn = mediaCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.AlbumId);

            int isMusic;
            ulong duration, id;
            string artist, album, title, uri, genre, artwork, artworkId;

            if (mediaCursor.MoveToFirst())
            {
                do
                {
                    isMusic = int.Parse(mediaCursor.GetString(isMusicColumn));
                    if (isMusic != 0)
                    {
                        ulong.TryParse(mediaCursor.GetString(durationColumn), out duration);
                        artist = mediaCursor.GetString(artistColumn);
                        album = mediaCursor.GetString(albumColumn);
                        title = mediaCursor.GetString(titleColumn);
                        uri = mediaCursor.GetString(uriColumn);
                        ulong.TryParse(mediaCursor.GetString(idColumn), out id);
                        artworkId = mediaCursor.GetString(albumIdColumn);

                        genreCursor = Application.Context.ContentResolver.Query(
                            MediaStore.Audio.Genres.GetContentUriForAudioId("external", (int)id),
                            _genresProjections, null, null, null);
                        int genreColumn = genreCursor.GetColumnIndex(MediaStore.Audio.Genres.InterfaceConsts.Name);
                        if (genreCursor.MoveToFirst())
                        {
                            genre = genreCursor.GetString(genreColumn) ?? string.Empty;
                        }
                        else
                        {
                            genre = string.Empty;
                        }
                        //https://stackoverflow.com/questions/63181820/why-is-album-art-the-only-field-that-returns-null-from-mediastore-when-others-ar

                        ImageSource artworkImage = null;

                        if (DeviceInfo.Version.Major < 10)
                        {
                            albumCursor = Application.Context.ContentResolver.Query(
                             MediaStore.Audio.Albums.ExternalContentUri,
                             _albumProjections,
                             $"{MediaStore.Audio.Albums.InterfaceConsts.Id}=?",
                             new string[] { artworkId },
                             null);
                            int artworkColumn = albumCursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.AlbumArt);
                            if (albumCursor.MoveToFirst())
                            {
                                artwork = albumCursor.GetString(artworkColumn) ?? string.Empty;
                            }
                            else
                            {
                                artwork = String.Empty;
                            }

                            albumCursor?.Close();
                            artworkImage = artwork;

                        }
                        else
                        {
                            var extUrl = MediaStore.Audio.Albums.ExternalContentUri;
                            var albumArtUri = ContentUris.WithAppendedId(extUrl, long.Parse(artworkId));

                            try
                            {
                                //var art = System.IO.Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "albumart" + artworkId + ".jpg");
                                var art = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath, "albumart" + artworkId + ".jpg");

                                var bitmap = Application.Context.ContentResolver.LoadThumbnail(albumArtUri, new Android.Util.Size(1024, 1024), null);
                                var h = bitmap.Height;
                                var w = bitmap.Width;
                                var bb = bitmap.ByteCount;

                                using (Stream ms = new FileStream(art, FileMode.Create))
                                {
                                    bitmap.Compress(Bitmap.CompressFormat.Png, 100, ms);
                                    bitmap.Recycle();
                                }


                                artworkImage = art;



                            }
                            catch (Exception e)
                            {
                                System.Console.WriteLine(e.Message);
                            }
                        }




                        songs.Add(new MusicInfo()
                        {
                            Id = (int)id,
                            Title = title,
                            Artist = artist,
                            AlbumTitle = album,
                            Genre = genre,
                            Duration = duration / 1000,
                            Url = uri,
                            AlbumArt = artworkImage
                        });
                        genreCursor?.Close();
                    }
                } while (mediaCursor.MoveToNext());
            }
            mediaCursor?.Close();

            return songs;

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
                CommonHelper.ShowNoAuthorized();

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
                    var Infos = (from item in GetAllSongs()

                                 select new MusicInfo()
                                 {
                                     Id = item.Id,
                                     Title = item.Title,
                                     Duration = item.Duration,
                                     Url = item.Url,
                                     AlbumTitle = item.AlbumTitle,
                                     Artist = item.Artist,
                                     AlbumArt = item.AlbumArt,
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
                var isSucc = await GetMusicInfos();
                if (!isSucc.IsSucess)
                {
                    //CommonHelper.ShowNoAuthorized();

                }
                albumInfo = await Task.Run(() =>
                {
                    var info = (from item in isSucc.Result
                                group item by item.AlbumTitle
                        into c
                                select new AlbumInfo()
                                {
                                    Title = c.Key,
                                    GroupHeader = GetGroupHeader(c.Key),

                                    AlbumArt = c.FirstOrDefault().AlbumArt,
                                    Musics = new ObservableCollection<MusicInfo>(c.Select(d => new MusicInfo()
                                    {
                                        Id = d.Id,
                                        Title = d.Title,
                                        Duration = d.Duration,
                                        Url = d.Url,
                                        AlbumTitle = d.AlbumTitle,
                                        Artist = d.Artist,
                                        AlbumArt = d.AlbumArt,
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
                var isSucc = await GetMusicInfos();
                if (!isSucc.IsSucess)
                {
                    //CommonHelper.ShowNoAuthorized();

                }
                artistInfo = await Task.Run(() =>
                {

                    var info = (from item in isSucc.Result
                                group item by item.Artist
                        into c
                                select new ArtistInfo()
                                {
                                    Title = c.Key,
                                    GroupHeader = GetGroupHeader(c.Key),
                                    Musics = new ObservableCollection<MusicInfo>(c.Select(d => new MusicInfo()
                                    {
                                        Id = d.Id,
                                        Title = d.Title,
                                        Duration = d.Duration,
                                        Url = d.Url,
                                        AlbumTitle = d.AlbumTitle,
                                        Artist = d.Artist,
                                        AlbumArt = d.AlbumArt,
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
                artistInfo = new List<ArtistInfo>();
                result = false;
            }
            return new InfoResult<List<ArtistInfo>>(result, artistInfo);
        }

        
    }
}
