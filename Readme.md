# Assignment analysis

Looking at dataset responses using both filters from the assignment requirement (Amsterdam and Amsterdam with garden), 
their sizes are relatively small (around 6k records in total). Assuming that the dataset sizes do not grow significantly,
it should be possible to fetch all records without hiting rate limit of the API (100 req/min) by increasing the page size.
One other consideration for choosing a right solution is a frequency of the feed. Expectation is that it happens somewhere once a day (perhaps a little more frequently)

Considering the above my choice would be the simplest implementation as following (the simpler solution solves business problem the better)
- have a process that eagerly visits the API to calculate the result for both filters
- - process stores the calculated result (cache or db) per filter
- - process refreshes a data fully depending on how frequently a new data appears on API (assumed to be around 1-2 times a day)
- expose an endpoint for a client that would return the result from the store 

Even considering that page size can not be increased, so hitting a rate limit is inevitable, it takes 3 minutes for a full update - 100 requests/min * 25 record/request = 2500 records/min
3 minutes for a feed refreshing every half a day sounds negligible. So all it takes for a solution is to process 429 by introducing a 1 minute backoff for an http client
BTW, it is the case now, the API from the assignment has a broken pagination, returning an inconsistent result when incresing page size (tried with 100).
It still returns 25 records, but changes a "Paging" field as if it returned 100 records.

## Scaling limitation

Although a solution would satisfy the requirements, it has scaling limitation.

The following scenarios can eventually affect the system:
	- growing result datasets from funda API; (and/or)
	- growing feed frequency (every couple of minutes); (and/or)
	- growing number of filters to support (or dynamic filters defined by user).

Note: for dynamic filters, the API should be designed to be asyncronous (no immidiate response).

# Scaling mitigation strategies

The strategies suggested below are not mutually exclusive, they can be mixed and matched.

## Result calculation partitioning

Warn: only possible if funda API significanly reduces rate limiting

The idea is to split the work among multiple servers. Such as per filter or per range of pages. 	

## Diff update

The idea is to introduce a breakdown for a makelaar listing count (such as listing count per date range) and store them in a local db.
This will allow to fetch the limited number of pages from funda API (from head and from tail to detect new and obsoleted results). 
Calculate the diff accordingly and apply it to the current actual result.

## Prioritization

With a big user base and dynamic filters, it is nice to consider queues with priorities, especially if rate limiting is a bottleneck.
Possible priority metric: ratio between number of users expressing the interest in a particular filter and recalculation effort

# Other scaling consideration

One of the ways to look at the assignment is from data-engeneering perspective. 
The funda API becomes a stream of data without any filters, and data is indexed as necessary and placed in a local db.
So the user quieries are served from a DB.
Nice to consider when running business based on scraping data without parterning up with origin company :)