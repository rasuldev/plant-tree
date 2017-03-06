using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PlantTree.Data.Entities
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string ShortText { get; set; }
        [IgnoreDataMember]
        public virtual Image Photo { get; set; }

        public DateTime Date { get; set; }
        public int? ProjectId { get; set; }
        public virtual Project Project { get; set; }
    }
}