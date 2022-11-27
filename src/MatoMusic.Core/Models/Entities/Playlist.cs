using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatoMusic.Core.Models.Entities
{
    public class Playlist : FullAuditedEntity<long>
    {
        public Playlist()
        {

        }
        public Playlist(string name, bool isHidden, bool isRemovable)
        {
            Title = name;
            IsHidden = isHidden;
            IsRemovable = isRemovable;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }
        public string Title { get; set; }

        public bool IsHidden { get; set; }

        public bool IsRemovable { get; set; }


        /* 项目“MatoMusic.Core (net6.0-windows10.0.19041)”的未合并的更改
        在此之前:
                public ICollection<PlaylistItem.PlaylistItem> PlaylistItems { get; set; }
        在此之后:
                public ICollection<PlaylistItem> PlaylistItems { get; set; }
        */
        public ICollection<PlaylistItem> PlaylistItems { get; set; }

    }
}
