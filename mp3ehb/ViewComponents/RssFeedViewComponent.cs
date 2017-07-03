using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using mp3ehb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace mp3ehb.ViewComponents
{
    public class RssFeedViewComponent : ViewComponent
    {
        private const string GET_NEWSFEED_ERROR_MESSAGE = "Ошибка получения ленты новостей ";
        private const string ERROR_TITLE = "Ошибка";
        private const string NO_NEWSFEED_SOURCE_IN_CONFIG = "В настройках не указан источник ленты новостей ";
        private const string NEWSFEED_DATETIME_FORMAT = "ddd, dd MMM yyyy HH:mm:ss K";

        private const string RSSFEEDS_SECTION = "RssFeeds";
        private const string DEFAULT_FEED_NAME = "Default";
        private const string RSS_FEED_SETTINGS_URL_KEY = "Url";

        private readonly IConfigurationSection _rssFeedsSection;
        private readonly Dictionary<string, FeedList> _rssFeeds;

        public RssFeedViewComponent(IConfigurationRoot configuration)
        {
            this._rssFeedsSection = configuration.GetSection(RSSFEEDS_SECTION);
            this._rssFeeds = new Dictionary<string, FeedList>();
        }

        public async Task<IViewComponentResult> InvokeAsync(string feedName = DEFAULT_FEED_NAME)
        {
            var feedList = this._rssFeeds.ContainsKey(feedName) ? this._rssFeeds[feedName] : this._rssFeeds[feedName] = CreateErrorFeedList(feedName);
            var task = this.RetrieveFeed(feedName).ContinueWith<FeedList>(this.SaveFeedListIfNeeded);
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
                Title = ERROR_TITLE,
                Description = GET_NEWSFEED_ERROR_MESSAGE + feedName,
                RetrievalDate = DateTime.Now
            };
        }

        private FeedList SaveFeedListIfNeeded(Task<FeedList> task)
        {
            var feedList = task.Result;
            if (!task.IsCompleted || feedList == null) return CreateErrorFeedList("");
            //todo: implement SaveFeedListIfNeeded saving to DB
            this._rssFeeds[feedList.Name] = feedList;
            return feedList;
        }

        private async Task<FeedList> RetrieveFeed(string feedName)
        {
            var feedList = CreateErrorFeedList(feedName);
            try
            {
                var rssFeedSettings = this._rssFeedsSection?.GetSection(feedName);
                var feedUrl = rssFeedSettings?[RSS_FEED_SETTINGS_URL_KEY]; // "http://voiceofsufferers.org/feed";
                if (feedUrl == null)
                {
                    feedList.Description = NO_NEWSFEED_SOURCE_IN_CONFIG + feedName;
                }
                else using (var client = new HttpClient())
                {
                    feedList.Link = feedUrl;
                    client.BaseAddress = new Uri(feedUrl);
                    var responseMessage = await client.GetAsync(feedUrl);
                    var responseString = await responseMessage.Content.ReadAsStringAsync();
                    await this.ExtractFeedListFromXmlAsync(responseString, feedList);
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
                var channel = doc.Root?.Descendants()
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
                        PublishDate = this.ParseDate(item.Elements().First(i => i.Name.LocalName == "pubDate").Value),
                        Title = item.Elements().First(i => i.Name.LocalName == "title").Value
                    });
                feedList.Items = feedItems.ToList();
                feedList.Title = channel.FirstOrDefault(i => i.Name.LocalName == "title")?.Value;
                feedList.Link = channel.FirstOrDefault(i => i.Name.LocalName == "link" && !i.IsEmpty)?.Value;
                feedList.Description = channel.FirstOrDefault(i => i.Name.LocalName == "description")?.Value;
                var dtString = channel.FirstOrDefault(i => i.Name.LocalName == "lastBuildDate")?.Value;
                feedList.PublishDate = this.ParseDate(dtString);
            });
        }

        private DateTime ParseDate(string dtString, string format)
        {
            DateTime result;
            bool valid = DateTime.TryParseExact(dtString, format, CultureInfo.InvariantCulture, 
                DateTimeStyles.AssumeUniversal, out result);
            return result;
        }
        private DateTime ParseDate(string dtString)
        {
            DateTime result;
            bool valid = DateTime.TryParseExact(dtString, NEWSFEED_DATETIME_FORMAT, 
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result);
            if (!valid) valid = DateTime.TryParse(dtString, out result);
            if (!valid) result = default(DateTime);
            return result;
        }
    }
}