using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NewsUpdates;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using RestSharp;

namespace NewsUpdateTests
{
    [TestFixture]
    public class NewsProviderTests
    {
        private IRestClient _restclientMock;
        private NewsProvider _newsprovider;
        private News _responseContent;

        [SetUp]
        public void SetUp()
        {
             _restclientMock = Substitute.For<IRestClient>();
            _newsprovider = new NewsProvider(_restclientMock);
            _responseContent = new News
            {
                Articles = new List<Article>
                {
                    new Article
                    {
                        Title = "Title manchester",
                        Description = "Text",
                        PublishedAt = "2017-06-02T14:52:59+00:00"
                    }
                }
            };
        }

        [Test]
        public void ShouldGetNewsItemOnATopicIfPresent()
        {
            _restclientMock.Execute(Arg.Any<RestRequest>()).Returns(new RestResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(_responseContent)
            });

            var newsItem = _newsprovider.GetLatestNewItem("manchester");

            Assert.That(newsItem.Date, Is.EqualTo("2017-06-02T14:52:59+00:00"));
            Assert.That(newsItem.Text, Is.EqualTo("Text"));
            Assert.That(newsItem.Title, Is.EqualTo("Title manchester"));
            _restclientMock.Received().Execute(Arg.Is<RestRequest>(x => x.Parameters.Any(p => p.Name == "source" && (string)p.Value == "bbc-news")));
        }

        [Test]
        public void ShouldMakeSecondToAnotherSorceIfAPresentIsNotFoundInFirstOne()
        {
            var responseFromSecondSource = new News
            {
                Articles = new List<Article>
                {
                    new Article
                    {
                        Title = "Title election",
                        Description = "Text",
                        PublishedAt = "2017-06-02T14:52:59+00:00"
                    }
                }
            };
            _restclientMock.Execute(Arg.Any<RestRequest>()).Returns(new RestResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(_responseContent)
            }, new RestResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(responseFromSecondSource)
            });

            var newsItem = _newsprovider.GetLatestNewItem("election");

            Assert.That(newsItem.Date, Is.EqualTo("2017-06-02T14:52:59+00:00"));
            Assert.That(newsItem.Text, Is.EqualTo("Text"));
            Assert.That(newsItem.Title, Is.EqualTo("Title election"));

            _restclientMock.Received(2).Execute(Arg.Any<RestRequest>());
            Received.InOrder(() =>
            {
                _restclientMock.Execute(Arg.Is<RestRequest>(x => x.Parameters.Any(p => p.Name == "source" && (string)p.Value == "bbc-news")));
                _restclientMock.Execute(Arg.Is<RestRequest>(x => x.Parameters.Any(p => p.Name == "source" && (string)p.Value == "the-telegraph")));
            });
        }

        [Test]
        public void ShouldGetNullResponseIfTopicIsNotPresent()
        {
            
            _restclientMock.Execute(Arg.Any<RestRequest>()).Returns(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonConvert.SerializeObject(_responseContent)
            });

            var newsItem = _newsprovider.GetLatestNewItem("election");

            Assert.That(newsItem, Is.Null);
            _restclientMock.Received(2).Execute(Arg.Any<RestRequest>());
            Received.InOrder(() =>
            {
                _restclientMock.Execute(Arg.Is<RestRequest>(x => x.Parameters.Any(p => p.Name == "source" && (string)p.Value == "bbc-news")));
                _restclientMock.Execute(Arg.Is<RestRequest>(x => x.Parameters.Any(p => p.Name == "source" && (string)p.Value == "the-telegraph")));
            });
        }

        [Test]
        public void ShouldGetExceptionIfApiThrowsException()
        {
            _restclientMock.When(x => x.Execute(Arg.Any<RestRequest>()))
                .Do(x => { throw new Exception("Unable to connect to server"); });

            Assert.That(() => _newsprovider.GetLatestNewItem(null),
                Throws.Exception
                    .TypeOf<Exception>()
                    .With.Message
                    .EqualTo("Unable to connect to server"));
        }
    }
}
