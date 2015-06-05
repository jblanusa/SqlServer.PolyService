-----------------------------------------
-- To configure press CTRL + SHIFT + M --
-----------------------------------------

sp_configure 'clr enabled', 1
reconfigure

Alter Database <database name ,nvarchar(max), database> set trustworthy on 

-- Drop if exists
IF TYPE_ID('dbo.OData') IS NOT NULL
DROP TYPE dbo.OData
GO

IF TYPE_ID('dbo.TableStorage') IS NOT NULL
DROP TYPE dbo.TableStorage
GO

IF TYPE_ID('dbo.Blob') IS NOT NULL
DROP TYPE dbo.Blob
GO

IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'PolyService' AND is_user_defined = 1)
DROP ASSEMBLY PolyService
GO

-- Create Assembly

CREATE ASSEMBLY PolyService
FROM '<assembly path,nvarchar(max),path>'
WITH PERMISSION_SET = EXTERNAL_ACCESS
GO

-- Create UDTs
CREATE TYPE dbo.TableStorage
EXTERNAL NAME PolyService.[SqlServer.PolyService.TableStorage];
GO

CREATE TYPE dbo.OData 
EXTERNAL NAME PolyService.[SqlServer.PolyService.OData];
GO

CREATE TYPE dbo.Blob 
EXTERNAL NAME PolyService.[SqlServer.PolyService.Blob];
GO

