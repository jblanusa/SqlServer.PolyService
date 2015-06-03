sp_configure 'show advanced options', 1;
GO
RECONFIGURE;
GO
sp_configure 'clr enabled', 1;
GO
RECONFIGURE;
GO

SET NOCOUNT ON

alter database <database name, nvarchar(max), name> set trustworthy on;

--Dropping functions for compression and decompression
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('TextCompress') AND TYPE ='FS')
DROP FUNCTION TextCompress
go
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('TextDecompress') AND TYPE ='FS')
DROP FUNCTION TextDecompress
go
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('Json2Xml') AND TYPE ='FN')
DROP FUNCTION Json2Xml
go
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('dbo.fn_parse_json2xml') AND TYPE ='FN')
DROP FUNCTION dbo.fn_parse_json2xml
go

-- Dropping UDTs and their handle functions
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('TableStorage') AND TYPE ='FN')
Drop function TableStorage
GO
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('Neo4j') AND TYPE ='FN')
Drop function Neo4j
go
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('AzureSearch') AND TYPE ='FN')
DROP FUNCTION AzureSearch
go
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('TextAnalytics') AND TYPE ='FN')
Drop function TextAnalytics
go
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('GetBlobContent') AND TYPE ='FN')
Drop function GetBlobContent
go
IF EXISTS( SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID('CreateBlob') AND TYPE ='FN')
Drop function CreateBlob
go
IF TYPE_ID('dbo.Blob') IS NOT NULL
DROP TYPE dbo.Blob
GO
IF TYPE_ID('dbo.TextAnalytics') IS NOT NULL
DROP TYPE dbo.TextAnalytics
GO
IF TYPE_ID('dbo.ASearch') IS NOT NULL
DROP TYPE dbo.ASearch
GO
IF TYPE_ID('dbo.Neo4j') IS NOT NULL
DROP TYPE dbo.Neo4j
GO
IF TYPE_ID('dbo.TableStorage') IS NOT NULL
DROP TYPE dbo.TableStorage
GO
----------------------
-- Install Assembly --
----------------------
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'PolyService' AND is_user_defined = 1)
DROP ASSEMBLY PolyService
GO
CREATE ASSEMBLY PolyService
FROM '<assembly path, nvarchar(max), path>'
WITH PERMISSION_SET = EXTERNAL_ACCESS
GO

-------------------
-- Creating UDTs --
-------------------
--Creating Neo4j type
CREATE TYPE dbo.Neo4j
EXTERNAL NAME PolyService.[SqlServer.PolyService.Neo4j];
GO

-- Creating Blob type
CREATE TYPE dbo.Blob 
EXTERNAL NAME PolyService.[SqlServer.PolyService.Blob];
GO

-- Creating ASearch type
CREATE TYPE dbo.ASearch
EXTERNAL NAME PolyService.[SqlServer.PolyService.AzureSearch];
GO

-- Creating TextAnalytics type
CREATE TYPE dbo.TextAnalytics
EXTERNAL NAME PolyService.[SqlServer.PolyService.TextAnalytics];
GO

-- Creating TableStorage type
CREATE TYPE dbo.TableStorage
EXTERNAL NAME PolyService.[SqlServer.PolyService.TableStorage];
GO

----------------------------------------------
-- Creating function handles for those UDTs --
----------------------------------------------
CREATE FUNCTION Neo4j() RETURNS dbo.Neo4j
AS
BEGIN
	DECLARE @srch AS dbo.Neo4j = N'http://localhost:7474/db/data/transaction/commit'

	RETURN @srch.SetNamePass('<neo4j username, nvarchar(max), name>','<neo4j password, nvarchar(max), pass>')
END
GO

CREATE FUNCTION AzureSearch(@indexName NVARCHAR(MAX)) RETURNS dbo.ASearch
AS
BEGIN
	DECLARE @apk AS NVARCHAR(MAX) = '<azure search api-key, nvarchar(max), pass>'

	DECLARE @srch AS dbo.ASearch = N'https://<azure search account name, nvarchar(max), name>.search.windows.net/indexes/'+@indexName+'/docs'

	RETURN @srch.SetApiKey(@apk);
END
GO

CREATE FUNCTION TextAnalytics() RETURNS dbo.TextAnalytics
AS
BEGIN
	DECLARE @analythics as dbo.TextAnalytics = ''

	SET @analythics = @analythics.SetNamePass('<azure marketplace username, nvarchar(max), name>','<azure marketplace account key, nvarchar(max), pass>')

	RETURN @analythics
