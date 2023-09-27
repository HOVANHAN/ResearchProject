using ResearchProject.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace ResearchProject.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        public string DocumentName { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime UploadDate { get; set; }

        public string path { get; set; }

        public string ResearchProjectUserId { get; set; }
        public ResearchProjectUser ResearchProjectUser { get; set; }

        public ICollection<Project> Projects { get; set; }
        public Document()
        {
            this.Projects = new HashSet<Project>();
            UploadDate = DateTime.Now;
        }
    }
}
