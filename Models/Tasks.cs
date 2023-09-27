using ResearchProject.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResearchProject.Models
{
    public class Tasks
    {
        public int Id { get; set; }
        public string TaskName { get; set; }

        [StringLength(100, ErrorMessage = "Mô tả không được vượt quá 100 ký tự.")]
        public string? TaskDescription { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }

        public string ResearchProjectUserId { get;set; }
        public ResearchProjectUser  ProjectUser { get; set; }
       
        public int ProjectId { get; set; }
        public Project Project{ get; set; }
    }
}
