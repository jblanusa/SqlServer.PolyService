USE [json]
GO

/****** Object:  UserDefinedType [dbo].[DocumentDb]    Script Date: 7/10/2015 10:29:46 PM ******/
CREATE TYPE dbo.OData
EXTERNAL NAME [PolyService].[PolyService.Service.OData]

go

declare @odata as dbo.OData = N'http://services.odata.org/V4/Northwind/Northwind.svc/Regions'
select @odata.Get()
go

declare @odata as dbo.OData = N'http://services.odata.org/V4/Northwind/Northwind.svc/Regions'
select @odata.Filter(N'RegionID eq 1').Get()
go

declare @odata as dbo.OData = N'http://services.odata.org/V4/Northwind/Northwind.svc/Customers'
select @odata
		.[Select](N'CustomerID, CompanyName,Address,City,Country,Phone')
		.Filter(N'Country eq ''Mexico''')
		.OrderBy(N'Phone asc')
		.Skip(2)
		.Take(2)
		.Get()
go


declare @odata as dbo.OData = 'http://services.odata.org/V4/Northwind/Northwind.svc/Orders'
declare @response as nvarchar(max) = @odata.Get()

select *
from openjson(@response, '$.value')
with (OrderID int, CustomerID nvarchar(30), OrderDate datetime2,
ShipName nvarchar(300), ShipAddress nvarchar(300), ShipCity nvarchar(300), ShipCountry nvarchar(300), ShippedDate datetime)
GO




DROP TYPE dbo.OData


