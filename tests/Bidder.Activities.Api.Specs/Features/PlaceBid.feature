Feature: Widget place bid

Users on the widget need a way to place bids
Given this is a public endpoint, we need to validate they are authenticated and registered for the auction before placing the bid
Then we to forward the request to the shared service (Shared Bidding Service), and return their response to our clients.
Background:

	Given my required headers
		| Key                  | Value |
		| x-ba-source-platform | 10    |
		| x-ba-client-id       | 1     |
		| x-ba-client-ip       | 1     |
		| x-ba-app-id          | 1     |
		| x-ba-user-reference  | 1     |

Scenario: Approved bidder
	Given my authorization token header is authenticated with
		| SourceId | CustomerId     | MarketplaceUniqueCode |
		| 20         | a_customer_123 | 201           |
	And there is a record for customer id 10-a_customer_123-201 and partitionKey a_customer_123-201 in the database
		| BuyerId | BuyerRef | Status   |
		| 99       | 101C      | Approved |
	And the bidding service response code is 200 with response body
		"""
		{ "success": true }
		"""
	When I send POST request to v1/place-bid
		"""
		{
		  "ItemId": 111,
		  "TenderId": 10,
		  "bidAmount": 99.59
		}
		"""
	Then response should be 200 OK
	And the shared bidding service must have been called with
		| ItemId | TenderId | Amount | BuyerId | BuyerRef | SourceId | MarketplaceUniqueCode | MarketplaceChannelCode |
		| 111   | 10        | 99.59  | 99       | 101C      | 20         | 201           | PxbJJKWid1               |
	And the response body should be
		"""
		{ "success": true }
		"""

Scenario: Approved bidder from cookie
	Given my authorization token cookie is authenticated with
		| SourceId | CustomerId     | MarketplaceUniqueCode |
		| 20         | a_customer_123 | 201           |
	And there is a record for customer id 10-a_customer_123-201 and partitionKey a_customer_123-201 in the database
		| BuyerId | BuyerRef | Status   |
		| 99       | 101C      | Approved |
	And the bidding service response code is 200 with response body
		"""
		{ "success": true }
		"""
	When I send POST request to v1/place-bid
		"""
		{
		  "ItemId": 111,
		  "TenderId": 10,
		  "bidAmount": 99.59
		}
		"""
	Then response should be 200 OK
	And the shared bidding service must have been called with
		| ItemId | TenderId | Amount | BuyerId | BuyerRef | SourceId | MarketplaceUniqueCode | MarketplaceChannelCode |
		| 111   | 10        | 99.59  | 99       | 101C      | 20         | 201           | PxbJJKWid1               |
	And the response body should be
		"""
		{ "success": true }
		"""

	
Scenario: Approved bidder Bad request from shared service
	Given my authorization token header is authenticated with
		| SourceId | CustomerId     | MarketplaceUniqueCode |
		| 20         | a_customer_123 | 201           |
	And there is a record for customer id 10-a_customer_123-201 and partitionKey a_customer_123-201 in the database
		| BuyerId | BuyerRef | Status   |
		| 99       | 101C      | Approved |
	And the bidding service response code is 400 with response body
		"""
		{ "validationErrors": "anyErrors" }
		"""
	When I send POST request to v1/place-bid
		"""
		{
		  "ItemId": 111,
		  "TenderId": 10,
		  "bidAmount": 99.59
		}
		"""
	Then response should be 400 Bad Request
	And the shared bidding service must have been called with
		| ItemId | TenderId | Amount | BuyerId | BuyerRef | SourceId | MarketplaceUniqueCode | MarketplaceChannelCode |
		| 111   | 10        | 99.59  | 99       | 101C      | 20         | 201           | PxbJJKWid1               |
	And the response body should be
		"""
		{ "validationErrors": "anyErrors" }
		"""

Scenario Outline: Bidder with Pending or Denied status
	Given my authorization token header is authenticated with
		| SourceId | CustomerId     | MarketplaceUniqueCode |
		| 20         | a_customer_123 | 201           |
	And there is a record for customer id 10-a_customer_123-201 and partitionKey a_customer_123-201 in the database
		| BuyerId | BuyerRef | Status   |
		| 99       | 101C      | <Status> |
	When I send POST request to v1/place-bid
		"""
		{
		  "ItemId": 111,
		  "TenderId": 10,
		  "bidAmount": 99.59
		}
		"""
	Then response should be 403 Forbidden
	And the bidding service has never been called
Examples:
	| Status  |
	| Pending |
	| Denied  |

Scenario: Bidder not registered for the auction
	Given my authorization token header is authenticated with
		| SourceId | CustomerId     | MarketplaceUniqueCode |
		| 20         | a_customer_123 | 201           |
	And there is no record with customer id 10-a_customer_123-201 and partitionKey a_customer_123-201 in the database
	When I send POST request to v1/place-bid
		"""
		{
		  "ItemId": 111,
		  "TenderId": 10,
		  "bidAmount": 99.59
		}
		"""
	Then response should be 403 Forbidden
	And the bidding service has never been called

Scenario: Request without token
	When I send POST request to v1/place-bid
		"""
		{
		  "ItemId": 111,
		  "TenderId": 10,
		  "bidAmount": 99.59
		}
		"""
	Then response should be 401 Unauthorized
	And the bidding service has never been called

Scenario: Request with expired token
	Given the token is expired for
		| SourceId | CustomerId     |
		| 10         | a_customer_123 |
	When I send POST request to v1/place-bid
		"""
		{
		  "ItemId": 111,
		  "TenderId": 10,
		  "bidAmount": 99.59
		}
		"""
	Then response should be 401 Unauthorized
	And the bidding service has never been called

Scenario: Placebid missing fields
	Given my authorization token header is authenticated with
		| SourceId | CustomerId     | MarketplaceUniqueCode |
		| 20         | a_customer_123 | 201           |
	When I send POST request to v1/place-bid
		"""
		{
		  "ItemId": null,
		  "TenderId": null,
		  "bidAmount": null
		}
		"""
	Then response should be 400 Bad Request
	And the bidding service has never been called
	And the response contains these validation errors with unique paths
		| code | value              | description                      | path         |
		| 100  | ERROR_MISSING_DATA | The ItemId field is required.     | ItemId     |
		| 100  | ERROR_MISSING_DATA | The TenderId field is required. | TenderId |
		| 100  | ERROR_MISSING_DATA | The BidAmount field is required. | BidAmount    |

Scenario: Placebid negative numbers
	Given my authorization token header is authenticated with
		| SourceId | CustomerId     | MarketplaceUniqueCode |
		| 20         | a_customer_123 | 201           |
	When I send POST request to v1/place-bid
		"""
		{
		  "ItemId": -1,
		  "TenderId": -1,
		  "bidAmount": -1
		}
		"""
	Then response should be 400 Bad Request
	And the bidding service has never been called
	And the response contains these validation errors with unique paths
		| code | value              | description                                         | path         |
		| 100  | ERROR_MISSING_DATA | The ItemId field should be a positive number.     | ItemId     |
		| 100  | ERROR_MISSING_DATA | The TenderId field should be a positive number. | TenderId |
		| 100  | ERROR_MISSING_DATA | The BidAmount field should be a positive number.    | BidAmount    |