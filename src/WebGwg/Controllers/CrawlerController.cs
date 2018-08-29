using gwg;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebGwg.Controllers
{
    [Route("api/[controller]/[action]")]
    public class CrawlerController : Controller
    {
        private Crawler _crawler;
        public CrawlerController(Crawler crawler)
        {
            _crawler = crawler;
        }
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GameInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(Exception), (int)HttpStatusCode.InternalServerError)]
        public async Task<ActionResult> GetGames()
        {
            try
            {
                return(Ok(await _crawler.Search(null)));
            }
            catch (Exception e)
            {
                return (StatusCode((int)HttpStatusCode.InternalServerError, e));
            }
        }
    }
}