END
GO

CREATE FUNCTION TableStorage() RETURNS dbo.TableStorage
AS
BEGIN
	DECLARE @apk AS NVARCHAR(MAX) = '<table storage account key, nvarchar(max), pass>'

	DECLARE @srch AS dbo.TableStorage = N'https://<table storage account name, nvarchar(max), name>.table.core.windows.net'

	RETURN @srch.SetAccountKey(@apk);
END
GO

CREATE FUNCTION CreateBlob(@blobName NVARCHAR(MAX), @content VARBINARY(MAX)) RETURNS dbo.Blob
AS
BEGIN
	DECLARE @apk AS NVARCHAR(MAX) = '<blob storage account key, nvarchar(max), pass>'
	SET @blobName = REPLACE(@blobName,' ','')
	SET @blobName = REPLACE(@blobName,'/','')
	DECLARE @res AS dbo.Blob = N'https://<blob storage account name, nvarchar(max), name>.blob.core.windows.net/' + @blobName

	RETURN @res.SetAccountKey(@apk).UploadContent(@content)
END
GO

CREATE FUNCTION GetBlobContent(@blobName NVARCHAR(MAX)) RETURNS varbinary(max)
AS
BEGIN
	DECLARE @apk AS NVARCHAR(MAX) = '<blob storage account key, nvarchar(max), pass>'
	SET @blobName = REPLACE(@blobName,' ','')
	SET @blobName = REPLACE(@blobName,'/','')
	DECLARE @res AS dbo.Blob = N'https://<blob storage account name, nvarchar(max), name>.blob.core.windows.net/' + @blobName

	RETURN @res.SetAccountKey(@apk).Get()
END
GO

-- Creating functions for compression and decompression
CREATE FUNCTION [dbo].[TextCompress] (@input nvarchar(max))
RETURNS varbinary(max)
AS EXTERNAL NAME PolyService.[SqlServer.PolyService.Compress].TextCompress
go

CREATE FUNCTION [dbo].[TextDecompress] (@input varbinary(max))
RETURNS nvarchar(max)
AS EXTERNAL NAME PolyService.[SqlServer.PolyService.Compress].TextDecompress
go

--Taken from http://sqlsunday.com/2013/05/12/converting-json-data-to-xml-using-a-t-sql-function/
CREATE FUNCTION dbo.fn_parse_json2xml(
    @json    varchar(max)
)
RETURNS xml
AS

