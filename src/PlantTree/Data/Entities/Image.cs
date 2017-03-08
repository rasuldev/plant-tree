using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Threading.Tasks;
using Common.Images;
using Common.Services;
using Microsoft.AspNetCore.Http;


namespace PlantTree.Data.Entities
{
    public class Image : CommonImage
    {
        public Image()
        {
            
        }
        public Image(string title, ImageKind kind)
        {
            Kind = kind;
            Title = title;
        }

        /// <summary>
        /// Kind of image. For example, it can be project or user image. 
        /// This property we can use to keep images in different folders depending of image kind.
        /// </summary>
        public ImageKind Kind { get; set; }
        [NotMapped]
        public override string ImageDir
        {
            get
            {
                switch ((ImageKind)this.Kind)
                {
                    case ImageKind.Common:
                        return "/images";
                    case ImageKind.Project:
                        return "/images/projects";
                    case ImageKind.User:
                        return "/images/users";
                    default:
                        return "/images";
                }
            }
        }

        public int? ProjectId { get; set; }
        public Project Project { get; set; }

        /// <summary>
        /// Projects that have this image as main
        /// </summary>
        //public ICollection<Project> Projects { get; set; }

    }
}