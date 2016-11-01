using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;
using mp3ehb.core1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace mp3ehb.core1.ViewComponents
{
    public class RssFeedViewComponent : ViewComponent
    {
        private readonly IConfigurationSection _rssFeedsSection;
        private readonly Dictionary<string, FeedList> _rssFeeds;

        public RssFeedViewComponent(IConfigurationRoot configuration)
        {
            _rssFeedsSection = configuration.GetSection("RssFeeds");
            _rssFeeds = new Dictionary<string, FeedList>();
        }

        public async Task<IViewComponentResult> InvokeAsync(string feedName = "Default")
        {
            var feedList = _rssFeeds.ContainsKey(feedName) ? _rssFeeds[feedName] : _rssFeeds[feedName] = CreateErrorFeedList(feedName);
            var task = RetrieveFeed(feedName).ContinueWith<FeedList>(SaveFeedListIfNeeded);
            if (feedList.PublishDate == default(DateTime))
            {
                feedList = await task;
            }

            return View(feedName, feedList);
        }

        private static FeedList CreateErrorFeedList(string feedName)
        {
            return new FeedList
            {
                Name = feedName,
                Title = "Ошибка",
                Description = "Ошибка получения ленты новостей " + feedName,
                RetrievalDate = DateTime.Now
            };
        }

        private FeedList SaveFeedListIfNeeded(Task<FeedList> task)
        {
            var feedList = task.Result;
            if (!task.IsCompleted || feedList == null) return CreateErrorFeedList("");
            //todo: implement SaveFeedListIfNeeded saving to DB
            _rssFeeds[feedList.Name] = feedList;
            return feedList;
        }

        private async Task<FeedList> RetrieveFeed(string feedName)
        {
            var feedList = CreateErrorFeedList(feedName);
            try
            {
                var rssFeedSettings = _rssFeedsSection?.GetSection(feedName);
                var feedUrl = rssFeedSettings?["Url"]; // "http://voiceofsufferers.org/feed";
                if (feedUrl == null)
                {
                    feedList.Description = "В настройках не указан источник ленты новостей " + feedName;
                }
                else using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(feedUrl);
                        var responseMessage = await client.GetAsync(feedUrl);
                        var responseString = await responseMessage.Content.ReadAsStringAsync();
                        await ExtractFeedListFromXmlAsync(responseString, feedList);
                    }
            }
            catch (Exception exception)
            {
                feedList.Description += ". " + exception.Message;
            }
            return feedList;
        }

        private Task ExtractFeedListFromXmlAsync(string responseString, FeedList feedList)
        {
            return Task.Run(() =>
            {
                XDocument doc = XDocument.Parse(responseString);
                var channel = doc.Root.Descendants()
                    .First(i => i.Name.LocalName == "channel")
                    .Elements().ToList();
                //extract feed items
                var feedItems = channel
                    .Where(i => i.Name.LocalName == "item")
                    .Select(item => new FeedItem
                    {
                        Description = item.Elements().First(i => i.Name.LocalName == "description").Value,
                        Content = item.Elements().First(i => i.Name.ToString().Contains("content")).Value,
                        Link = item.Elements().First(i => i.Name.LocalName == "link").Value,
                        PublishDate = ParseDate(item.Elements().First(i => i.Name.LocalName == "pubDate").Value),
                        Title = item.Elements().First(i => i.Name.LocalName == "title").Value
                    });
                feedList.Items = feedItems.ToList();
                feedList.Title = (channel.FirstOrDefault(i => i.Name.LocalName == "title")).Value;
                feedList.Link = (channel.FirstOrDefault(i => i.Name.LocalName == "link")).Value;
                feedList.Description = (channel.FirstOrDefault(i => i.Name.LocalName == "description")).Value;
                var dtString = (channel.FirstOrDefault(i => i.Name.LocalName == "lastBuildDate")).Value;
                feedList.PublishDate = ParseDate(dtString);
            });
        }
        /*Tue, 27 Sep 2016 22:02:45 +0000
         * 'ddd, dd MMM yyyy HH:mm:ss K'
          string[] dateStrings = {"2008-05-01T07:34:42-5:00", 
                              "2008-05-01 7:34:42Z", 
                              "Thu, 01 May 2008 07:34:42 GMT"};
          foreach (string dateString in dateStrings)
          {
             DateTime convertedDate = DateTime.Parse(dateString);
             Console.WriteLine("Converted {0} to {1} time {2}", 
                               dateString, 
                               convertedDate.Kind.ToString(), 
                               convertedDate);
          }       
         */
        private DateTime ParseDate(string dtString, string format)
        {
            DateTime result = default(DateTime);
            bool valid = DateTime.TryParseExact(dtString, format, CultureInfo.InvariantCulture, 
                DateTimeStyles.AssumeUniversal, out result);
            return result;
        }
        private DateTime ParseDate(string dtString)
        {
            DateTime result;
            bool valid = DateTime.TryParseExact(dtString, "ddd, dd MMM yyyy HH:mm:ss K", 
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result);
            if (!valid) valid = DateTime.TryParse(dtString, out result);
            if (!valid) result = default(DateTime);
            return result;
        }
    }
}