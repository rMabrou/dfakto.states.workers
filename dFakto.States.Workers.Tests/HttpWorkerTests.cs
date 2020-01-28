using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using dFakto.States.Workers.FileStores;
using dFakto.States.Workers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace dFakto.States.Workers.Tests
{
    public class HttpWorkerTests : BaseTests
    {
        class TestJson
        {
            public int IntTest { get; set; } = 33;
        }
        private readonly FileStoreFactory _fileStoreFactory;

        public HttpWorkerTests()
        {
            _fileStoreFactory = Host.Services.GetService<FileStoreFactory>();
        }

        private string GetFileTokenContent(string fileToken)
        {
            using(var fileStore = _fileStoreFactory.GetFileStoreFromFileToken(fileToken))
            using (var input = fileStore.OpenRead(fileToken).Result)
            using (var reader = new StreamReader(input, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        
        private string SetFileTokenContent(string content)
        {
            using var fileStore = _fileStoreFactory.GetFileStoreFromName("test");

            var token = fileStore.CreateFileToken("tmp").Result;
            using (var output = fileStore.OpenWrite(token).Result)
            using (var writer = new StreamWriter(output))
            {
                writer.Write(content);
            }

            return token;
        }
        
        [Fact]
        public async Task TestGetJson()
        {
            HttpWorker worker = Host.Services.GetService<HttpWorker>();
            var response = await worker.DoWorkAsync(new HttpWorkerInput
            {
                Method = "GET",
                Uri = new Uri("https://postman-echo.com/get?foor=bar"),
                OutputContentFileName = "test.json",
                OutputFileStoreName = "test"
            }, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK,response.StatusCode);
            Assert.True(response.Length > 0);

            var json = GetFileTokenContent(response.ContentFileToken);
            using (var doc = JsonDocument.Parse(json))
            {
                Assert.Equal("https://postman-echo.com/get?foor=bar", doc.RootElement.EnumerateObject().First(x => x.Name == "url").Value.GetString());   
            }
        }
        
        [Fact]
        public async Task TestPostJson()
        {
            var json = JsonSerializer.Serialize(new TestJson(), new JsonSerializerOptions());

            var token = SetFileTokenContent(json);
            
            HttpWorker worker = Host.Services.GetService<HttpWorker>();
            var response = await worker.DoWorkAsync(new HttpWorkerInput
            {
                Method = "POST",
                Uri = new Uri("https://postman-echo.com/post"),
                ContentFileToken = token,
                OutputFileStoreName = "test"
            }, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK,response.StatusCode);
            Assert.True(response.Length > 0);

            var content = GetFileTokenContent(response.ContentFileToken);
            using (var d = JsonDocument.Parse(content))
            {
                Assert.Equal(json,d.RootElement.GetProperty("data").ToString());
            }
        }
        
        [Fact]
        public async Task TestPostText()
        {
            var text = "Hello world, thanks postman-echo !";
            using var fileStore = _fileStoreFactory.GetFileStoreFromName("test");

            string token = await fileStore.CreateFileToken("sample.txt");
            using (StreamWriter writer = new StreamWriter(await fileStore.OpenWrite(token)))
            {
                writer.Write(text);
            }
            HttpWorker worker = Host.Services.GetService<HttpWorker>();
            var response = await worker.DoWorkAsync(new HttpWorkerInput
            {
                Method = "POST",
                Uri = new Uri("https://postman-echo.com/post"),
                ContentFileToken = token,
                OutputFileStoreName = "test",
                OutputContentFileName = "test.json"
            }, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK,response.StatusCode);
            Assert.True(response.Length > 0);
            Assert.NotNull(response.ContentFileToken);
        }
        
        [Fact]
        public async Task TestErrorManagement()
        {
            HttpWorker worker = Host.Services.GetService<HttpWorker>();
            var response = await worker.DoWorkAsync(new HttpWorkerInput
            {
                Method = "GET",
                Uri = new Uri("https://postman-echo.com/status/404"),
                FailIfError = false
            }, CancellationToken.None);

            Assert.Equal(HttpStatusCode.NotFound,response.StatusCode);
            Assert.Null(response.ContentFileToken);
        }
    }
}