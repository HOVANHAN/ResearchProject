using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ResearchProject.Areas.Identity.Data;
using ResearchProject.Models;

namespace ResearchProject.Controllers
{
    public class InvitationsController : Controller
    {
        private readonly ResearchProjectContext _context;
        private readonly UserManager<ResearchProjectUser> _userManager;

        public InvitationsController(ResearchProjectContext context, UserManager<ResearchProjectUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Invitations
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var invitations = _context.invitations
                    .Where(inv => (inv.SenderId == user.Id && (inv.IsAccepted || inv.IsRejected)) ||
                                  (inv.ReceiverId == user.Id && !(inv.IsAccepted || inv.IsRejected)))
                    .ToList();
                return View(invitations);
                    
            }
            else
                return NotFound();

        }

        // GET: Invitations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.invitations == null)
            {
                return NotFound();
            }

            var invitation = await _context.invitations
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invitation == null)
            {
                return NotFound();
            }

            return View(invitation);
        }

        // GET: Invitations/Create
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Id");
            return View();
        }

        // POST: Invitations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SenderId,ReceiverId,IsAccepted,IsRejected,SentDate,SendMessage,ResponseDate,ResponseMessage,ProjectId")] Invitation invitation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(invitation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Id", invitation.ProjectId);
            return View(invitation);
        }

        // GET: Invitations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.invitations == null)
            {
                return NotFound();
            }

            var invitation = await _context.invitations.FindAsync(id);
            if (invitation == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Id", invitation.ProjectId);
            return View(invitation);
        }

        // POST: Invitations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SenderId,ReceiverId,IsAccepted,IsRejected,SentDate,SendMessage,ResponseDate,ResponseMessage,ProjectId")] Invitation invitation)
        {
            if (id != invitation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invitation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvitationExists(invitation.Id))
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
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Id", invitation.ProjectId);
            return View(invitation);
        }

        // GET: Invitations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.invitations == null)
            {
                return NotFound();
            }

            var invitation = await _context.invitations
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invitation == null)
            {
                return NotFound();
            }

            return View(invitation);
        }

        // POST: Invitations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.invitations == null)
            {
                return Problem("Entity set 'ResearchProjectContext.invitations'  is null.");
            }
            var invitation = await _context.invitations.FindAsync(id);
            if (invitation != null)
            {
                _context.invitations.Remove(invitation);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InvitationExists(int id)
        {
          return (_context.invitations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
