using System;
using System.Net;
using NewsUpdates;
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

        [SetUp]
        public void SetUp()
        {
             _restclientMock = Substitute.For<IRestClient>();
            _newsprovider = new NewsProvider(_restclientMock);
        }


        [Test]
        public void ShouldGetNewsItemOnATopicIfPresent()
        {
            _restclientMock.Execute(Arg.Any<RestRequest>()).Returns(new RestResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{\n \"articles\": [\n {\n \"author\": \"BBC News\",\n \"title\": \"Title manchester\",\n\"description\": \"Text\",\n\"publishedAt\": \"2017-06-02T14:52:59+00:00\"\n }\n]\n}"
            });

            var newsItem = _newsprovider.GetLatestNewItem("manchester");

            Assert.That(newsItem.Date, Is.EqualTo("2017-06-02T14:52:59+00:00"));
            Assert.That(newsItem.Text, Is.EqualTo("Text"));
            Assert.That(newsItem.Title, Is.EqualTo("Title manchester"));
            _restclientMock.Received().Execute(Arg.Any<RestRequest>());
        }


        [Test]
        public void ShouldGetNullResponseIfTopicIsNotPresent()
        {
            _restclientMock.Execute(Arg.Any<RestRequest>()).Returns(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{\n \"articles\": [\n {\n \"author\": \"BBC News\",\n \"title\": \"Title manchester\",\n\"description\": \"Text\",\n\"publishedAt\": \"2017-06-02T14:52:59+00:00\"\n }\n]\n}"
            });

            var newsItem = _newsprovider.GetLatestNewItem("election");

            Assert.That(newsItem, Is.Null);
            _restclientMock.Received(1).Execute(Arg.Any<RestRequest>());
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
