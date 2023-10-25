# Introduction

This is the FFC Integrations project. It replaces the functionality previously provided by Logic Apps. See the following [wiki](https://dev.azure.com/defragovuk/DEFRA-EST/_wiki/wikis/DEFRA-EST/13997/FFC-Payments-Statement-Generator-workflows) for information about the functionality.

# Getting Started

Clone this [repo](https://github.com/DEFRA/ffc-payment-integrations).

# Local setup

No sensitive information is stored in the local.settings.json. 

When developing locally values may be set via `dotnet user-secrets`.

The simplest way to add secrets to the local store via Visual Studio by right clicking on the
`FFC.Payment.Integrations.Function` project in the Solution Explorer, and selecting 'Manage User Secrets'.

Alternatively save the values into a `secrets.json` file, i.e. 
```
{
  "NotifyApiKey": "--SECRET-VALUE--",
  "PortalClientId": "--SECRET-VALUE--",
  "PortalTenantId": "--SECRET-VALUE--",
  "PortalClientSecret": "--SECRET-VALUE--",
  "FunctionSasToken": "--SECRET-VALUE--",
  "ServiceBusConnectionString": "--SECRET-VALUE--"
}

```
then load via the command line:
```
cat ./secrets.json | dotnet user-secrets set
```


# Storage Account Setup

The project requires a storage account in order to run the functions.

Install Azure Storage Explorer to provide the ability for the function apps to connect to a storage account.

For local development ensure Azurite is installed. This is installed as part of VS2002, or can be installed as an extension of VS Code.

# Build and Test

The easiest way to build the project is with VS2022. It should download all required nuget dependencies.

Run the tests using the VS2022 Test Explorer.

To run locally in Docker, run:
```docker-compose up```

(Homepage will be accessible on http://localhost:3001 to prove the Function App is running)

Post a message to the appropriate Service Bus Topic and subscription. Here is a sample message:

```
{ 
    "sbi": 27,
    "frn": 1102077240,
    "apiLink": "https://myStatementRetrievalApiEndpoint/statement-receiver/statement/v1/FFC_PaymentSchedule_SFI_2022_1000000002_2023072703002347.pdf",
    "documentType": "Payment statement", 
    "scheme": "SFI"
}
```

# Gov Notify

You will need a [Gov Notify account] (https://www.notifications.service.gov.uk/) to view the Notify templates that are used. Each template has an id. Each template id is mapped via the Action property of the incoming message.

If using an API key that is a 'team' key, you will need to be added to the 'team' by whoever owns the API key in order to send emails to yourself.

# Code check-in

Prior to committing changes run the following command from a cmd line within the solution folder: dotnet format whitespace --verbosity n

Install the VS2022 extension SonarLint. Prior to commiting changes check the Messages window for any code smells / issues that would cause the build pipeline to fail.

# Cloud setup
To create the cloud infrastructure in AKS follow the guidlines on the FC Platform section of the DEFRA-EST Wiki. 



