using System.Linq;
using System.Threading.Tasks;
using mp3ehb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace mp3ehb.Controllers
{
    /// <summary>
    /// Standard mvc core controller for basic actions and redirected content
    /// </summary>
    public class HomeController : Controller
    {
        private const string VIEWDATA_MESSAGE_KEY = "Message";
        private const string OUR_APPLICATION_DESCRIPTION = "Here wll be our application description page.";
        private const string OUR_CONTACT_PAGE_MESSAGE = "Here will be our contact page.";
        private const string CATEGORY_ID_IS_NOT_FOUND_MESSAGE = "Category Id={0} is not found";

        /// <summary>
        /// The main app storage (UOW) <see cref="Mp3EhbContext"/>,
        /// <seealso cref="DbContext"/>
        /// </summary>
        protected Mp3EhbContext Db { get; set; }

        /// <summary>
        /// Home controller constructor
        /// </summary>
        /// <param name="dbContext">The <see cref="Mp3EhbContext"/> instance,
        /// <seealso cref="DbContext"</param>
        public HomeController(Mp3EhbContext dbContext)
        {
            this.Db = dbContext;
        }

        /// <summary>
        /// Home page
        /// </summary>
        /// <returns>The <see cref="ViewResult"/> instance</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Gets content page by id. 
        /// <seealso cref="ContentUrlRewritingMiddleware"/> that redirects to this action
        /// </summary>
        /// <param name="id">The category id</param>
        /// <returns>The <see cref="Task{ViewResult}"/> instance</returns>
        public async Task<IActionResult> Content(int id)
        {
            var contents = this.Db.Contents.AsNoTracking();
            var contentList = await contents
                .FirstOrDefaultAsync(c => c.Id == id);

            return View(contentList);
        }

        /// <summary>
        /// Category page by id    
        /// </summary>
        /// <seealso cref="ContentUrlRewritingMiddleware"/> that redirects to this action
        /// <param name="id">The category id</param>
        /// <returns>The <see cref="Task{ViewResult}"/> instance</returns>
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

            if (category == null)
            {
                return this.Error(string.Format(CATEGORY_ID_IS_NOT_FOUND_MESSAGE, id));
            }

            var contents = this.Db.Set<Content>().AsNoTracking();
            category.Contents = await contents
                .Where(c => c.CatId == id)
                .Select(content => new ContentLink { Title = content.Title, Alias = content.Alias })
                .ToListAsync();

            return View(category);
        }

        /// <summary>
        /// About page action
        /// </summary>
        /// <returns>The <see cref="ViewResult"/> instance</returns>
        public ViewResult About()
        {
            ViewData[VIEWDATA_MESSAGE_KEY] = OUR_APPLICATION_DESCRIPTION;

            return View();
        }

        /// <summary>
        /// Contact page action
        /// </summary>
        /// <returns>The <see cref="ViewResult"/> instance</returns>
        public ViewResult Contact()
        {
            ViewData[VIEWDATA_MESSAGE_KEY] = OUR_CONTACT_PAGE_MESSAGE;

            return View();
        }

        /// <summary>
        /// Error page action
        /// </summary>
        /// <returns>The <see cref="ViewResult"/> instance</returns>
        public IActionResult Error(string message)
        {
            ViewData[VIEWDATA_MESSAGE_KEY] = message;
            return View();
        }

    }
}
