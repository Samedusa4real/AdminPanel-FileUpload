using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PustokTemplate.DAL;
using PustokTemplate.Models;
using PustokTemplate.ViewModels;

namespace PustokTemplate.Areas.Manage.Controllers
{
    [Area("manage")]
    public class SliderController : Controller
    {
        private readonly PustokDbContext _context;

        public SliderController(PustokDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int page=1)
        {
            var query = _context.Sliders.OrderBy(x=>x.Order).AsQueryable();

            return View(PaginatedList<Slider>.Create(query, page, 3));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Slider slider)
        {
            if (!ModelState.IsValid)
                return View();

            if(slider.ImageFile.ContentType!="image/jpeg" && slider.ImageFile.ContentType != "image/png")
            {
                ModelState.AddModelError("ImageFile", "Content type must includes jpeg or png");
                return View();
            }

            if (slider.ImageFile.Length > 2097152)
            {
                ModelState.AddModelError("ImageFile", "File size cant be higher than 2mb");
                return View();
            }

            var existingSlider = _context.Sliders.FirstOrDefault(x => x.Order == slider.Order);

            if (existingSlider != null)
            {
                ModelState.AddModelError("Order", "The order must be unique.");
                return View();
            }

            var minOrder = _context.Sliders.Min(x => x.Order);

            if (slider.Order < minOrder)
            {
                var slidersToUpdate = _context.Sliders.Where(x => x.Order >= slider.Order && x.Id != existingSlider.Id).ToList();

                foreach (var item in slidersToUpdate)
                {
                    item.Order++;
                }
            }

            slider.Url = slider.ImageFile.Name;

            _context.Sliders.Add(slider);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        
    }
}
