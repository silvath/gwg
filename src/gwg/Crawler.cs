using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gwg
{
    public class Crawler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public Crawler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        private string GetSiteLink()
        {
            return ("https://www.xbox.com/en-US/live/games-with-gold/rowJS/gwg-globalContent.js");
        }

        private List<string> GetCultures()
        {
            List<string> cultures = new List<string>();
            cultures.Add("pt-BR");
            return (cultures);
        }

        public async Task<List<GameInfo>> Search(string[] args)
        {
            this.Log("GWG");
            //Content
            string content = await GetContent(true);
            if (content == null)
            {
                this.Log("No content");
                return (null);
            }
            //Parse
            List<GameInfo> gamesInfo = Parse(content);
            //Log
            foreach (GameInfo gameinfo in gamesInfo)
                this.Log(string.Format("{0} - {1} - {2}", gameinfo.Name, gameinfo.Culture, gameinfo.Link));
            return (gamesInfo);
        }

        private async Task<string> GetContent(bool cache)
        {
            string content = string.Empty;
            string path = Directory.GetCurrentDirectory();
            string cachePath = Path.Combine(path, "content.txt");
            if ((cache) && (File.Exists(cachePath)))
                return(await File.ReadAllTextAsync(cachePath));
            string uri = GetSiteLink();
            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
                return (null);
            content = await response.Content.ReadAsStringAsync();
            if (File.Exists(cachePath))
                File.Delete(cachePath);
            if (cache)
                File.WriteAllText(cachePath, content);
            return (content);
        }

        private List<GameInfo> Parse(string content)
        {
            List<GameInfo> gamesInfo = new List<GameInfo>();
            int index = content.IndexOf("globalContentNew");
            string contentGraph = content.Substring(index + 19);
            JObject graph = JsonConvert.DeserializeObject<dynamic>(contentGraph);
            foreach (JToken graphLocate in graph.Children())
            {
                foreach (JToken graphCultures in graphLocate.Children())
                {
                    foreach (JToken graphCulture in graphCultures.Children())
                    {
                        string culture = ((JProperty)graphCulture).Name;
                        List<string> cultureNames = new List<string>();
                        List<string> cultureLinks = new List<string>();
                        foreach (JToken graphRow in graphCulture.Children())
                        {
                            foreach (JToken graphEntry in graphRow.Children())
                            {
                                string name = ((JProperty)graphEntry).Name;
                                string value = ((JProperty)graphEntry).Value.ToString();
                                if (name.StartsWith("keyCopytitlenowgame"))
                                    cultureNames.Add(value);
                                if (name.StartsWith("keyLinknowgame"))
                                    cultureLinks.Add(value);
                            }
                        }
                        for (int i = 0; i < cultureNames.Count;i++)
                        {
                            GameInfo gameInfo = new GameInfo();
                            gameInfo.Culture = culture;
                            gameInfo.Name = cultureNames[i];
                            gameInfo.Link = cultureLinks[i];
                            if (string.IsNullOrEmpty(gameInfo.Link))
                                continue;
                            Join(gamesInfo, gameInfo);
                        }
                    }
                }
            }
            return (gamesInfo);
        }

        private void Join(List<GameInfo> gamesInfo, GameInfo gameInfoCulture)
        {
            if (gamesInfo.Find(gi => gi.Name == gameInfoCulture.Name) == null)
                gamesInfo.Add(gameInfoCulture);
        }

        private void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
