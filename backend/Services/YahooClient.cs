
using FantasyArchive.Api.Extensions;
using FantasyArchive.Api.Models.Yahoo;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FantasyArchive.Api.Services
{
    public class YahooClient : IDisposable
    {

        private readonly HttpClient client;

        public YahooClient(ISyncContext context, IConfiguration configuration)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(configuration["Yahoo:ApiBaseUrl"] ?? "https://fantasysports.yahooapis.com");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {context.Token}");
            Console.WriteLine($"Bearer {context.Token}");
        }


        public async Task<T> GetAsync<T>(string request, CancellationToken cancellationToken) where T : class
        {
            var result = await GetCollectionAsync<T>(request, cancellationToken, 1);

            return result.FirstOrDefault();
        }


        public async Task<IList<T>> GetCollectionAsync<T>(string request, CancellationToken cancellationToken, int? max = null)
        {
            var response = await client.GetAsync(request, cancellationToken);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                try
                {
                    var errorContext = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(errorContext);
                }
                catch { }
                throw new Exception($"Request failed Yahoo! request: {response.StatusCode}");
            }

            var result = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(result))
            {
                Console.WriteLine("No content in Yahoo! response");
                throw new Exception("No content in Yahoo! response");
            }

            try
            {
                var xml = XDocument.Parse(result);

                var lookup = typeof(T).GetAttributeValue((YahooLookupAttribute x) => x.Name);

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                List<XElement> xElements = xml.Descendants(YahooXml.XMLNS + lookup).ToList();
                List<T> collection = new List<T>();
                int count = 0;
                foreach (var element in xElements)
                {
                    collection.Add((T)serializer.Deserialize(element.CreateReader()));
                    if (max.HasValue && ++count >= max.Value)
                    {
                        break;
                    }
                }
                return collection;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
    internal static class YahooXml
    {
        /// <summary>
        /// Xml Namespace for Yahoo Fantasy Xml Returned
        /// </summary>
        internal static XNamespace XMLNS = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng";
    }
}