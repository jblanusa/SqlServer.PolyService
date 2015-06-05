declare @odata as dbo.OData = N'http://services.odata.org/V4/Northwind/Northwind.svc/Regions'
select @odata.Get()
go

declare @odata as dbo.OData = N'http://services.odata.org/V4/Northwind/Northwind.svc/Regions'
select @odata.Filter(N'RegionID eq 1').Get()
go

declare @odata as dbo.OData = N'http://services.odata.org/V4/Northwind/Northwind.svc/Customers'
select @odata
		.Returns(N'CustomerID, CompanyName,Address,City,Country,Phone')
		.Filter(N'Country eq ''Mexico''')
		.OrderBy(N'Phone asc')
		.Skip(2)
		.Take(2)
		.Get()
go