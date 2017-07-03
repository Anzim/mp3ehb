using System;
using System.Linq;
using System.Threading.Tasks;
using mp3ehb.Entities;
using Microsoft.EntityFrameworkCore;

namespace mp3ehb.Helpers
{
    /// <summary>
    /// This helper class defines vairous extension methods
    /// </summary>	
    /// <Author>Andriy Zymenko</Author>
    public static class Extensions
    {
        public static async Task<string> GetCategoryPath(this IQueryable<Category> categories, int id)
        {
            var path = await categories.Where(c => c.Id == id).Select(c => c.Path).FirstOrDefaultAsync();
            return "/" + path;
        }

        public static string TrimTail(this string text, string tail = "?")
        {
            var index = text.IndexOf(tail, StringComparison.OrdinalIgnoreCase);
            var result = index > 0 ? text.Remove(index) : text;
            return result;
        }
    }
}