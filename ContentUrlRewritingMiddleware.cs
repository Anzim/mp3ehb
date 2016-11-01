using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using mp3ehb.core1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace mp3ehb.core1
{
    public class ContentUrlRewritingMiddleware
    {
        private readonly RequestDelegate _next;

        public ContentUrlRewritingMiddleware(RequestDelegate next) 
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, Mp3EhbContext dbContext)
        {
            var path = context.Request.Path.ToUriComponent();
            //if (_dbContext == null) _dbContext = new Mp3EhbContext();

            var transformResult = await TransformPathAsync(path, dbContext, context.Items);
            //If is an category, change the request path to be "Content/Index/{path}" so it is handled by the content controller, index action
            switch (transformResult.Action)
            {
                case TransformAction.Substitute:
                    //Let the next middleware (MVC routing) handle the request
                    //In case the path was updated, the MVC routing will see the updated path
                    context.Request.Path = transformResult.NewPath;
                    break;
                case TransformAction.Redirect:
                    context.Response.StatusCode = 302;
                    context.Response.Headers.Add("Location", transformResult.NewPath);
                    return;
                case TransformAction.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            await _next.Invoke(context);
        }

        private async Task<TransformResult> TransformPathAsync(string path, Mp3EhbContext dbContext, IDictionary<object, object> contextItems)
        {
            //The middleware will try to find a Category and a content in the database that matches the current path
            var transformResult = new TransformResult();
            var trimmedPath = TrimTail(path, tail: ".html");
            var names = trimmedPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length == 0) return transformResult; //None

            string bestMatchingPath = null;
            string correctPath = null;
            var contentId = default(int);
            var bestMatchingCategoryId = default(int);

            var categores = dbContext.Categories.AsNoTracking();
            foreach (var name in names)
            {
                var categoryId = default(int);
                var currentPath = name;
                if (bestMatchingPath == null) //first entry
                {
                    if (MenuDictionary.ContainsKey(name)) categoryId = MenuDictionary[name];
                }
                else
                {
                    currentPath = bestMatchingPath + "/" + name;
                }

                //let check if name is in idNumber-aliasString format
                var match = Regex.Match(name, @"^(\d+)-(.*)$");
                Expression<Func<Category, bool>> categoryPredicate = c => c.Path == currentPath;
                int id = 0;
                if (match.Success && match.Groups.Count > 2)
                {
                    id = int.Parse(match.Groups[1].Value);
                    //name = match.Groups[2].Value;
                    categoryPredicate = c => c.Id == id; // && c.Alias == name;
                }

                if (categoryId == default(int))
                {
                    //The midleware will try to find a Category in the database that matches the current Path
                    categoryId = await categores
                        .Where(categoryPredicate)
                        .Select(c => c.Id)
                        .FirstOrDefaultAsync();
                }
                if (categoryId == default(int)) //name may be content name
                {
                    Expression<Func<Content, bool>> contentPredicate = c => c.Alias.Contains(name);
                    if (id != 0)
                    {
                        contentPredicate = c => c.Id == id;// || c.Alias.Contains(name);
                    }
                    //The midleware will try to find a content in the database that matches the current name
                    var contents = dbContext.Contents.AsNoTracking();
                    var content = await contents
                        .Where(contentPredicate)
                        .Select(c => new { c.Id, c.CatId, c.Alias })
                        .FirstOrDefaultAsync();
                    if (content == null) break;
                    contentId = content.Id;
                    correctPath = await GetCategoryPath(categores, content.CatId) + "/" + content.Alias;
                    break;
                }
                bestMatchingPath = currentPath;
                bestMatchingCategoryId = categoryId;
            }

            if (contentId != default(int))
            {
                contextItems["ContentId"] = contentId;
                if (correctPath == path)
                {
                    return new TransformResult("/Home/Content/" + contentId, TransformAction.Substitute);
                }
            }
            else if (bestMatchingCategoryId != default(int))
            {
                contextItems["CategoryId"] = bestMatchingCategoryId;
                correctPath = await GetCategoryPath(categores, bestMatchingCategoryId);
                if (correctPath == path)
                {
                    return new TransformResult("/Home/Category/" + bestMatchingCategoryId, TransformAction.Substitute);
                }
            }
            else return transformResult; //None
            return new TransformResult(correctPath, TransformAction.Redirect);
        }

        private static async Task<string> GetCategoryPath(IQueryable<Category> categories, int id)
        {
            var path = await categories.Where(c => c.Id == id).Select(c => c.Path).FirstOrDefaultAsync();
            return "/" + path;
        }

        private static string TrimTail(string text, string tail = "?")
        {
            var index = text.IndexOf(tail, StringComparison.OrdinalIgnoreCase);
            var result = index > 0 ? text.Remove(index) : text;
            return result;
        }

        private static readonly Dictionary<string, int> MenuDictionary = new Dictionary<string, int>
        {
            ["hristianskie-knigi"] = 43,
            ["msc-ehb"] = 55,
            ["voskresnaya-shkola"] = 45,
            ["traktaty"] = 46,
            ["stories"] = 41,
            ["textbooks"] = 37,
            ["verses"] = 39,
            ["scores"] = 38,
            ["sites"] = 35,
            ["materials"] = 53,
            ["the-news"] = 52,
            ["pomosch"] = 53,
            ["ustav-msc-ehb"] = 55,
            ["msc-ehb-music"] = 42,
            ["novosti-msc-ehb"] = 44,
            ["slidefilms"] = 40,
            ["downloads"] = 1,
            ["jdownloads"] = 1
        };
    }

    internal class TransformResult
    {
        public string NewPath { get; }
        public TransformAction Action { get; }

        public TransformResult(string newPath = null, TransformAction action = TransformAction.None)
        {
            NewPath = newPath;
            Action = action;
        }
    }

    internal enum TransformAction
    {
        None,
        Redirect,
        Substitute
    }
}
