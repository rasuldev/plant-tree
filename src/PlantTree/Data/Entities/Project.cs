using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using PlantTree.Infrastructure.Common;

namespace PlantTree.Data.Entities
{
    public class Project
    {
        public int Id { get; set; }
        [Required, Display(Name="Название проекта")]
        public string Name { get; set; } 
        public string Tag { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public int Goal { get; set; } = 0;
        public int Reached { get; set; } = 0;
        public decimal TreePrice { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ReachedDate { get; set; }
        public DateTime FinishedDate { get; set; }

        [IgnoreDataMember]
        public Currency Currency { get; set; } = Currency.Ruble;
        public bool Deleted { get; set; } = false;
        //var likeUserIds: [Int] = []
        //var photos: [UIImage] = []
        [IgnoreDataMember]
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }
        public int? ImageId { get; set; }
        [ForeignKey("ImageId"), IgnoreDataMember]
        public virtual Image MainImage { get; set; }
        [IgnoreDataMember]
        public virtual ICollection<Image> OtherImages { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<Transaction> Transactions { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<News> News { get; set; }
        
        // for json
        [NotMapped]
        public string MainImageUrl => MainImage?.Url != null ? GlobalConf.Host + MainImage?.Url : "";

        [NotMapped]
        public string MainImageUrlSmall => MainImage?.UrlSmall != null ? GlobalConf.Host + MainImage?.UrlSmall : "";

        [NotMapped]
        public IEnumerable<string> OtherImagesUrl => OtherImages?.Select(i => GlobalConf.Host + i.Url);
        [NotMapped]
        public IEnumerable<string> OtherImagesUrlSmall => OtherImages?.Select(i => GlobalConf.Host + i.UrlSmall);

        [NotMapped]
        public string CurrencyName => Currency.ToString();

        [NotMapped]
        public int? LikesCount => ProjectUsers?.Count;

        /// <summary>
        /// For authorized requests show whether this project was liked by current user
        /// </summary>
        [NotMapped]
        public bool IsLiked { get; set; } = false;
        public int DonatorsCount { get; set; }

        // ShouldSerialize - conditional property serialization
    }

    public enum ProjectStatus
    {
        InProgress, Finished
    }
}