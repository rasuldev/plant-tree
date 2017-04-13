using System.Collections.Generic;
using AuthTokenServer.ExternalLogin;
using PlantTree.Data.Entities;

namespace PlantTree.Models.Api
{
    public class DetailedUserInfo
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Birthday { get; set; }
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public decimal Donated { get; set; }
        public int DonatedProjectsCount { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public string PhotoUrlSmall { get; set; }
        public string PhotoUrl { get; set; }
    }
}