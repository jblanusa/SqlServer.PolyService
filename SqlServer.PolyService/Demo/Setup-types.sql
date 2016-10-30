-- Use demo database
use PolyServiceDb

-------------------
-- Creating UDTs --
-------------------

--Creating DataSource type
CREATE TYPE dbo.DataSource
EXTERNAL NAME [PolyService].[PolyService.Common.DataSource];
GO

--Creating OData Service type
CREATE TYPE dbo.OData
EXTERNAL NAME [PolyService].[PolyService.Service.OData];
GO

--Creating DocumentDB Service type
CREATE TYPE Azure.DocumentDB
EXTERNAL NAME [PolyService].[PolyService.Azure.DocumentDB];
GO

--Creating DocumentDb factory function
CREATE FUNCTION dbo.CreateDocumentDBService (@ds dbo.DataSource)
RETURNS Azure.DocumentDB
AS EXTERNAL NAME [PolyService].[PolyService.Azure.DocumentDB].[CreateDocumentDbService]
GO

CREATE TYPE dbo.RestService 
EXTERNAL NAME PolyService.[PolyService.Service.RestWebService];
GO

--Creating WebService type
CREATE TYPE dbo.WebService
EXTERNAL NAME [PolyService].[PolyService.Service.WebService];
GO

--Creating generic REST service factory function
CREATE FUNCTION [dbo].[CreateService] (@ds dbo.DataSource)
RETURNS dbo.WebService
AS EXTERNAL NAME [PolyService].[PolyService.Service.RestWebService].[CreateRestService]
GO


-- Creating Blob type
CREATE TYPE Azure.BlobStorage
EXTERNAL NAME [PolyService].[PolyService.Azure.Blob];
GO

-- Creating TableStorage type
CREATE TYPE Azure.TableStorage
EXTERNAL NAME [PolyService].[PolyService.Azure.Table];
GO

-- Creating AzureSearchIndex type
CREATE TYPE Azure.Search
EXTERNAL NAME [PolyService].[PolyService.Azure.SearchService];
GO

-- Creating TextAnalytics type
CREATE TYPE Azure.TextAnalytics
EXTERNAL NAME [PolyService].[PolyService.Azure.TextAnalytics];
GO


-------------------
--Creating Neo4j type
CREATE TYPE dbo.Neo4j
EXTERNAL NAME [PolyService].[PolyService.Service.Neo4j];
GO
