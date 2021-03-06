Feature: Get the bidder registration status

Bidder has registered with tender in order to place bid or doing any other activities.
Before doing any activities it verifies the details and save the status in database.
This endpoint returns the status of registered bidder.
Background:

	Given my required headers
		| Key                   | Value |
		| x-ba-source-platform | 10    |
		| x-ba-client-id       | 1     |
		| x-ba-client-ip       | 1     |
		| x-ba-app-id          | 1     |
		| x-ba-user-reference  | 1     |

Scenario: Approved bidder
	Given my authorization token header is authenticated with
		| SourceId | CustomerId     | MarketplaceUniqueCode |
		| 10         | a_customer_123 | 201           |
	And there is a record for customer id 121-a_customer_123-201 and partitionKey a_customer_123-201 in the database
		| BuyerId | CTA | Status   |
		| 99       |     | Approved |
	When I send get request to /tender/121/bidder/me
	Then response should be 200 OK

	And the response should contain these details
		| BuyerId | CTA | Status   |
		| 99       |     | Approved |

Scenario: Approved bidder with invalid tender id
	Given my authorization token header is authenticated with
		| SourceId | CustomerId     |
		| 10         | a_customer_123 |
	When I send get request to /tender/-121/bidder/me
	Then response should be 400 Bad Request

Scenario: Request without token
	When I send get request to /tender/121/bidder/me
	Then response should be 401 Unauthorized

Scenario: Request with expired token
	Given the token is expired for
		| SourceId | CustomerId     |
		| 10         | a_customer_123 |
	When I send get request to /tender/121/bidder/me
	Then response should be 401 Unauthorized

Scenario: Request when the tender does not contain a registered bidder
	Given my authorization token header is authenticated with
		| SourceId | CustomerId     | MarketplaceUniqueCode |
		| 10         | a_customer_123 | 201           |
	And there is no record with customer id 121-a_customer_123-201 and partitionKey a_customer_123-201 in the database
	When I send get request to /tender/121/bidder/me
	Then response should be 200 OK
	And the response should contain these details
		| BuyerId | Status | CTA |
		|          | None   |     |