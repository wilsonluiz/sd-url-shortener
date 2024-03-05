using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using Labs.SystemDesign.UrlShortener.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Labs.SystemDesign.UrlShortener.Controllers
{
    [ApiController]
    [Route("")]
    public class UrlShortenerController : ControllerBase
    {
        private readonly ILogger<UrlShortenerController> _logger;
        private readonly UrlShortenerContext _dbUrl;
        private readonly IDistributedCache _distributedCache;

        public UrlShortenerController(ILogger<UrlShortenerController> logger, UrlShortenerContext dbUrl, IDistributedCache distributedCache)
        {
            _logger = logger;
            _dbUrl = dbUrl;
            _distributedCache = distributedCache;
        }

        [HttpPost]
        public async Task<ActionResult> CadastraUrlLonga([FromBody] string urlLonga)
        {
            var url = await EncontrarUrlLonga(urlLonga);
            if (url != null) 
                return Redirect(urlLonga);

            var urlCurta = EncurtarUrlV4(urlLonga);

            url = new Url
            {
                Id = Guid.NewGuid().ToString(),
                UrlLonga = urlLonga,
                UrlCurta = urlCurta,
                Criacao = DateTime.UtcNow,
                Ativo = true
            };

            // Persiste no Banco
            await _dbUrl.AddAsync(url);
            await _dbUrl.SaveChangesAsync();

            await AdicionarValorCache(urlLonga, urlCurta);

            // return Created(nameof(ObterUrlPorId), new {id = url.Id});
            return Created("", new { id = url.Id });
        }

        [HttpGet("{urlCurta}")]
        public async Task<ActionResult> ObterUrlLonga(string urlCurta)
        {
            var urlLonga = await _distributedCache.GetStringAsync(urlCurta);
            if (!string.IsNullOrEmpty(urlLonga))
                return Redirect(urlLonga);

            urlLonga = (await _dbUrl.Urls.FirstOrDefaultAsync(u => u.UrlCurta.Equals(urlCurta)))?.UrlLonga;
            if (string.IsNullOrEmpty(urlLonga))
                return NotFound();

            await AdicionarValorCache(urlLonga, urlCurta);

            return Redirect(urlLonga);
        }

        [HttpGet]
        public async Task<ActionResult> ListarUrls()
        {
            var urls = await _dbUrl.Urls.ToListAsync();
            return Ok(urls);
        }

        [HttpDelete]
        public async Task<ActionResult> ExcluirUrl(string id)
        {
            var url = await _dbUrl.Urls.FirstOrDefaultAsync(u => u.Id == id);
            if (url == null)
                return NotFound();

            _dbUrl.Urls.Remove(url);
            await _dbUrl.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Rotina simples para gear URL curta usando o GUID
        /// como base
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string EncurtarUrlV1(string url)
        {
            var guid = Guid.NewGuid();
            var shortenUrl = guid.ToString()
                .Replace("-", "")[..7];

            return shortenUrl;
        }

        /// <summary>
        /// Rotina para gerar a URL curta usando o CRC32
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string EncurtarUrlV2(string url)
        {
            var bytes = Encoding.UTF8.GetBytes(url);
            var crc32 = new Crc32();
            crc32.Append(bytes);

            var checksum = crc32.GetCurrentHash();
            var shortenUrl = BitConverter.ToString(checksum);
            shortenUrl = shortenUrl.Replace("-", "")[..7]
                .ToLower();

            return shortenUrl;
        }

        /// <summary>
        /// Rotina para gerar a URL curta usando o MD5
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string EncurtarUrlV3(string url)
        {
            var bytes = Encoding.UTF8.GetBytes(url);
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(bytes);

            var shortenUrl = BitConverter.ToString(hash);
            shortenUrl = shortenUrl
                .Replace("-", "")[..7]
                .ToLower();

            return shortenUrl;
        }

        /// <summary>
        /// Rotina para gerar a URL curta usando o MD5
        /// e implementando a verificacao para evitar o 'hash collision'
        /// </summary>
        /// <param name="urlLonga"></param>
        /// <returns></returns>
        private string EncurtarUrlV4(string urlLonga)
        {
            const string collisionText = "collision text";

            var urlCurta = EncurtarUrlV3(urlLonga);
            var url = _dbUrl.Urls.FirstOrDefault(u => u.UrlCurta.Equals(urlCurta));

            while (url != null)
            {
                urlLonga += collisionText;

                urlCurta = EncurtarUrlV3(urlLonga);
                url = _dbUrl.Urls.FirstOrDefault(u => u.UrlCurta.Equals(urlCurta));
            }

            return urlCurta;
        }

        private async Task<Url?> EncontrarUrlLonga(string urlLonga)
        {
            return await _dbUrl.Urls.FirstOrDefaultAsync(u => u.UrlLonga.Equals(urlLonga));
        }

        private async Task AdicionarValorCache(string urlLonga, string urlCurta)
        {
            // Config do Cache
            var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTime.Now.AddMinutes(20))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            // Uso do Cache
            await _distributedCache.SetStringAsync(urlCurta, urlLonga, options);
        }
    }
}
