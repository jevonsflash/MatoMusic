using Abp.AutoMapper;
using Microsoft.Maui.Controls;

namespace MatoMusic.Core.Models
{

    /* 项目“MatoMusic.Core (net6.0-windows10.0.19041)”的未合并的更改
    在此之前:
        [AutoMapFrom(typeof(Playlist.Playlist))]
    在此之后:
        [AutoMapFrom(typeof(Playlist))]
    */
    [AutoMapFrom(typeof(Playlist))]
    public class PlaylistInfo : MusicCollectionInfo
    {
        public bool IsHidden { get; set; }

        public bool IsRemovable { get; set; }

        public ImageSource PlaylistArt { get; set; }

    }
}