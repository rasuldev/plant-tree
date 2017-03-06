namespace PlantTree.Data.Entities
{
    public class ProjectUser
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; } 
    }
}