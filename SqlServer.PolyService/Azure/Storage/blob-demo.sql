
-- Use demo database
use __json


-- Creating BlobStorage type
CREATE TYPE Azure.Blob
EXTERNAL NAME PolyService.[PolyService.Azure.Blob];

go
-- Upload content to blob
DECLARE @res AS Azure.Blob = N'https://ty.blob.core.windows.net/first/info.txt' 
SELECT @res.SetAccountKey('hb5qy6eXLqIrTHDvWjUZg3Gu7bubKLg==').UploadContent(CAST('Lorem ipsum' AS VARBINARY(MAX)))

go
-- Get content from blob
DECLARE @res AS Azure.Blob = N'https://ty.blob.core.windows.net/first/info.txt'
SELECT CAST(@res.SetAccountKey('hb5qy6eXLqIrTHDvWjUZg3Gu7bubKLg==').Get()  AS VARCHAR(MAX))

go
DROP TYPE Azure.BlobStorage


