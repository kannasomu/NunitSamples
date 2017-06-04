using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace NewsUpdates
{
    class Program
    {
        static void Main(string[] args)
        {
            var topicToSearchFor = args[0];
            Console.WriteLine($"Latest news on {topicToSearchFor}");

            var newsProvider = new NewsProvider(new RestClient(new Uri(@"https://newsapi.org/v1/articles")));
            var newsItem = newsProvider.GetLatestNewItem(topicToSearchFor);
            
            Console.WriteLine($"Title: {newsItem.Title}");
            Console.WriteLine($"Date: {newsItem.Date}");
            Console.WriteLine("Text");
            Console.WriteLine(newsItem.Text);
        }
    }

    public class NewsItem
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string Text { get; set; }
    }
    
    public class NewsProvider
    {
        private readonly IRestClient _restClient;
        public NewsProvider(IRestClient restClient)
        {
            _restClient = restClient;
        }
        public NewsItem GetLatestNewItem(string topicName)
        {
            var newsItem = TryGetNewsItemsFromSource(topicName, "bbc-news");
            return newsItem ?? TryGetNewsItemsFromSource(topicName, "the-telegraph");
        }

        private NewsItem TryGetNewsItemsFromSource(string topicName, string source)
        {
            var req = new RestRequest();
            AddRequestParametersWithSource(req, source);

            var response = _restClient.Execute(req);
            var news = JsonConvert.DeserializeObject<News>(response.Content);

            if (news.Articles.FirstOrDefault(x => x.Title.Contains(topicName)) == null) return null;
            {
                return new NewsItem
                {
                    Date = news.Articles[0].PublishedAt,
                    Title = news.Articles[0].Title,
                    Text = news.Articles[0].Description
                };
            }
        }

        private static void AddRequestParametersWithSource(RestRequest req, string source)
        {
            req.AddParameter("source", source);
            req.AddParameter("sortBy", "top");
            req.AddParameter("apiKey", ConfigurationManager.AppSettings["ApiKey"]);
        }
    }

    public class Article
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string UrlToImage { get; set; }
        public string PublishedAt { get; set; }
    }

    public class News
    {
        public string Status { get; set; }
        public string Source { get; set; }
        public string SortBy { get; set; }
        public List<Article> Articles { get; set; }
    }
}
