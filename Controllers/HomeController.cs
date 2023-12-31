﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ResearchProject.Areas.Identity.Data;
using ResearchProject.Models;
using System.Diagnostics;

namespace ResearchProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ResearchProjectContext _context;
        private readonly UserManager<ResearchProjectUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, ResearchProjectContext context, UserManager<ResearchProjectUser> userManager, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                // Get projects where the user is a member
                //var userProjects = _context.Projects
                //    .Where(p => p.ResearchProjectUsers.Any(rpu => rpu.Id == user.Id))
                //    .ToList();
                var userProjects = _context.Projects
                        .Where(p => p.ResearchProjectUsers.Any(rpu => rpu.Id == user.Id))
                        .Include(p => p.ResearchProjectUsers) // Include the users related to each project
                        .ToList();

                // get the invitation sent
                var sentInvitations = _context.invitations
                    .Where(inv => inv.SenderId == user.Id && !(inv.IsAccepted || inv.IsRejected))
                    .ToList();

                // Get invitations responsed
                var respondedInvitations = _context.invitations
                    .Where(inv => inv.SenderId == user.Id && (inv.IsAccepted || inv.IsRejected))
                    .ToList();
                // 
                var receivedInvitations = await _context.invitations
                    .Where(inv => inv.ReceiverId == user.Id && !(inv.IsAccepted || inv.IsRejected))
                    .ToListAsync();
                //
                // Combine and order the invitations by date
                var allInvitations = respondedInvitations
                    .Concat(receivedInvitations)
                    .OrderByDescending(inv => inv.ResponseDate ?? inv.SentDate) // Sort by ResponseDate if available, otherwise by SentDate
                    .ToList();
                //



                var isSender = true;
                var viewModel = new IndexViewModel
                {
                    IsSender = isSender,
                    UserProjects = userProjects,
                    SentInvitations = sentInvitations,
                    AllInvitations = allInvitations
                    
                 
                };

                return View(viewModel);
            }
            else
            {
                // Create an empty view model when the user is not logged in
                var emptyViewModel = new IndexViewModel
                {
                    UserProjects = new List<Project>(),
                    SentInvitations = new List<Invitation>(),
                    AllInvitations= new List<Invitation>()
                };

                return View(emptyViewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToProject(int projectId, string userEmail)
        {
            // Find the project by its ID
            var project = await _context.Projects.FindAsync(projectId);

            if (project != null)
            {
                // Find the user by email
                var receiver = await _userManager.FindByEmailAsync(userEmail);
                var sender = await _userManager.GetUserAsync(User);


                if (sender != null && receiver != null)
                {
                    // Check if the user is already a member of the project
                    var isMember = await _context.Projects
                        .Where(p => p.Id == projectId)
                        .AnyAsync(p => p.ResearchProjectUsers.Any(rpu => rpu.Id == receiver.Id));
                    if (!isMember)
                    {
                        // Create an invitation
                        var invitation = new Invitation
                        {
                            ProjectId = projectId,
                            SenderId = sender.Id, // Get the current user's ID
                            ReceiverId = receiver.Id,
                            SentDate = DateTime.Now,
                            SendMessage = $"{sender.Name} has invited you to join project {project.ProjectName} ",
                            IsAccepted = false // Invitation is not accepted yet
                        };

                        _context.invitations.Add(invitation);
                        await _context.SaveChangesAsync();
                        TempData["InvitationNotification"] = "Sent successfully";

                        return RedirectToAction("Details", new { id = projectId });
                    }
                    else
                    {
                        // User is already a member of the project

                        TempData["InvitationNotification"] = "The user is already a member of the project.";
                        return RedirectToAction("Details", new { id = projectId });
                    }
                }
                else
                {
                    // User with the provided email not found, handle accordingly
                    TempData["InvitationNotification"] = "The provided email address does not exist in our system.";
                    return RedirectToAction("Details", new { id = projectId });
                }
            }

            // Redirect back to the project details page 
            return RedirectToAction("Details", new { id = projectId });
        }


        [HttpPost]
        public async Task<IActionResult> AcceptInvitation(int invitationId)
        {
            // Find the invitation by its ID
            var invitation = await _context.invitations.FindAsync(invitationId);

            if (invitation != null)
            {
                var receiver = await _userManager.FindByIdAsync(invitation.ReceiverId);

                // Mark the invitation as accepted
                invitation.IsAccepted = true;
                invitation.ResponseDate = DateTime.Now;
                invitation.ResponseMessage = $"{receiver.Name} has accepted the invatation and joined the project";
                _context.invitations.Update(invitation);

                // Get the project associated with the invitation
                var project = await _context.Projects.FindAsync(invitation.ProjectId);

                if (project != null)
                {
                    // Add the user (Receiver) to the project
                    project.ResearchProjectUsers.Add(await _userManager.FindByIdAsync(invitation.ReceiverId));
                    await _context.SaveChangesAsync();

                    TempData["AcceptanceNotification"] = "You have accepted the invitation and joined the project.";

                    return RedirectToAction("Details", new { id = project.Id });
                }
            }

            // Redirect to a relevant page if the invitation cannot be accepted
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectInvitation(int invitationId)
        {
            // Find the invitation by its ID
            var invitation = await _context.invitations.FindAsync(invitationId);


            if (invitation != null)
            {
                var receiver = await _userManager.FindByIdAsync(invitation.ReceiverId);

                if (!invitation.IsAccepted && !invitation.IsRejected) // Check if not accepted or rejected
                {
                    var project = await _context.Projects.FindAsync(invitation.ProjectId);


                    // Mark the invitation as rejected
                    invitation.IsRejected = true;
                    invitation.ResponseDate = DateTime.Now;
                    invitation.ResponseMessage = $"'{receiver.Name}' has rejected your invitation to join project.";
                    _context.invitations.Update(invitation);
                    await _context.SaveChangesAsync();

                    TempData["InvitationNotification"] = "You have rejected the invitation.";


                    // Redirect back to the project details page or wherever you want
                    return RedirectToAction("Index");
                }
               
            }
            return RedirectToAction("Index");
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectName,Description,StartDate,EndDate")] Project project)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    // Create the project
                    var newProject = new Project
                    {
                        ProjectName = project.ProjectName,
                        Description = project.Description,
                        StartDate = project.StartDate,
                        EndDate = project.EndDate,
                        ResearchProjectUsers = new List<ResearchProjectUser> { user } // Add the user to the project
                    };

                    _context.Add(newProject);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(project);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            //var project = _context.Projects
            //    .Include(p => p.Tasks)
            //    .ThenInclude(t => t.ProjectUser)
            //    .FirstOrDefault(p => p.Id == id);
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .ThenInclude(t => t.ProjectUser)
                .Include(p => p.ResearchProjectUsers) // Include the members of the project
                .FirstOrDefaultAsync(p => p.Id == id);

            ViewData["ProjectId"] = id;

            var rejectedInvitations = _context.invitations
                .Where(inv => inv.ProjectId == id && inv.IsRejected)
                .ToList();
            if (project != null)
            {
                // Query for received invitations related to this project
                var receivedInvitations = _context.invitations.Where(inv => inv.ProjectId == id && !inv.IsAccepted).ToList();


                // Pass the project and received invitations to the view
                ViewBag.Project = project;
                ViewBag.ReceivedInvitations = receivedInvitations;
                ViewBag.InvitationNotification = TempData["InvitationNotification"];
                if (rejectedInvitations.Any())
                {
                    ViewBag.RejectedInvitations = rejectedInvitations;
                }

                return View(project);
            }
            return NotFound();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int Id, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Lấy dự án theo projectid
                var project = await _context.Projects.FindAsync(Id);
                var user = await _userManager.GetUserAsync(User);

                if (project != null)
                {
                    // Lưu tệp vào thư mục lưu trữ (webroot/File)
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "File", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Tạo một đối tượng Document và gắn nó với dự án
                    var document = new Document
                    {
                        DocumentName = fileName,
                        UploadDate = DateTime.Now,
                        path = filePath,
                        ResearchProjectUserId = user.Id
                    };

                    project.Documents.Add(document);
                    _context.Document.Add(document);
                    await _context.SaveChangesAsync();
                }
            }

            // Chuyển hướng lại trang chi tiết dự án sau khi tải tệp lên
            return RedirectToAction("Details", new { id = Id });
        }

        public IActionResult Document(int ProjectId)
        {
            // Lấy danh sách tài liệu thuộc dự án có projectId
            var documents = _context.Document
                .Where(d => d.Projects.Any(p => p.Id == ProjectId))
                .ToList();

            return View(documents);
        }
        public IActionResult Member(int ProjectId)
        {
            var project = _context.Projects
                .Include(p => p.ResearchProjectUsers) // Ensure ResearchProjectUsers are loaded
                .FirstOrDefault(p => p.Id == ProjectId);
        

            if (project == null)
            {
                return NotFound(); // Handle project not found
            }

            return View(project);

        }






        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}