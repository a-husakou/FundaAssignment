using FundaAssignment.TrendingMakelaarApi.App;
using Microsoft.AspNetCore.Mvc;

namespace FundaAssignment.TrendingMakelaarApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrendingMakelaarController : ControllerBase
    {
        private FilterConfig filterConfig;
        private ICalculatedResultStore calculatedResultStore;

        public TrendingMakelaarController(FilterConfig filterConfig, ICalculatedResultStore calculatedResultStore)
        {
            this.filterConfig = filterConfig;
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
            var result = await calculatedResultStore.GetCalculatedResultAsync(searchTerm);
            if (result == null)
            {
                return NotFound($"No calculated result found for filter: {filter}");
            }
            return Ok(result);
        }
    }
}
