using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Common.Images
{
    /// <summary>
    /// Base class for handling images. To create image entity make descendant from this class.
    /// </summary>
    public class CommonImage //<TImageKind> where TImageKind: struct 
    {
        [NotMapped]
        public virtual string ImageDir => "/images";

        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageId { get; set; }
        public string ImageExt { get; set; }

        protected string UrlWithoutExtension => $"{ImageDir}/{ImageId}";
        public string UrlSmall => $"{UrlWithoutExtension}_small.{ImageExt}";
        public string Url => $"{UrlWithoutExtension}.{ImageExt}";

        public int Sort { get; set; } = 100;
    }
}