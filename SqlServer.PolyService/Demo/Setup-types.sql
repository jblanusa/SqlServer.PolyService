-- Use demo database
use __json

IF TYPE_ID('dbo.DataSource') IS NOT NULL
DROP TYPE dbo.DataSource
GO
IF TYPE_ID('dbo.WebService') IS NOT NULL
DROP TYPE dbo.WebService
GO
IF TYPE_ID('dbo.AzureBlob') IS NOT NULL
DROP TYPE dbo.AzureBlob
GO
IF TYPE_ID('dbo.DocumentDB') IS NOT NULL
DROP TYPE dbo.DocumentDB
GO
IF TYPE_ID('dbo.TextAnalytics') IS NOT NULL
DROP TYPE dbo.TextAnalytics
GO
IF TYPE_ID('dbo.AzureSearch') IS NOT NULL
DROP TYPE dbo.AzureSearch
GO
IF TYPE_ID('dbo.Neo4j') IS NOT NULL
DROP TYPE dbo.Neo4j
GO
IF TYPE_ID('dbo.AzureTable') IS NOT NULL
DROP TYPE dbo.AzureTable
GO


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
CREATE TYPE dbo.DocumentDB
EXTERNAL NAME [PolyService].[PolyService.Azure.DocumentDB];
GO

--Creating DocumentDb factory function
CREATE FUNCTION dbo.CreateDocumentDBService (@ds dbo.DataSource)
RETURNS dbo.DocumentDB
AS EXTERNAL NAME [PolyService].[PolyService.Azure.DocumentDB].[CreateDocumentDbService]
GO

--Creating RestWebService type
CREATE TYPE dbo.WebService
EXTERNAL NAME [PolyService].[PolyService.Service.RestWebService];
GO

--Creating generic REST service factory function
CREATE FUNCTION [dbo].[CreateService] (@ds dbo.DataSource)
RETURNS dbo.WebService
AS EXTERNAL NAME [PolyService].[PolyService.Service.RestWebService].[CreateRestService]
GO


-- Creating Blob type
CREATE TYPE dbo.AzureBlob 
EXTERNAL NAME [PolyService].[PolyService.Azure.Blob];
GO

-- Creating TableStorage type
CREATE TYPE dbo.AzureTable
EXTERNAL NAME [PolyService].[PolyService.Azure.Table];
GO

-- Creating AzureSearchIndex type
CREATE TYPE dbo.AzureSearch
EXTERNAL NAME [PolyService].[PolyService.Azure.SearchService];
GO

-- Creating TextAnalytics type
CREATE TYPE dbo.TextAnalytics
EXTERNAL NAME [PolyService].[PolyService.Azure.TextAnalytics];
GO


-------------------
--Creating Neo4j type
CREATE TYPE dbo.Neo4j
EXTERNAL NAME [PolyService].[PolyService.Service.Neo4j];
GO
