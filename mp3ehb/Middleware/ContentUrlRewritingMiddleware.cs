using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using mp3ehb.Entities;
using mp3ehb.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace mp3ehb.Middleware
{
    /// <summary>
    /// The middleware for routing hierarchical category structure
    /// It transforms tree folder structure to MVC pipeline
    /// </summary>
    /// <Author>Andriy Zymenko</Author>
    public class ContentUrlRewritingMiddleware
    {
        #region Private constants

        private const string HOME_CONTENT_URL = "/Home/Content/";
        private const string HOME_CATEGORY_URL = "/Home/Category/";
        private const string CATEGORY_REGEX_PATTERN = @"^(\d+)-(.*)$";
        private const string CATEGORY_ID = "CategoryId";
        private const string DEFAULT_EXTENSION = ".html";
        private const string LOCATION_HEADER_NAME = "Location";
        private const char FOLDER_SEPARATOR_CHAR = '/';

        #endregion

        private enum TransformAction
        {
            None,
            Redirect,
            Substitute
        }

        private class TransformResult
        {
            public string NewPath { get; }
            public TransformAction Action { get; }

            public TransformResult(string newPath = null, TransformAction action = TransformAction.None)
            {
                this.NewPath = newPath;
                this.Action = action;
            }
        }

        private readonly RequestDelegate _next;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="next">The <see cref="RequestDelegate"/> instance</param>
        public ContentUrlRewritingMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task Invoke(HttpContext context, Mp3EhbContext dbContext)
        {
            var path = context.Request.Path.ToUriComponent();
            var transformResult = await this.TransformPathAsync(path, dbContext, context.Items);

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
                    context.Response.Headers.Add(LOCATION_HEADER_NAME, transformResult.NewPath);
                    return;
                case TransformAction.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await this._next.Invoke(context);
        }

        /// <summary>
        /// Converts hierarchical category structure to mvc route
        /// </summary>
        /// <param name="path">The route being processed</param>
        /// <param name="dbContext">The main app storage: <see cref="Mp3EhbContext"/></param>
        /// <returns></returns>
        private async Task<TransformResult> TransformPathAsync(string path, Mp3EhbContext dbContext,
            IDictionary<object, object> contextItems)
        {
            //The middleware will try to find a Category and a content in the database that matches the current path
            var transformResult = new TransformResult();
            var trimmedPath = path.TrimTail(tail: DEFAULT_EXTENSION);
            var names = trimmedPath.Split(new[] {FOLDER_SEPARATOR_CHAR}, StringSplitOptions.RemoveEmptyEntries);
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
                        contentPredicate = c => c.Id == id; // || c.Alias.Contains(name);
                    }
                    //The midleware will try to find a content in the database that matches the current name
                    var contents = dbContext.Contents.AsNoTracking();
                    var content = await contents
                        .Where(contentPredicate)
                        .Select(c => new {c.Id, c.CatId, c.Alias})
                        .FirstOrDefaultAsync();
                    if (content == null) break;
                    contentId = content.Id;
                    correctPath = await categores.GetCategoryPath(content.CatId) + "/" + content.Alias;
                    break;
                }
                bestMatchingPath = currentPath;
                bestMatchingCategoryId = categoryId;
            }

            if (contentId != default(int))
            {
                contextItems[CATEGORY_ID] = contentId;
                if (correctPath == path)
                {
                    return new TransformResult(HOME_CONTENT_URL + contentId, TransformAction.Substitute);
                }
            }
            else if (bestMatchingCategoryId != default(int))
            {
                contextItems[CATEGORY_ID] = bestMatchingCategoryId;
                correctPath = await categores.GetCategoryPath(bestMatchingCategoryId);
                if (correctPath == path)
                {
                    return new TransformResult(HOME_CONTENT_URL + bestMatchingCategoryId, TransformAction.Substitute);
                }
            }
            else return transformResult; //None

            return new TransformResult(correctPath, TransformAction.Redirect);
        }

        /// <summary>
        /// Harcoded categories dictionary
        /// </summary>
        //todo: refactoring needed
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
}
