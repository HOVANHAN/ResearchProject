using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ResearchProject.Models;

namespace ResearchProject.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ResearchProjectUser class
public class ResearchProjectUser : IdentityUser
{
    public string Name { get; set; }


    public ICollection<Project> Projects { get; set; }
    public ICollection<Tasks> Tasks { get; set; }

    public ICollection<Document> Documents { get; set; }
    public ResearchProjectUser()
    {
        this.Projects = new HashSet<Project>();
        this.Tasks = new HashSet<Tasks>();
        this.Documents = new HashSet<Document>();
    }

}

