

--Dropping functions for compression and decompression
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('TextCompress') AND TYPE ='FS')
DROP FUNCTION TextCompress
go
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('TextDecompress') AND TYPE ='FS')
DROP FUNCTION TextDecompress
go


-- Creating functions for compression and decompression
CREATE FUNCTION [dbo].[TextCompress] (@input nvarchar(max))
RETURNS varbinary(max)
AS EXTERNAL NAME PolyService.[SqlServer.PolyService.Compress].TextCompress
go

CREATE FUNCTION [dbo].[TextDecompress] (@input varbinary(max))
RETURNS nvarchar(max)
AS EXTERNAL NAME PolyService.[SqlServer.PolyService.Compress].TextDecompress
go
GO