sp_configure 'show advanced options', 1;
GO
RECONFIGURE;
GO
sp_configure 'clr enabled', 1;
GO
RECONFIGURE;
GO

SET NOCOUNT ON
CREATE DATABASE PolyServiceDb
GO
ALTER DATABASE PolyServiceDb SET TRUSTWORTHY ON;
GO
use PolyServiceDb

----------------------
-- Install Assembly --
----------------------
DROP ASSEMBLY IF EXISTS PolyService
GO

CREATE ASSEMBLY PolyService
FROM 'C:\Users\jovanpop\Documents\GitHub\SqlServer.PolyService\SqlServer.PolyService\bin\Release\SqlServer.PolyService.dll'
WITH PERMISSION_SET = EXTERNAL_ACCESS
GO

