sp_configure 'show advanced options', 1;
GO
RECONFIGURE;
GO
sp_configure 'clr enabled', 1;
GO
RECONFIGURE;
GO

SET NOCOUNT ON
CREATE DATABASE __json
GO
ALTER DATABASE __json SET TRUSTWORTHY ON;
GO
use __json

----------------------
-- Install Assembly --
----------------------
DROP ASSEMBLY IF  EXISTS PolyService
GO

CREATE ASSEMBLY PolyService
FROM 'C:\Users\a-jovanp\Documents\GitHub\SqlServer.PolyService\SqlServer.PolyService\bin\Debug\SqlServer.PolyService.dll'
WITH PERMISSION_SET = EXTERNAL_ACCESS
GO

