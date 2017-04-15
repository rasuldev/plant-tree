using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using PlantTree.Infrastructure.Common;

namespace PlantTree.Data.Entities
{
    public class News
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Text { get; set; }
        [Required]
        public string ShortText { get; set; }
        [ForeignKey("Photo")]
        public int? PhotoId { get; set; }
        [IgnoreDataMember]
        public virtual Image Photo { get; set; }
        // for json
        [NotMapped]
        public string PhotoUrl => Photo?.Url != null ? GlobalConf.Host + Photo?.Url : "";
        [NotMapped]
        public string PhotoUrlSmall => Photo?.UrlSmall != null ? GlobalConf.Host + Photo?.UrlSmall : "";

        public DateTime Date { get; set; } = DateTime.Now;
        public int? ProjectId { get; set; }
        [IgnoreDataMember]
        public virtual Project Project { get; set; }
    }
}