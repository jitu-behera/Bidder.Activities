Feature: Healthcheck
	Confirm service is working

@mytag
Scenario: Check service health
	And my required headers
		| Key                   | Value |
		| x-ba-source-platform | 10    |
		| x-ba-client-id       | 1     |
		| x-ba-client-ip       | 1     |
		| x-ba-app-id          | 1     |
		| x-ba-user-reference  | 1     |
	When I ask for a service healthcheck
	Then the response should be 200 OK