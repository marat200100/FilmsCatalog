using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Models
{
    public class Film
    {
        public int Id { get; set; }
        public byte[] Img { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public string Director { get; set; }
        public string CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public virtual User Creator { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
