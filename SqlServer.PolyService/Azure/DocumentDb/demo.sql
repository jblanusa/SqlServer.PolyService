﻿
-- Use demo database
use __json

IF TYPE_ID('dbo.DocumentDB') IS NOT NULL
DROP TYPE dbo.DocumentDB
GO

--Creating DocumentDB Service type
CREATE TYPE dbo.DocumentDB
EXTERNAL NAME PolyService.[PolyService.Azure.DocumentDB];
GO


-- Query DocumentDB
declare @docdb as dbo.DocumentDB = 'Data Source=https://t21.documents.azure.com:443/dbs/dPgThrfgjdrjtAA==/colls/dPgTAKjtyuikguulkfghljhl;kEGwA=;Username=hdI18eZC0tuiE9i14bvGH3Q==';
select * from openjson( @docdb.ExecuteQuery(N'SELECT * FROM books'), '$.Documents')
with (id nvarchar(20), title nvarchar(50), url nvarchar(50), pubDate datetime2, rid nvarchar(100) '$._rid')
go


-- Delete document with id 4
declare @docdb as dbo.DocumentDB = 'Data Source=https://t21.documents.azure.com:443/dbs/dPgThrfgjdrjtAA==/colls/dPgTAKjtyuikguulkfghljhl;kEGwA=;Username=hdI18eZC0tuiE9i14bvGH3Q==';
declare @json as nvarchar(MAX) = @docdb.ExecuteQuery(N'SELECT * FROM books ')
select @docdb.DeleteDocument(rid)
from openjson( @json, '$.Documents') with (id int, rid nvarchar(50) '$._rid')
where id = 5
go

-- create document
declare @docdb as dbo.DocumentDB = 'Data Source=https://t21.documents.azure.com:443/dbs/dPgThrfgjdrjtAA==/colls/dPgTAKjtyuikguulkfghljhl;kEGwA=;Username=hdI18eZC0tuiE9i14bvGH3Q==';
select @docdb.CreateDocument(N'{"id": "4", "title":"JQuery In Action", "price":46,"published":2012}')

go

-- Delete all documents
declare @docdb as dbo.DocumentDB = 'Data Source=https://techready21.documents.azure.com:443/dbs/dPgTAA==/colls/dPgTAKjEGwA=;Username=hdI18zMZ1sMtiL9uv9JgrgsLWeSkLF+MJ9zzc3bja4CfdbeaHG7j5MqpCM4Wy4j1rCmueZC0tuiE9i14bvGH3Q==';
declare @json as nvarchar(MAX) = @docdb.ExecuteQuery(N'SELECT * FROM books ')
select rid, id, @docdb.DeleteDocument(rid)
from openjson( @json, '$.Documents') with (id int, rid nvarchar(50) '$._rid')


go

-- move table to DcoumentDb
declare @docdb as dbo.DocumentDB = 'Data Source=https://t21.documents.azure.com:443/dbs/dPgThrfgjdrjtAA==/colls/dPgTAKjtyuikguulkfghljhl;kEGwA=;Username=hdI18eZC0tuiE9i14bvGH3Q==';
declare @json as nvarchar(max)
set @json =( select top 10 cast(id as nvarchar(20)) as id, title, description, url, pubDate from article for json path)
select @docdb.CreateDocument(value)
from openjson(@json)

GO

DROP TYPE dbo.DocumentDB