﻿
## Linea Nft Indexer

A nft Indexer for linea blockchain


## Env config

In the appsettings replace BlockchainDatabase, RpcUrl,IpfsGateway by your own.

Or you can create appsettings.Development.json with your own env to launch in local


## Launch

[Install dotnet 7 sdk](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

launch project in local

```dotnet run```


## Scaffold database

Install postgressql with pgadmin in local

<!-- add new database migration -->
dotnet ef migrations add CreateDatabase

<!-- update database after migration -->
dotnet ef database update