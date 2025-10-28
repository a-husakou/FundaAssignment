using FundaAssignment.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FundaAssignment.TrendingMakelaarApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrendingMakelaarController : ControllerBase
    {
        // this can become an endpoint input argument
        public const int ItemsLimit = 10; 

        private FilterConfig filterConfig;
        private ICalculatedResultStore calculatedResultStore;

        public TrendingMakelaarController(IOptions<FilterConfig> filterConfig, ICalculatedResultStore calculatedResultStore)
        {
            this.filterConfig = filterConfig.Value;
            this.calculatedResultStore = calculatedResultStore;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery]string filter)
        {
            if (!filterConfig.FilterSearchTerms.TryGetValue(filter, out var searchTerm))
            {
                return BadRequest($"Unsupported filter: {filter}. Supported filters: {string.Join(", ", filterConfig.FilterSearchTerms)}");
            }

            // unrealistic unless no data on funda at all
            var result = await calculatedResultStore.GetCalculatedDataAsync(searchTerm, ItemsLimit);
            if (result == null)
            {
                return NotFound($"No calculated result found for filter: {filter}");
            }
            return Ok(result);
        }
    }
}
