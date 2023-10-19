using ResearchProject.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ResearchProject.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        public ICollection<ResearchProjectUser> ResearchProjectUsers { get; set; }

        public ICollection<Tasks> Tasks { get; set; }

        public ICollection<Document> Documents { get; set; }

        public ICollection<Invitation> Invations { get; set; }
        public Project()
        {
            this.ResearchProjectUsers = new HashSet<ResearchProjectUser>();
            this.Tasks = new HashSet<Tasks>();
            this.Documents = new HashSet<Document>();
            this.Invations = new HashSet<Invitation>();
        }
    }
}
