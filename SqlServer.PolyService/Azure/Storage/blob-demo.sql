
-- Use demo database
use __json


-- Creating BlobStorage type
CREATE TYPE dbo.AzureBlob
EXTERNAL NAME PolyService.[PolyService.Azure.Blob];

go
-- Upload content to blob
DECLARE @res AS dbo.AzureBlob = N'https://ty.blob.core.windows.net/first/info.txt' 
SELECT @res.SetAccountKey('hb5qy6eXLqIrTHDvWjUZg3Gu7bubKLg==').UploadContent(CAST('Lorem ipsum' AS VARBINARY(MAX)))

go
-- Get content from blob
DECLARE @res AS dbo.AzureBlob = N'https://ty.blob.core.windows.net/first/info.txt'
SELECT CAST(@res.SetAccountKey('hb5qy6eXLqIrTHDvWjUZg3Gu7bubKLg==').Get()  AS VARCHAR(MAX))

go
DROP TYPE dbo.AzureBlobStorage


