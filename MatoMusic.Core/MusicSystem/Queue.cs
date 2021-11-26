using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatoMusic.Core.MusicSystem
{
    public class Queue : FullAuditedEntity<long>
    {
        public Queue(string musicTitle, int rank, int musicInfoId)
        {
            MusicTitle = musicTitle;
            Rank = rank;
            MusicInfoId = musicInfoId;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override long Id { get; set; }

        public int MusicInfoId { get; set; }

        public int Rank { get; set; }

        public string MusicTitle { get; set; }
    }
}