BEGIN;
    DECLARE @output varchar(max), @key varchar(max), @value varchar(max),
        @recursion_counter int, @offset int, @nested bit, @array bit,
        @tab char(1)=CHAR(9), @cr char(1)=CHAR(13), @lf char(1)=CHAR(10);

    --- Clean up the JSON syntax by removing line breaks and tabs and
    --- trimming the results of leading and trailing spaces:
    SET @json=LTRIM(RTRIM(
        REPLACE(REPLACE(REPLACE(@json, @cr, ''), @lf, ''), @tab, '')));

    --- Sanity check: If this is not valid JSON syntax, exit here.
    IF (LEFT(@json, 1)!='{' OR RIGHT(@json, 1)!='}')
        RETURN '';

    --- Because the first and last characters will, by definition, be
    --- curly brackets, we can remove them here, and trim the result.
    SET @json=LTRIM(RTRIM(SUBSTRING(@json, 2, LEN(@json)-2)));

    SELECT @output='';
    WHILE (@json!='') BEGIN;

        --- Look for the first key which should start with a quote.
        IF (LEFT(@json, 1)!='"')
            RETURN 'Expected quote (start of key name). Found "'+
                LEFT(@json, 1)+'"';

        --- .. and end with the next quote (that isn't escaped with
        --- and backslash).
        SET @key=SUBSTRING(@json, 2,
            PATINDEX('%[^\\]"%', SUBSTRING(@json, 2, LEN(@json))+' "'));

        --- Truncate @json with the length of the key.
        SET @json=LTRIM(SUBSTRING(@json, LEN(@key)+3, LEN(@json)));

        --- The next character should be a colon.
        IF (LEFT(@json, 1)!=':')
            RETURN 'Expected ":" after key name, found "'+
                LEFT(@json, 1)+'"!';

        --- Truncate @json to skip past the colon:
        SET @json=LTRIM(SUBSTRING(@json, 2, LEN(@json)));

        --- If the next character is an angle bracket, this is an array.
        IF (LEFT(@json, 1)='[')
            SELECT @array=1, @json=LTRIM(SUBSTRING(@json, 2, LEN(@json)));

        IF (@array IS NULL) SET @array=0;
        WHILE (@array IS NOT NULL) BEGIN;

            SELECT @value=NULL, @nested=0;
            --- The first character of the remainder of @json indicates
            --- what type of value this is.

            --- Set @value, depending on what type of value we're looking at:
            ---
            --- 1. A new JSON object:
            ---    To be sent recursively back into the parser:
            IF (@value IS NULL AND LEFT(@json, 1)='{') BEGIN;
                SELECT @recursion_counter=1, @offset=1;
                WHILE (@recursion_counter!=0 AND @offset<LEN(@json)) BEGIN;
                    SET @offset=@offset+
                        PATINDEX('%[{}]%', SUBSTRING(@json, @offset+1,
                            LEN(@json)));
                    SET @recursion_counter=@recursion_counter+
                        (CASE SUBSTRING(@json, @offset, 1)
                            WHEN '{' THEN 1
                            WHEN '}' THEN -1 END);
                END;

                SET @value=CAST(
                    dbo.fn_parse_json2xml(LEFT(@json, @offset))
                        AS varchar(max));
                SET @json=SUBSTRING(@json, @offset+1, LEN(@json));
                SET @nested=1;
            END

            --- 2a. Blank text (quoted)
            IF (@value IS NULL AND LEFT(@json, 2)='""')
                SELECT @value='', @json=LTRIM(SUBSTRING(@json, 3,
                    LEN(@json)));

            --- 2b. Other text (quoted, but not blank)
            IF (@value IS NULL AND LEFT(@json, 1)='"') BEGIN;
                SET @value=SUBSTRING(@json, 2,
                    PATINDEX('%[^\\]"%',
                        SUBSTRING(@json, 2, LEN(@json))+' "'));
                SET @json=LTRIM(
                    SUBSTRING(@json, LEN(@value)+3, LEN(@json)));
            END;

            --- 3. Blank (not quoted)
            IF (@value IS NULL AND LEFT(@json, 1)=',')
                SET @value='';

            --- 4. Or unescaped numbers or text.
            IF (@value IS NULL) BEGIN;
                SET @value=LEFT(@json,
                    PATINDEX('%[,}]%', REPLACE(@json, ']', '}')+'}')-1);
                SET @json=SUBSTRING(@json, LEN(@value)+1, LEN(@json));
            END;

            --- Append @key and @value to @output:
            SET @output=@output+@lf+@cr+
                REPLICATE(@tab, @@NESTLEVEL-1)+
                '<'+@key+'>'+
                    ISNULL(REPLACE(
                        REPLACE(@value, '\"', '"'), '\\', '\'), '')+
                    (CASE WHEN @nested=1
                        THEN @lf+@cr+REPLICATE(@tab, @@NESTLEVEL-1)
                        ELSE ''
                    END)+
                '</'+@key+'>';

            --- And again, error checks:
            ---
            --- 1. If these are multiple values, the next character
            ---    should be a comma:
            IF (@array=0 AND @json!='' AND LEFT(@json, 1)!=',')
                RETURN @output+'Expected "," after value, found "'+
                    LEFT(@json, 1)+'"!';

            --- 2. .. or, if this is an array, the next character
            --- should be a comma or a closing angle bracket:
            IF (@array=1 AND LEFT(@json, 1) NOT IN (',', ']'))
                RETURN @output+'In array, expected "]" or "," after '+
                    'value, found "'+LEFT(@json, 1)+'"!';

            --- If this is where the array is closed (i.e. if it's a
            --- closing angle bracket)..
            IF (@array=1 AND LEFT(@json, 1)=']') BEGIN;
                SET @array=NULL;
                SET @json=LTRIM(SUBSTRING(@json, 2, LEN(@json)));

                --- After a closed array, there should be a comma:
                IF (LEFT(@json, 1) NOT IN ('', ',')) BEGIN
                    RETURN 'Closed array, expected ","!';
                END;
            END;

            SET @json=LTRIM(SUBSTRING(@json, 2, LEN(@json)+1));
            IF (@array=0) SET @array=NULL;

        END;
    END;

    --- Return the output:
    RETURN CAST(@output AS xml);
END;
GO

CREATE FUNCTION Json2Xml(@doc nvarchar(max)) RETURNS nvarchar(max)
AS
BEGIN
	SET @doc = REPLACE(@doc,'&','')
	SET @doc = REPLACE(@doc,'@','')
	SET @doc = REPLACE(@doc,'<','')
	SET @doc = REPLACE(@doc,'>','')
	SET @doc = ('<ROOT>'+(CAST(dbo.fn_parse_json2xml(@doc) AS NVARCHAR(MAX)))+'</ROOT>')

	RETURN @doc
END
GO
------------------------------------------------------
---------------- Creating databases ------------------
------------------------------------------------------

IF OBJECT_ID ('dbo.Reviews', 'U') IS NOT NULL
DROP TABLE dbo.Reviews;
GO

Create table dbo.Reviews(
	movieId int,
	movie nvarchar(50),
	review nvarchar(max)
)
Declare @path nvarchar(max)
set @path = '<assembly path, nvarchar(max), path>'
set @path = REPLACE(@path,'Debug\SqlServer.PolyService.dll','SetupFiles\MovieReviews.csv')
DECLARE @sql_bulk varchar(max)
set @sql_bulk = 'BULK INSERT dbo.Reviews
    FROM''' + @path + '''
    WITH
    (
		FIELDTERMINATOR = '','', 
		ROWTERMINATOR = ''\n'', 
		TABLOCK
    )'
EXEC (@sql_bulk)

-- Sending review to blob
DECLARE @suppress nvarchar(max)
UPDATE dbo.Reviews
SET @suppress = dbo.CreateBlob(movie,dbo.TextCompress(SUBSTRING(review,1,3500))).Url 
GO

Declare @path nvarchar(max)
set @path = '<assembly path, nvarchar(max), path>'
set @path = REPLACE(@path,'Debug\SqlServer.PolyService.dll','SetupFiles\MovieGraph.txt')
DECLARE @sql_bulk varchar(max)
set @sql_bulk = '
DECLARE @Query  NVARCHAR(MAX)
set @Query = (select dbo.Neo4j().Query(''Match (a), ()-[r]-() Delete r,a'') )
SELECT @Query = BulkColumn
FROM OPENROWSET(BULK '''+@path+''', SINGLE_CLOB) AS Contents
set @Query = (select dbo.Neo4j().Query(@Query) )'
EXEC (@sql_bulk)
go

-- Creating and populating Table storage table
DECLARE @suppress nvarchar(max)
set @suppress = dbo.TableStorage().CreateTable('PhoneBook')
GO

Declare @path nvarchar(max)
set @path = '<assembly path, nvarchar(max), path>'
set @path = REPLACE(@path,'Debug\SqlServer.PolyService.dll','SetupFiles\PhoneBook1.txt')
DECLARE @sql_bulk varchar(max)
set @sql_bulk = '
DECLARE @phonebook  NVARCHAR(MAX)
SELECT @phonebook = BulkColumn
FROM OPENROWSET(BULK '''+@path+''', SINGLE_CLOB) AS Contents
SET @phonebook =  REPLACE(@phonebook,''@@table_storage_name'',''jovanteststore'')

DECLARE @tbl AS dbo.TableStorage = N''https://<table storage account name, nvarchar(max), name>.table.core.windows.net''
SET @tbl = @tbl.SetAccountKey(''<table storage account key, nvarchar(max), pass>'')
							 .BatchInsert(@phonebook,''batch_a1e9d677-b28b-435e-a89e-87e6a768a431'') '
EXEC (@sql_bulk)							 
GO

Declare @path nvarchar(max)
set @path = '<assembly path, nvarchar(max), path>'
set @path = REPLACE(@path,'Debug\SqlServer.PolyService.dll','SetupFiles\PhoneBook2.txt')
DECLARE @sql_bulk varchar(max)
set @sql_bulk = '
DECLARE @phonebook  NVARCHAR(MAX)
SELECT @phonebook = BulkColumn
FROM OPENROWSET(BULK '''+@path+''', SINGLE_CLOB) AS Contents
SET @phonebook =  REPLACE(@phonebook,''@@table_storage_name'',''jovanteststore'')

DECLARE @tbl AS dbo.TableStorage = N''https://<table storage account name, nvarchar(max), name>.table.core.windows.net''
SET @tbl = @tbl.SetAccountKey(''<table storage account key, nvarchar(max), pass>'')
							 .BatchInsert(@phonebook,''batch_a1e9d677-b28b-435e-a89e-87e6a768a431'') '
EXEC (@sql_bulk)							 
GO

-- Creating and populating Azure Search index
Declare @path nvarchar(max)
set @path = '<assembly path, nvarchar(max), path>'
set @path = REPLACE(@path,'Debug\SqlServer.PolyService.dll','SetupFiles\ReviewsIndex.txt')
DECLARE @sql_bulk varchar(max)
set @sql_bulk = '
DECLARE @index  NVARCHAR(MAX)
SELECT @index = BulkColumn
FROM OPENROWSET(BULK N'''+@path+''', SINGLE_CLOB) AS Contents

DECLARE @srch AS dbo.ASearch = N''https://<azure search account name, nvarchar(max), name>.search.windows.net/indexes?api-version=2015-02-28''
SET @index =  @srch.SetApiKey(''<azure search api-key, nvarchar(max), pass>'').Post(@index)'
EXEC (@sql_bulk)	
GO

Declare @path nvarchar(max)
set @path = '<assembly path, nvarchar(max), path>'
set @path = REPLACE(@path,'Debug\SqlServer.PolyService.dll','SetupFiles\PopulatingIndex1.txt')
DECLARE @sql_bulk varchar(max)
set @sql_bulk = '
DECLARE @index  NVARCHAR(MAX)
SELECT @index = BulkColumn
FROM OPENROWSET(BULK N'''+@path+''', SINGLE_CLOB) AS Contents
DECLARE @srch AS dbo.ASearch = N''https://<azure search account name, nvarchar(max), name>.search.windows.net/indexes/reviews/docs/index?api-version=2015-02-28''
SET @index =  @srch.SetApiKey(''<azure search api-key, nvarchar(max), pass>'').Post(@index)'
EXEC (@sql_bulk)
GO

Declare @path nvarchar(max)
set @path = '<assembly path, nvarchar(max), path>'
set @path = REPLACE(@path,'Debug\SqlServer.PolyService.dll','SetupFiles\PopulatingIndex2.txt')
DECLARE @sql_bulk varchar(max)
set @sql_bulk = '
DECLARE @index  NVARCHAR(MAX)
SELECT @index = BulkColumn
FROM OPENROWSET(BULK N'''+@path+''', SINGLE_CLOB) AS Contents
DECLARE @srch AS dbo.ASearch = N''https://<azure search account name, nvarchar(max), name>.search.windows.net/indexes/reviews/docs/index?api-version=2015-02-28''
SET @index =  @srch.SetApiKey(''<azure search api-key, nvarchar(max), pass>'').Post(@index)'
EXEC (@sql_bulk)
GO

Declare @path nvarchar(max)
set @path = '<assembly path, nvarchar(max), path>'
set @path = REPLACE(@path,'Debug\SqlServer.PolyService.dll','SetupFiles\PopulatingIndex3.txt')
DECLARE @sql_bulk varchar(max)
set @sql_bulk = '
DECLARE @index  NVARCHAR(MAX)
SELECT @index = BulkColumn
FROM OPENROWSET(BULK N'''+@path+''', SINGLE_CLOB) AS Contents
DECLARE @srch AS dbo.ASearch = N''https://<azure search account name, nvarchar(max), name>.search.windows.net/indexes/reviews/docs/index?api-version=2015-02-28''
SET @index =  @srch.SetApiKey(''<azure search api-key, nvarchar(max), pass>'').Post(@index)'
EXEC (@sql_bulk)
GO

Declare @path nvarchar(max)
set @path = '<assembly path, nvarchar(max), path>'
set @path = REPLACE(@path,'Debug\SqlServer.PolyService.dll','SetupFiles\PopulatingIndex4.txt')
DECLARE @sql_bulk varchar(max)
set @sql_bulk = '
DECLARE @index  NVARCHAR(MAX)
SELECT @index = BulkColumn
FROM OPENROWSET(BULK N'''+@path+''', SINGLE_CLOB) AS Contents
DECLARE @srch AS dbo.ASearch = N''https://<azure search account name, nvarchar(max), name>.search.windows.net/indexes/reviews/docs/index?api-version=2015-02-28''
SET @index =  @srch.SetApiKey(''<azure search api-key, nvarchar(max), pass>'').Post(@index)'
EXEC (@sql_bulk)
GO