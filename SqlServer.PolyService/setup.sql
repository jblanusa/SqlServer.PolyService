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
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'PolyService' AND is_user_defined = 1)
	DROP ASSEMBLY PolyService
GO

CREATE ASSEMBLY PolyService
FROM 'C:\Users\a-jovanp\Documents\GitHub\SqlServer.PolyService\SqlServer.PolyService\bin\Debug\SqlServer.PolyService.dll'
WITH PERMISSION_SET = EXTERNAL_ACCESS
GO

