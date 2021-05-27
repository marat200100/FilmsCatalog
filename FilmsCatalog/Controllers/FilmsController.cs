using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FilmsCatalog.Data;
using FilmsCatalog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FilmsCatalog.Controllers
{
    public class FilmsController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FilmsController> _logger;

        public FilmsController(ApplicationDbContext context, ILogger<FilmsController> logger, UserManager<User> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int p = 1, int c = 10)
        {
            ViewData["prevPage"] = _context.Films.Take(p-1).Any() ? p-1 : "0";
            ViewData["nextPage"] = _context.Films.Skip(p * c).Any() ? p+1 : "0";
            var applicationDbContext = _context.Films.Skip((p-1)*c).Take(c).OrderByDescending(o => o.CreateDate).Include(f => f.Creator);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var film = await _context.Films
                .Include(f => f.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (film == null)
            {
                return NotFound();
            }

            return View(film);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Year,Director")] Film film, IFormFile img)
        {
            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {

                    var user = await _userManager.GetUserAsync(User);
                    if (img != null)
                    {
                        byte[] array = new byte[img.Length];
                        img.OpenReadStream().Read(array, 0, (int)img.Length);
                        film.Img = array;
                    }
                    film.CreatorId = user.Id;
                    _context.Add(film);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(film);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var film = await _context.Films.FindAsync(id);

                if (film.CreatorId == user.Id)
                {
                    if (film == null)
                    {
                        return NotFound();
                    }
                    return View(film);
                }
            }
            return ValidationProblem();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Img,Name,Description,Year,Director")] Film film, IFormFile img)
        {
            if (id != film.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated)
                {

                    var user = await _userManager.GetUserAsync(User);
                    if (img != null)
                    {
                        if (img.Length > 0)
                        {
                            byte[] array = new byte[img.Length];
                            img.OpenReadStream().Read(array, 0, (int)img.Length);
                            film.Img = array;
                        }
                    }
                    film.CreatorId = user.Id;
                    try
                    {
                        _context.Update(film);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!FilmExists(film.Id))
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
                return ValidationProblem();
            }
            return View(film);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var film = await _context.Films
                    .Include(f => f.Creator)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (film.CreatorId == user.Id)
                {
                    if (film == null)
                    {
                        return NotFound();
                    }

                    return View(film);
                }
            }
            return ValidationProblem();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var film = await _context.Films.FindAsync(id);
            _context.Films.Remove(film);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FilmExists(int id)
        {
            return _context.Films.Any(e => e.Id == id);
        }
    }
}
