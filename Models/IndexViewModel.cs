using Microsoft.EntityFrameworkCore.Metadata;

namespace ResearchProject.Models
{
    public class IndexViewModel
    {
        public List<Project> UserProjects { get; set; }
        public List<Invitation> SentInvitations { get; set; }
        //public List<Invitation> RespondedInvitations { get; set; }
        
        //public List<Invitation> ReceivedInvitations { get; set; }
        public List<Invitation> AllInvitations { get; set; }
        public bool IsSender { get; set; }
    }
}
