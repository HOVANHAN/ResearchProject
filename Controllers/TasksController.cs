using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using ResearchProject.Areas.Identity.Data;
using ResearchProject.Models;

namespace ResearchProject.Controllers
{
    public class TasksController : Controller
    {
        private readonly ResearchProjectContext _context;
        private readonly UserManager<ResearchProjectUser> _userManager;

        public TasksController(ResearchProjectContext context, UserManager<ResearchProjectUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
           var user = await _userManager.GetUserAsync(User);

            //var researchProjectContext = _context.Tasks.Include(t => t.Project).Include(t => t.ProjectUser);
            //return View(await researchProjectContext.ToListAsync());
            if(user !=null)
            {

                //var projects = await _context.Projects
                //    .Where(p => p.ResearchProjectUsers.Any(rpu => rpu.Id == user.Id))
                //    .Include(p => p.Tasks) // Include the tasks related to each project
                //    .ToListAsync();
                var projects = await _context.Projects
                    .Where(p => p.ResearchProjectUsers.Any(rpu => rpu.Id == user.Id))
                    .Include(p => p.Tasks) // Include the tasks related to each project
                    .ThenInclude(t => t.ProjectUser) // Include the user assigned to each task
                    .ToListAsync();

                return View(projects);
            }
            else
            {
                var emptyList = new List<Models.Project>();
                return View(emptyList);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCompletionStatus(int id, bool isCompleted)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            // Check if the user is authorized to update the task
            var user = await _userManager.GetUserAsync(User);
            if (task.ResearchProjectUserId != user.Id)
            {
                // The user is not authorized to update this task
                return Forbid(); // You can customize the response based on your requirements
            }

            // Update the completion status
        
                task.IsCompleted= true;
         

            try
            {
                // Save changes to the database
                _context.Update(task);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                
                throw;
            }

            // Redirect back to the task list or the original page
            return RedirectToAction(nameof(Index));
        }



        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tasks == null)
            {
                return NotFound();
            }

            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.ProjectUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tasks == null)
            {
                return NotFound();
            }

            return View(tasks);
        }

        // GET: Tasks/Create
        public IActionResult Create(int ProjectId)
        {
            // ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Id");
            //ViewData["ResearchProjectUserId"] = new SelectList(_context.researchProjectUsers, "Id", "Id");
            // ViewData["ProjectId"] = projectId.ToString();
            ViewBag.ProjectId = ProjectId;
            var projectMembers = _context.Projects
                .Where(p => p.Id == ProjectId)
                .SelectMany(p => p.ResearchProjectUsers)
                .ToList();

            // Create a SelectList for the dropdown with user ID as the value and user name as the text.
            ViewData["ResearchProjectUserId"] = new SelectList(projectMembers, "Id", "UserName");

            return View();
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TaskName,TaskDescription,StartDate,EndDate,IsCompleted,ResearchProjectUserId,ProjectId")] Tasks tasks)
        {
            var projectId = int.Parse(Request.Form["ProjectId"]);
            tasks.ProjectId = projectId;
            _context.Add(tasks);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Id", tasks.ProjectId);
            //ViewData["ResearchProjectUserId"] = new SelectList(_context.researchProjectUsers, "Id", "Id", tasks.ResearchProjectUserId);
            //return View(tasks);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tasks == null)
            {
                return NotFound();
            }

            var tasks = await _context.Tasks.FindAsync(id);
            if (tasks == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Id", tasks.ProjectId);
            ViewData["ResearchProjectUserId"] = new SelectList(_context.researchProjectUsers, "Id", "Id", tasks.ResearchProjectUserId);
            return View(tasks);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TaskName,TaskDescription,StartDate,EndDate,IsCompleted,ResearchProjectUserId,ProjectId")] Tasks tasks)
        {
            if (id != tasks.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tasks);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TasksExists(tasks.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Id", tasks.ProjectId);
            ViewData["ResearchProjectUserId"] = new SelectList(_context.researchProjectUsers, "Id", "Id", tasks.ResearchProjectUserId);
            return View(tasks);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tasks == null)
            {
                return NotFound();
            }

            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.ProjectUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tasks == null)
            {
                return NotFound();
            }

            return View(tasks);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tasks == null)
            {
                return Problem("Entity set 'ResearchProjectContext.Tasks'  is null.");
            }
            var tasks = await _context.Tasks.FindAsync(id);
            if (tasks != null)
            {
                _context.Tasks.Remove(tasks);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TasksExists(int id)
        {
          return (_context.Tasks?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
