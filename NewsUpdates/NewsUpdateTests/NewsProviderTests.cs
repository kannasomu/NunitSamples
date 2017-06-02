using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NewsUpdates;
using NUnit.Framework;
using RestSharp;

namespace NewsUpdateTests
{
    [TestFixture]
    public class NewsProviderTests
    {
        private Mock<IRestClient> _restclientMock;
        private NewsProvider _newsprovider; 

        [SetUp]
        public void SetUp()
        {
            _restclientMock = new Mock<IRestClient>();
            _newsprovider = new NewsProvider(_restclientMock.Object);
        }


        [Test]
        public void ShouldGetNewsItemOnATopicIfPresent()
        {
            
            _restclientMock.Setup(x => x.Execute(It.IsAny<RestRequest>())).Returns(new RestResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{\n \"articles\": [\n {\n \"author\": \"BBC News\",\n \"title\": \"Title manchester\",\n\"description\": \"Text\",\n\"publishedAt\": \"2017-06-02T14:52:59+00:00\"\n }\n]\n}"
            });

            var newsItem = _newsprovider.GetLatestNewItem("manchester");

            Assert.That(newsItem.Date, Is.EqualTo("2017-06-02T14:52:59+00:00"));
            Assert.That(newsItem.Text, Is.EqualTo("Text"));
            Assert.That(newsItem.Title, Is.EqualTo("Title manchester"));
            _restclientMock.Verify(x => x.Execute(It.IsAny<RestRequest>()), Times.Once);
        }



        [Test]
        public void ShouldGetNullResponseIfTopicIsNotPresent()
        {
            _restclientMock.Setup(x => x.Execute(It.IsAny<RestRequest>())).Returns(new RestResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Content = "{\n \"articles\": [\n {\n \"author\": \"BBC News\",\n \"title\": \"Title manchester\",\n\"description\": \"Text\",\n\"publishedAt\": \"2017-06-02T14:52:59+00:00\"\n }\n]\n}"
            });

            var newsItem = _newsprovider.GetLatestNewItem("election");

            Assert.That(newsItem, Is.Null);
        }

        [Test]
        public void ShouldGetExceptionIfApiThrowsException()
        {
            var restclientMock = new Mock<IRestClient>();
            var newsprovider = new NewsProvider(restclientMock.Object);
            restclientMock.Setup(x => x.Execute(It.IsAny<RestRequest>()))
                .Throws(new Exception("Unable to connect to server"));

            Assert.That(() => newsprovider.GetLatestNewItem(null),
                Throws.Exception
                    .TypeOf<Exception>()
                    .With.Message
                    .EqualTo("Unable to connect to server"));
        }
    }
}
