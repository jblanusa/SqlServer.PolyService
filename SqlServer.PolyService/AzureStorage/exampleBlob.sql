--Creating Test Table

CREATE TABLE test_table (
ID int,
blob Blob,
)
GO


--  CreateBlob function creates new Blob with given account name, account Key and binary content of that blob
--  CreateBlobFromString function creates new Blob with given account name, account Key and string content of that blob

declare @accKey  nvarchar(max) = '<blob storage account key, nvarchar(max),  pass>'
declare @accName nvarchar(max) = '<blob storage account name, nvarchar(max), name>'

--  Inserting some blobs in table

INSERT INTO test_table (ID,blob)   VALUES(1,dbo.CreateBlobFromString(@accName,@accKey,'Content of first blob'))
INSERT INTO test_table (ID,blob)   VALUES(2,dbo.CreateBlobFromString(@accName,@accKey,'Content of second blob'))
INSERT INTO test_table (ID,blob)   VALUES(3,dbo.CreateBlobFromString(@accName,@accKey,'Content of third blob'))
INSERT INTO test_table (ID,blob)   VALUES(4,dbo.CreateBlobFromString(@accName,@accKey,'Content of fourth blob'))
INSERT INTO test_table (ID,blob)   VALUES(5,dbo.CreateBlobFromString(@accName,@accKey,'Content of fifht blob'))

select ID,CONVERT(NVARCHAR(MAX), blob.Get()) as content from test_table
GO

--  Changing blob content

UPDATE test_table SET blob = blob.UploadContent(CAST('ID of this blob is '+CONVERT(nvarchar,ID) AS VARBINARY(MAX)))
GO
SELECT ID,CONVERT(NVARCHAR(MAX), blob.Get()) AS content FROM test_table
GO

--  Inserting new blob into table

INSERT INTO test_table (ID,blob)  VALUES(6,'')

--  Creating that blob later, and puting some content in it

DECLARE @accKey  NVARCHAR(MAX) = '<blob storage account key, nvarchar(max),  pass>'
DECLARE @accName NVARCHAR(MAX) = '<blob storage account name, nvarchar(max), name>'
UPDATE test_table SET blob = dbo.CreateBlobFromString(@accName,@accKey,'New content') WHERE ID = 6
GO
SELECT ID,CONVERT(NVARCHAR(MAX), blob.Get()) AS content FROM test_table

--Dropping Test Table

DROP TABLE test_table
GO
