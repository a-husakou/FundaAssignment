# Funda Assignment – Solution Guide

See also: docs/AssignmentAnalysis.md for the original analysis.

## Solution Structure
The main solution follows a clean architecure organization (/src folder). With the following projects
- `FundaAssignment.TrendingMakelaarApi` – ASP.NET Core Web API host (controllers, startup).
- `FundaAssignment.Application.Common` – shared contracts and configuration types (e.g., `FilterConfig`, result DTOs).
- `FundaAssignment.Application.TrendingMakelaarCalculation` – pulls listings, aggregates per makelaar, stores sorted results.
- `FundaAssignment.Infrastructure` – implementation of application interfaces that do not relate to application concerns, as well as wiring up background processing and warm up

More generic primitives that are reusable accross wider set of applications are placed in /external folder. 
This split is only made to extract generic logic out of the main solution and is not intended to be comprehensive.
It has a single project `Funda.Common` with the following functionality
- Background processor scheduler. Executes the processor at a fixed interval and can perform an initialization run. This is intentionally interval-only to cover the assignment’s scenario.
- The warmup short-circuits requests (except health and swagger) with 503 until the first successful calculation. This provides a readiness-like behavior for local/dev, where no platform readiness probes exist.
Further improvements
- Split into submodules (e.g., `Funda.Common.BackgroundProcessing`, `Funda.Common.Warmup`)
- Complete the logic to address needs for a wider group of applications
- Add tests

API level tests are added in /test folder. Smaller scope tests are skipped as there are no complicated business rules. 

## How It Works
- Background service runs on a schedule (Interval) and optionally once at startup (PerformInitializationRun).
- For each configured filter (`/amsterdam`, `/amsterdam/tuin`), the calculation fetches all pages, aggregates counts per makelaar, sorts them, and stores the result in memory.
- The endpoint returns the stored result in O(1).

## Configuration
- `Filters:FilterSearchTerms` – supported filters
- `TrendingMakelaarCalculation:RefreshInterval` – cache freshness threshold
- `BackgroundProcessing:RefreshCalculatedMakelaarData` – scheduler interval + retry policy
- `FundaApi` – base URL + API key + retry policy (reactive on HttpResponse)

## Running Locally
PowerShell example:
- ` $env:FundaApi__ApiKey = "<YOUR_FUNDA_API_KEY>" `
- ` $env:ASPNETCORE_URLS = "http://localhost:5136" `
- ` dotnet run --project FundaAssignment.TrendingMakelaarApi`

Open Swagger at `http://localhost:5136/swagger`.

## Rate Limiting, Pagination and Timing
- The HTTP client respects 429 (401 in case of FundaAPI) and applies a bounded retry with delay as per assignment doc.
- Page size is fixed to 25 based on current API behavior.
- Initial full calculation can take minutes; subsequent requests are served from memory.

## Testing
- Tests use `appsettings.Test.json` to shorten intervals and keep tests fast.

## Metrics (Future Work)
- Duration of full refresh + page fetch latency
- 429 rate and effective delays/backoff
- Listings processed per filter + distribution per makelaar
- Cache freshness and last success/failure markers

## Serialization Notes
- `FundaApiClient` uses System.Text.Json; if multiple formats are needed, abstract content serialization behind an interface and inject it.

## Security Notes
- Filters are whitelisted; results served from memory; no dynamic DB queries in this solution.
