using System.Linq;
using System.Threading.Tasks;
using mp3ehb.core1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace mp3ehb.core1.Controllers
{
    public class HomeController : Controller
    {
        public Mp3EhbContext Db { get; set; }
        //private DbContext _db;
        //protected DbContext Db
        //{
        //    get { return _db; }//?? (_db = new Mp3EhbContext()); }
        //    set { _db = value; }
        //}

        public HomeController(Mp3EhbContext dbContext)
        {
            Db = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Content(int id)
        {
            var contents = Db.Contents.AsNoTracking();
            var contentList = await contents
                .FirstOrDefaultAsync(c => c.Id == id);
                //.Select(content => new ContentLink { Title = content.Title, Alias = content.Alias })
                //.ToListAsync();

            return View(contentList);
        }

        public async Task<IActionResult> Category(int id)
        {
            var categories = Db.Categories.AsNoTracking();
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
            if (category == null) return Error($"Category Id={id} is not found");

            var contents = Db.Set<Content>().AsNoTracking();
            category.Contents = await contents
                .Where(c => c.CatId == id)
                .Select(content => new ContentLink { Title = content.Title, Alias = content.Alias })
                .ToListAsync();

            return View(category);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error(string message)
        {
            ViewData["Message"] = message;
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            //if (disposing && _db != null)
            //{
            //    _db.Dispose();
            //    _db = null;
            //}
            base.Dispose(disposing);
        }
    }
}
