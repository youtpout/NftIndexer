
## Linea Nft Indexer

A nft Indexer for linea blockchain


## How to init
In the appsettings replace BlockchainDatabase,RpcUrl, IpfsGateway by your own
Or you can create appsettings.Development.json with your own env to launch in local


## Scaffold database
Install postgressql with pgadmin in local

<!-- add new database migration -->
dotnet ef migrations add CreateDatabase

dotnet ef database update