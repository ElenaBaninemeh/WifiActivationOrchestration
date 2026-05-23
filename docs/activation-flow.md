WiFi Activation Flow



* Initial understanding

&#x20;  The service acts as an orchestration layer for WiFi activation.

&#x20;  The Customer Portal sends a WiFi activation request containing customer-related information and the requested speed profile.



&#x20;  The API must:

1. Receive the customer activation request.
2. Extract the required activation data.
3. Retrieve available speed profile information from the Network Infrastructure API.
4. Match the requested speed profile.
5. Build the activation request expected by the Network Controller API.
6. Send the activation request to the Network Controller API
7. Return a response to the caller.





Main systems

* Customer Portal: sends the activation request.
* WiFi Activation Orchestration API: receives the request and coordinates the flow.
* Network Infrastructure API: provides speed profile information
* Network Controller API: receives the final activation request.





Initial assumptions

* External APIs should be mocked for local testing and automated tests
* The first implementation should focus on the correct path
* Validation, failure handling, tests, Docker, and documentation will be improved in later steps



Initial failure scenarios to consider

* Missing required request data
* Requested speed profile does not exist
* Network Infrastructure API is unavailable
* Network Controller API rejects or fails the activation request

