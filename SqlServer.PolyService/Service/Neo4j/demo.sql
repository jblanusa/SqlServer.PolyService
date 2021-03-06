﻿-- Use demo database
use __json

CREATE TYPE Neo4j
EXTERNAL NAME PolyService.[PolyService.Service.Neo4j];

GO

DECLARE @neo4j AS dbo.Neo4j = N'http://t4.cloudapp.net:7474/db/data/transaction/commit'
SET @neo4j = @neo4j.SetNamePass('ty','ty')

DECLARE @result nvarchar(max);
SET  @result = @neo4j.ExecuteQuery('MATCH (nineties:Movie) RETURN nineties')
SELECT @result
SELECT * FROM OPENJSON(@result, '$.results[0].data') with (title nvarchar(100) '$.row[0].title', released int '$.row[0].released')

GO

DECLARE @neo4j AS dbo.Neo4j = N'http://t4.cloudapp.net:7474/db/data/transaction/commit'
SET @neo4j = @neo4j.SetNamePass('ty','ty')

declare @response nvarchar(max) = ( 
 @neo4j.MatchPattern('(@tom) -[@r1]-> (movie) <-[@r2]- (director)')
			.Node('tom').WithProperty('name','Tom Hanks')
			.Relationship('r1').AsType('ACTED_IN')
			.Relationship('r2').AsType('DIRECTED')
			.Returns('movie.title, director.name, id(director)')
			.Run())

select * FROM openjson(@response, '$.results[0].data') with (title nvarchar(100) '$.row[0]', title nvarchar(100) '$.row[1]', id int '$.row[2]')

GO

DECLARE @neo4j AS dbo.Neo4j = N'http://t4.cloudapp.net:7474/db/data/transaction/commit'
SET @neo4j = @neo4j.SetNamePass('ty','ty')

declare @response nvarchar(max) = (@neo4j.MatchPattern('(@kevin) -[@r]- (co_actor)')			--MatchPattern('(@kevin) -[@r]-> (movie) <-[@r]- (co_actor)')
									 .Node('kevin').WithProperty('name','Kevin Bacon') 
									 .Relationship('r').AsType('ACTED_IN*2')
									 .Returns('id(co_actor),co_actor.name')
									 .Run())

SELECT @response
select * FROM openjson(@response, '$.results[0].data') with (id int '$.row[0]', actor nvarchar(100) '$.row[1]')

GO

DECLARE @neo4j AS dbo.Neo4j = N'http://t4.cloudapp.net:7474/db/data/transaction/commit'
SET @neo4j = @neo4j.SetNamePass('ty','ty')

declare @response nvarchar(max) = (@neo4j.MatchPattern('(actor) -[@r]-> (movie)') 
										.Relationship('r').AsType('ACTED_IN')
										.WithVar('actor, count(movie) as MovieCount')
										.OrderBy('MovieCount DESC')
										.Limit(10)
										.Returns('id(actor),actor.name, MovieCount')
										.Run()
										)

SELECT @response
select * FROM openjson(@response, '$.results[0].data') with (id int '$.row[0]', actor nvarchar(100) '$.row[1]', MovieCount int '$.row[2]')

GO
DROP TYPE Neo4j


