
-- Use demo database
use __json

-- Creating TableStorage type
CREATE TYPE dbo.AzureTableStorage
EXTERNAL NAME PolyService.[PolyService.Azure.Table];

go

--list tables
declare @ts as dbo.AzureTableStorage = 'https://ty.table.core.windows.net/';
set @ts = @ts.SetAccountKey('hb5qy6eXLqIrTHDvWjUZg3Gu7bubKLg=='); 
select TableName from openjson(@ts.ListTables(), '$.value') with (TableName nvarchar (max))


go
-- create and drop table
declare @ts as dbo.AzureTableStorage = 'https://ty.table.core.windows.net/';
set @ts = @ts.SetAccountKey('hb5qy6eXLqIrTHDvWjUZg3Gu7bubKLg=='); 
select @ts.CreateTable('article2')
select TableName from openjson(@ts.ListTables(), '$.value') with (TableName nvarchar (max))
select @tstorage.FromTable('article').Get()
select @ts.DeleteTable('article2')
select TableName from openjson(@ts.ListTables(), '$.value') with (TableName nvarchar (max))


go

-- Insert single row into table storage
declare @ts as dbo.AzureTableStorage = 'https://ty.table.core.windows.net/article';
set @ts = @ts.SetAccountKey('hb5qy6eXLqIrTHDvWjUZg3Gu7bubKLg=='); 
select @ts.Post('{
   "Address":"Mountain View",
   "Age":23,
   "AmountDue":200.23,
   "CustomerCode@odata.type":"Edm.Guid",
   "CustomerCode":"c9da6455-213d-42c9-9a79-3e9149a57833",
   "CustomerSince@odata.type":"Edm.DateTime",
   "CustomerSince":"2008-07-10T00:00:00",
   "IsActive":true,
   "NumberOfOrders@odata.type":"Edm.Int64",
   "NumberOfOrders":"255",
   "PartitionKey":"article",
   "RowKey":"3"
}')
go

-- Insert single row generated with FOR JSON PATH into table storage
declare @ts as dbo.AzureTableStorage = 'https://ty.table.core.windows.net/article';
set @ts = @ts.SetAccountKey('hb5qy6eXLqIrTHDvWjUZg3Gu7bubKLg=='); 
select @ts.Post( (SELECT '1' as RowKey, '2' AS PartitionKey, 'SQL Server' AS Product FOR JSON PATH) )
go

-- Insert typed values into table storage
declare @ts as dbo.AzureTableStorage = 'https://ty.table.core.windows.net/';
set @ts = @ts.SetAccountKey('hb5qy6eXLqIrTHDvWjUZg3Gu7bubKLg==');  
select @ts.IntValue('Name', 2).InsertInto('article')

Set @ts = @ts.Value('Name','Marko')
					     .Value('Lastname','Markovic')
						 .IntValue('Age',26)
						 .InsertInto('article')

Set @ts = @ts.Value('Name','Jelena')
					     .Value('Lastname','Jelenovic')
						 .IntValue('Age',14)
						 .InsertInto('article')

Set @ts = @ts.Value('Name','Pera')
					     .Value('Lastname','Peric')
						 .IntValue('Age',49)
						 .InsertInto('article')

go

-- Query table storage via OData interface
declare @ts as dbo.AzureTableStorage = 'https://ty.table.core.windows.net/';
set @ts = @ts.SetAccountKey('hb5qy6eXLqIrTHDvWjUZg3Gu7bubKLg=='); 
declare @response nvarchar(max) = (@ts.FromTable('PolyService').Filter('Age le 30').Get());
select * from openjson(@response, '$.value')

GO

DROP TYPE dbo.AzureTableStorage


