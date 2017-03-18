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
        [Display(Name="Описание")]
        public string Description { get; set; }
        [Display(Name = "Краткое описание")]
        public string ShortDescription { get; set; }
        [Display(Name = "Цель")]
        public int Goal { get; set; } = 0;
        [Display(Name = "Получено")]
        public int Reached { get; protected set; } = 0;
        [Display(Name = "Стоимость одного дерева")]
        public decimal TreePrice { get; set; }
        [Display(Name = "Статус")]
        public ProjectStatus Status { get; set; } = ProjectStatus.InProgress;
        public DateTime CreationDate { get; set; } = DateTime.Now;
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

        public void AddTrees(int treeCount)
        {
            Reached += treeCount;
            if (Reached >= Goal)
            {
                Status = ProjectStatus.Reached;
                ReachedDate = DateTime.Now;
            }
        }
    }

    public enum ProjectStatus
    {
        InProgress = 10, Reached = 50, Finished = 100
    }
}