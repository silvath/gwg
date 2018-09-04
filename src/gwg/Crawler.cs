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
        private readonly HttpClient _httpClient;
        public Crawler()
        {
            _httpClient = new HttpClient();
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
            string content = await GetContent();
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

        private async Task<string> GetContent()
        {
            string content = string.Empty;
            string path = Directory.GetCurrentDirectory();
            string uri = GetSiteLink();
            HttpClient httpClient = this._httpClient;
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
                return (null);
            content = await response.Content.ReadAsStringAsync();
            return (content);
        }

        private List<GameInfo> Parse(string content)
        {
            List<GameInfo> gamesInfo = new List<GameInfo>();
            int index = content.IndexOf("globalContentNew");
            string contentGraph = content.Substring(index + 19);
            int indexOld = contentGraph.IndexOf("globalContentOld");
            if (indexOld > 0)
                contentGraph = contentGraph.Substring(0, indexOld - 1);
            contentGraph = contentGraph.Trim();
            JObject graph = JsonConvert.DeserializeObject<JObject>(contentGraph);
            foreach (JToken graphLocate in graph.Children())
            {
                foreach (JToken graphCultures in graphLocate.Children())
                {
                    foreach (JToken graphCulture in graphCultures.Children())
                    {
                        string culture = ((JProperty)graphCulture).Name;
                        List<string> cultureNames = new List<string>();
                        List<string> cultureLinks = new List<string>();
                        List<string> cultureImages = new List<string>();
                        List<string> cultureInclude = new List<string>();
                        List<string> cultureExclude = new List<string>();
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
                                if (name.StartsWith("keyImagenowgame"))
                                    cultureImages.Add(value);
                                if (name.StartsWith("keyIncludenow"))
                                    cultureInclude.Add(value);
                                if (name.StartsWith("keyExcludenow"))
                                    cultureExclude.Add(value);
                            }
                        }
                        for (int i = 0; i < cultureNames.Count;i++)
                        {
                            string include = cultureInclude[i];
                            string exclude = cultureExclude[i];
                            if (!IsCultureAllowed(culture, include, exclude))
                                continue;
                            GameInfo gameInfo = new GameInfo();
                            gameInfo.Culture = culture;
                            gameInfo.Name = cultureNames[i];
                            gameInfo.Link = GetLink(culture, cultureLinks[i]);
                            gameInfo.Image = cultureImages[i];
                            if (string.IsNullOrEmpty(gameInfo.Link))
                                continue;
                            Join(gamesInfo, gameInfo);
                        }
                    }
                }
            }
            return (gamesInfo);
        }

        private bool IsCultureAllowed(string culture, string include, string exclude)
        {
            bool includeEmpty = string.IsNullOrEmpty(include);
            if ((includeEmpty) && (string.IsNullOrEmpty(exclude)))
                return (true);
            if (exclude.Contains(culture))
                return (false);
            return ((includeEmpty) || (include.Contains(culture)));
        }

        private string GetLink(string culture, string link)
        {
            if (string.IsNullOrEmpty(link))
                return (link);
            if (link.Contains("store"))
                link = link.Replace("store", culture);
            return (link);
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
