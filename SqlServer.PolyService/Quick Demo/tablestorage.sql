-- Creating table at table storage
Declare @tstorage as dbo.TableStorage = 'https://<table storage account name ,nvarchar(max), name>.table.core.windows.net'
Set @tstorage = @tstorage.SetAccountKey('<table storage account key ,nvarchar(max), pass>')
select @tstorage.FromTable('PolyService').Get()
Set @tstorage = @tstorage.CreateTable('PolyService')
GO

-- Insert into Table storage table
Declare @tstorage as dbo.TableStorage = 'https://<table storage account name ,nvarchar(max), name>.core.windows.net'
Set @tstorage = @tstorage.SetAccountKey('<table storage account key ,nvarchar(max), pass>')

Set @tstorage = @tstorage.Value('Name','Marko')
					     .Value('Lastname','Markovic')
						 .IntValue('Age',26)
						 .InsertInto('PolyService')

Set @tstorage = @tstorage.Value('Name','Jelena')
					     .Value('Lastname','Jelenovic')
						 .IntValue('Age',14)
						 .InsertInto('PolyService')

Set @tstorage = @tstorage.Value('Name','Pera')
					     .Value('Lastname','Peric')
						 .IntValue('Age',49)
						 .InsertInto('PolyService')
GO

-- Querying Table storage table
Declare @tstorage as dbo.TableStorage = 'https://<table storage account name ,nvarchar(max), name>.table.core.windows.net'
Set @tstorage = @tstorage.SetAccountKey('<table storage account key ,nvarchar(max), pass>')

Declare @doc nvarchar(max),
		@idoc int

Set @doc = @tstorage.Returns('Name, Lastname, Age')
			--		.Take(2)
			--		.Filter('Age ge 30')
					.FromTable('PolyService')
					.Get()


SET @doc = dbo.Json2Xml(@doc)
EXEC sp_xml_preparedocument @idoc OUTPUT, @doc; 
SELECT * 
FROM
	OPENXML(@idoc,'/ROOT/value',2)
		WITH (Name nvarchar(max),
			  Lastname nvarchar(max),
			  Age int
			)
EXEC sp_xml_removedocument @idoc;