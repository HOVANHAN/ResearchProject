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
    public class DocumentsController : Controller
    {
        private readonly ResearchProjectContext _context;
        private readonly UserManager<ResearchProjectUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public DocumentsController(ResearchProjectContext context, UserManager<ResearchProjectUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: Documents
        //public async Task<IActionResult> Index()
        //{ //var user = await _userManager.GetUserAsync(User);
        //    var researchProjectContext = _context.Document.Include(d => d.ResearchProjectUser);
        //    return View(await researchProjectContext.ToListAsync());
        //}

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var userDocuments = _context.Document
                .Where(d => d.ResearchProjectUser.Id == user.Id)
                .ToList();

            var sharedProjectIds = _context.Projects
                .Where(p => p.ResearchProjectUsers.Any(rpu => rpu.Id == user.Id))
                .Select(p => p.Id)
                .ToList();

            var coWorkerDocuments = _context.Document
                .Where(d => sharedProjectIds.Contains(d.Id) && d.ResearchProjectUser.Id != user.Id)
                .ToList();

            var allDocuments = userDocuments.Concat(coWorkerDocuments).ToList();

            return View(allDocuments);
        }



        // GET: Documents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Document == null)
            {
                return NotFound();
            }

            var document = await _context.Document
                .Include(d => d.ResearchProjectUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Documents/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user != null)
            {
                if(file !=null )
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "File", fileName);

                    await using var memoryStream = new MemoryStream();
                    await file.OpenReadStream().CopyToAsync(memoryStream);


                    using(var stream = new FileStream(filePath, FileMode.Create))
                    {
                        memoryStream.Position = 0;
                             await memoryStream.CopyToAsync(stream);
                     }
                    var document = new Document
                    {
                        DocumentName = file.FileName,
                        UploadDate = DateTime.Now,
                        path = filePath,
                        ResearchProjectUserId = user.Id
                    };
                    _context.Document.Add(document);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError("File", "Please select a file to upload.");
                    return View();
                }
            }
            

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Download(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var document = await _context.Document.FindAsync(id);

                if (document == null)
                {
                    return NotFound();
                }

                // Xây dựng đường dẫn ảo đến thư mục "wwwroot" và tệp tải lên
                var wwwrootPath = Path.Combine(_env.WebRootPath, document.path);


                // Kiểm tra xem tệp có tồn tại không
                if (System.IO.File.Exists(wwwrootPath))
                {
                    // Đọc dữ liệu từ tệp và trả về như một tệp tải về
                    var fileBytes = System.IO.File.ReadAllBytes(wwwrootPath);
                    return File(fileBytes, "application/pdf", document.DocumentName);
                }
            }

            return RedirectToAction("Index");
        }


        // GET: Documents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Document == null)
            {
                return NotFound();
            }

            var document = await _context.Document.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            ViewData["ResearchProjectUserId"] = new SelectList(_context.researchProjectUsers, "Id", "Id", document.ResearchProjectUserId);
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DocumentName,UploadDate,path,ResearchProjectUserId")] Document document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
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
            ViewData["ResearchProjectUserId"] = new SelectList(_context.researchProjectUsers, "Id", "Id", document.ResearchProjectUserId);
            return View(document);
        }

        // GET: Documents/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.Document == null)
        //    {
        //        return NotFound();
        //    }

        //    var document = await _context.Document
        //        .Include(d => d.ResearchProjectUser)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (document == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(document);
        //}

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Document == null)
            {
                return Problem("Entity set 'ResearchProjectContext.Document'  is null.");
            }
            var document = await _context.Document.FindAsync(id);
            if (document != null)
            {
                _context.Document.Remove(document);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocumentExists(int id)
        {
          return (_context.Document?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
