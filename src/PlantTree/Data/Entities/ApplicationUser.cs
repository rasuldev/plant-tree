using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using AuthTokenServer.ExternalLogin;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PlantTree.Infrastructure.Common;

namespace PlantTree.Data.Entities
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }
        [IgnoreDataMember]
        public virtual ICollection<Transaction> Transactions { get; set; }

        public string Name { get; set; }
        public string LastName { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public decimal Donated { get; set; }
        public int DonatedProjectsCount { get; set; }
        public bool Deleted { get; set; }
        [ForeignKey("Photo")]
        public int? PhotoId { get; set; }
        [IgnoreDataMember]
        public virtual Image Photo { get; set; }
        // for json
        [NotMapped]
        public string PhotoUrl => Photo?.Url != null ? GlobalConf.Host + Photo?.Url : "";
        [NotMapped]
        public string PhotoUrlSmall => Photo?.UrlSmall != null ? GlobalConf.Host + Photo?.UrlSmall : "";
    }
}
