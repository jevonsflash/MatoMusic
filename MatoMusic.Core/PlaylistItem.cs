using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace MatoMusic.Core
{
    public class PlaylistItem : FullAuditedEntity<long>
    {
        public PlaylistItem(int playlistId, string musicTitle, int rank)
        {
            PlaylistId = playlistId;
            MusicTitle = musicTitle;
            Rank = rank;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }

        public int Rank { get; set; }

        public int PlaylistId { get; set; }
        [ForeignKey("PlaylistId")]

        /* 项目“MatoMusic.Core (net6.0-windows10.0.19041)”的未合并的更改
        在此之前:
                public Playlist.Playlist Playlist { get; set; }
        在此之后:
                public Playlist Playlist { get; set; }
        */
        public Playlist Playlist { get; set; }
        public string MusicTitle { get; set; }

        public int MusicInfoId { get; set; }


    }
}
