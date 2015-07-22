-- Use demo database
use __json

-- Creating AzureSearch type
CREATE TYPE [dbo].AzureSearch
EXTERNAL NAME PolyService.[PolyService.Azure.SearchService]

go

declare @as dbo.AzureSearch = 'https://t21.search.windows.net/indexes/articles/docs/index?api-version=2014-07-31-Preview';
set @as = @as.SetApiKey('9DAC191BB26D7708DC7F2');
select * 
FROM OPENJSON(
		@as.Post('{"value": [ { "@search.action": "upload","id":"1", "title":"Markets ride out Greek storm", "likes":21, "description":"European and Asian stock markets and the euro have fallen after Greece''s \"no\" vote, but by far less than analysts had feared." }]}'),
		'$.value' )
	 WITH ([key] int, status nvarchar(50));
go

declare @as dbo.AzureSearch = 'https://t21.search.windows.net/indexes/articles/docs/index?api-version=2014-07-31-Preview';
set @as = @as.SetApiKey('9DAC191BB26D7708DC7F2');
select json_value(@as.Post('{"value": [{"@search.action": "delete","id":"1"}]}'),'$.value[0].status');
go

declare @as dbo.AzureSearch = 'https://t21.search.windows.net/indexes/articles/docs';
set @as = @as.SetApiKey('9DAC191BB26D7708DC7F2');
declare @json as nvarchar(max) = @as.Search('Greek').Get();
select *
from openjson(@json, '$.value')
with (title nvarchar(5), description nvarchar(500), likes int)
go

declare @as dbo.AzureSearch = 'https://t21.search.windows.net/indexes/articles/docs/index?api-version=2014-07-31-Preview';
set @as = @as.SetApiKey('9DAC191BB26D7708DC7F2');
select @as.Post(
	(select top 10 'upload' as [@search.action], cast(id as nvarchar(5)) as id,
					title, description, CAST(pubDate as datetimeoffset) as pubDate, [fb.likes] as [likes]
	from article
	order by id
	FOR JSON AUTO, ROOT('value') )
)
go

declare @as dbo.AzureSearch = 'https://t21.search.windows.net/indexes/articles/docs';
set @as = @as.SetApiKey('9DAC191BB26D7708DC7F2');
declare @json as nvarchar(max) = @as.Search('California').Get();
select *
from openjson(@json, '$.value')
with (title nvarchar(50), description nvarchar(500), pubDate datetime, likes int)
go

declare @as dbo.AzureSearch = 'https://t21.search.windows.net/indexes/articles/docs/index?api-version=2014-07-31-Preview';
set @as = @as.SetApiKey('9DAC191BB26D7708DC7F2');
SELECT *
FROM OPENJSON(@as.Post(
	(select top 10 'upload' as [@search.action], cast(id as nvarchar(5)) as id,
					title, description, CAST(pubDate as datetimeoffset) as pubDate, 0 as [likes]
	from article
	order by id
	FOR JSON AUTO, ROOT('value'))), '$.value' )
	 WITH ([key] int, status nvarchar(50))

go


declare @as dbo.AzureSearch = 'https://t21.search.windows.net/indexes/articles/docs/index?api-version=2014-07-31-Preview';
set @as = @as.SetApiKey('9DAC191BB26D7708DC7F2');
declare @json as nvarchar(max) = @as.Search('California').Get();
select *
from openjson(@json, '$.value')
with (title nvarchar(50), description nvarchar(500), pubDate datetime, likes int)
go


declare @as dbo.AzureSearch = 'https://t21.search.windows.net/indexes/articles/docs/index?api-version=2014-07-31-Preview';
set @as = @as.SetApiKey('9DAC191BB26D7708DC7F2');
SELECT *
FROM OPENJSON( @as.Post(
	(select top 10 'delete' as [@search.action], cast(id as nvarchar(5)) as id
	from .article
	order by id
	FOR JSON AUTO, ROOT('value'))), '$.value' )
	 WITH ([key] int, status nvarchar(50))

go


