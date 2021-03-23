# Digital Apprenticeships Service

## Apprentice Feedback Bot (Alpha)
|               |               |
| ------------- | ------------- |
|![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png)|Tasks|
| Build | [![Build Status](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_apis/build/status/Provide%20Feedback/das-provide-feedback-apprentice?branchName=CON-3318_fix-netcore3-issue)](https://dev.azure.com/sfa-gov-uk/Digital%20Apprenticeship%20Service/_build/latest?definitionId=1142&branchName=CON-3318_fix-netcore3-issue) |

## Developer setup

### Requirements

* [.NET Core SDK >= 2.1.302](https://www.microsoft.com/net/download/)
* [Docker for X](https://docs.docker.com/install/#supported-platforms) (not required for emailer functions)
* [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) (not required for emailer functions)
* Azure Service Bus

### Environment Setup

The default development environment uses docker containers to host the following dependencies.

* Redis
* Elasticsearch
* Logstash

On first setup run the following command from _**/setup/containers/**_ to create the docker container images:

`docker-compose build`

To start the containers run:

`docker-compose up -d`

You can view the state of the running containers using:

`docker ps -a`

Run Azure Cosmos DB Emulator

##### Publish database (ESFA.DAS.EmployerFeedbackEmail.Database) to sql server.


### Application logs
Application logs are logged to [Elasticsearch](https://www.elastic.co/products/elasticsearch) and can be viewed using [Kibana](https://www.elastic.co/products/kibana) at http://localhost:5601

## License

Licensed under the [MIT license](LICENSE)

