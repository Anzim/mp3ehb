using System.Linq;
using System.Threading.Tasks;
using mp3ehb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace mp3ehb.Controllers
{
    public class HomeController : Controller
    {
        protected Mp3EhbContext Db { get; set; }

        public HomeController(Mp3EhbContext dbContext)
        {
            this.Db = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Content(int id)
        {
            var contents = this.Db.Contents.AsNoTracking();
            var contentList = await contents
                .FirstOrDefaultAsync(c => c.Id == id);

            return View(contentList);
        }

        public async Task<IActionResult> Category(int id)
        {
            var categories = this.Db.Categories.AsNoTracking();
            var category = await categories
                .Where(c => c.Id == id)
                .Include(c => c.Children)
                .Select(c => new CategoryLink
                {
                    Title = c.Title,
                    Description = c.Description,
                    Path = c.Path,
                    Children = c.Children.Where(ch => ch.Published).Select(child => new CategoryLink
                    {
                        Alias = child.Alias,
                        Title = child.Title,
                        Path = child.Path
                    })
                })
                .FirstOrDefaultAsync();
            if (category == null) return this.Error($"Category Id={id} is not found");

            var contents = this.Db.Set<Content>().AsNoTracking();
            category.Contents = await contents
                .Where(c => c.CatId == id)
                .Select(content => new ContentLink { Title = content.Title, Alias = content.Alias })
                .ToListAsync();

            return View(category);
        }

        public IActionResult Error(string message)
        {
            ViewData["Message"] = message;
            return View();
        }

    }
}
