# Assignment Analysis

## Overview
- ✔️ Using both filters (Amsterdam and Amsterdam with garden), the dataset sizes are relatively small (around 6k records in total).
- ✔️ Assuming the dataset size does not grow significantly, it should be possible to fetch all records without hitting the API rate limit (100 req/min) by increasing the page size.
- ✔️ Another factor is feed frequency, which is expected to be around once a day (perhaps slightly more often).

## Proposed Approach (Keep It Simple)
- ✔️ Run a process that eagerly queries the API to compute the result for both filters.
- ✔️ Store the computed result per filter (cache or DB).
- ✔️ Refresh the data fully based on how frequently new data appears (assumed 1-2 times per day).
- ✔️ Expose an endpoint for clients that returns the stored result.

## Rate Limit and Pagination Notes
- Even if the page size cannot be increased and the rate limit is unavoidable, a full update takes roughly 3 minutes:
  - 100 requests/min * 25 records/request = 2,500 records/min.
  - For a feed refreshing every half a day, 3 minutes is negligible.
- Handle HTTP 429 by introducing a 1-minute backoff in the HTTP client.
- Current behavior: the API shows inconsistent pagination when increasing page size (tested with 100). It still returns 25 records but adjusts the `Paging` field as if it returned 100.

## Scaling Limitations
Although this approach satisfies the requirements, it has scaling limitations. The system can be affected by:
- Growing result datasets from the Funda API.
- Increasing feed frequency (e.g., every couple of minutes).
- An increasing number of filters to support (or dynamic, user-defined filters).

**Note:** for dynamic filters, the API should be designed to be asynchronous (no immediate response).

## Scaling Mitigation Strategies
The strategies below are not mutually exclusive and can be combined.

### Result Calculation Partitioning
- **Warning:** only feasible if the Funda API significantly reduces rate limiting.
- Split the work across multiple servers (e.g., per filter or per range of pages).

### Diff Update
- Introduce a breakdown for makelaar listing counts (e.g., counts per date range) and store them in a local DB.
- Fetch a limited number of pages from the Funda API (from the head and tail to detect new and obsolete results).
- Compute the diff and apply it to the current result.

### Prioritization
- With a large user base and dynamic filters, consider priority queues, especially if rate limiting is a bottleneck.
- Possible priority metric: the ratio between the number of users interested in a particular filter and the recalculation effort.

## Other Scaling Consideration
- A data-engineering perspective: treat the Funda API as a stream of data without filters, index as necessary, and store it in a local DB.
- Serve user queries from the DB.


// TODO mention the updated project structure, it is probably not the most important to create a structure for this size of a project, but since Stephan touched upon separation of concerns, the decision was to demonstrate broader ideas with clean arc

