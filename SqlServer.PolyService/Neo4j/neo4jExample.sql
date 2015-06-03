-------------------------------------------------------------------
---	Creating temp table that from Person table in Table Storage ---
-------------------------------------------------------------------

Declare @numbers nvarchar(max),
		@idoc int

Set @numbers = dbo.Json2Xml(
					  dbo.TableStorage().Returns('ActorID, PhoneNum')
										.FromTable('Person')
										.Get()
							 )
							 
EXEC sp_xml_preparedocument @idoc OUTPUT, @numbers; 
SELECT * INTO #PhoneTemp
FROM
	OPENXML(@idoc,'/ROOT/value',2)
		WITH (actor_id int  'ActorID',
			  phone_num nvarchar(max) 'PhoneNum'
			)
EXEC sp_xml_removedocument @idoc;

select * from #PhoneTemp
Order by actor_id

-----------------------------------------------------------------------------------------
---	EXAMPLE: Let find directors of the movies that directed movies Tom Hanks acted in ---
-----------------------------------------------------------------------------------------

DECLARE @doc as nvarchar(max),
		@idoc as int;

-- Querying Neo4j graph base
SET @doc = dbo.Json2Xml(
						  dbo.Neo4j().MatchPattern('(@tom) -[@r1]-> (movie) <-[@r2]- (director)')
									 .Node('tom').WithProperty('name','Tom Hanks')
									 .Relationship('r1').AsType('ACTED_IN')
									 .Relationship('r2').AsType('DIRECTED')
									 .Returns('movie.title, director.name, id(director)')
									 .Run()
						 )

-- preparing document
EXEC sp_xml_preparedocument @idoc OUTPUT, @doc; 

WITH
MovieData(movie, director, director_id) AS (
	SELECT * FROM
		OPENXML(@idoc,'/ROOT/results/data',2)
         WITH (
			   movie nvarchar(max) 'row[1]',
			   director nvarchar(max) 'row[2]',
			   director_id int  'row[3]')
)


-- Joining data from Neo4j and Table Storage
SELECT t1.movie, t1.director, t2.phone_num
FROM MovieData AS t1
	JOIN #PhoneTemp AS t2 On t2.actor_id = t1.director_id

-- removing document
EXEC sp_xml_removedocument @idoc;
GO

------------------------------------------------------
---	EXAMPLE: Let find coactors for the given actor ---
------------------------------------------------------

DECLARE @doc as nvarchar(max),
		@idoc as int;

-- Querying Neo4j graph base
SET @doc = dbo.Json2Xml(
						  dbo.Neo4j().MatchPattern('(@kevin) -[@r]- (co_actor)')
									 .Node('kevin').WithProperty('name','Kevin Bacon') 
									 .Relationship('r').AsType('ACTED_IN*2')
									 .Returns('id(co_actor),co_actor.name')
									 .Run()
						 )

-- preparing document
EXEC sp_xml_preparedocument @idoc OUTPUT, @doc; 

WITH
ActorData(coactor_id, co_actor) AS (
	SELECT * FROM
		OPENXML(@idoc,'/ROOT/results/data',2)
         WITH (
			   coactor_id int 'row[1]',
			   co_actor nvarchar(max) 'row[2]')
)
-- Joining data from Neo4j and Table Storage
SELECT t1.co_actor, t2.phone_num
FROM ActorData AS t1
	JOIN #PhoneTemp AS t2 On t1.coactor_id = t2.actor_id

-- removing document
EXEC sp_xml_removedocument @idoc;
GO

------------------------------------------------------------------
---	EXAMPLE: find 10 actors with the most movies they acted in ---
------------------------------------------------------------------

DECLARE @actors as nvarchar(max),
        @idoc int;

-- Querying Neo4j graph base
SET @actors = dbo.Json2Xml(
							dbo.Neo4j().MatchPattern('(actor) -[@r]-> (movie)') 
										.Relationship('r').AsType('ACTED_IN')
										.WithVar('actor, count(movie) as MovieCount')
										.OrderBy('MovieCount DESC')
										.Limit(10)
										.Returns('id(actor),actor.name, MovieCount')
										.Run()
						 )

-- preparing document
EXEC sp_xml_preparedocument @idoc OUTPUT, @actors; 

WITH
ActorData(actor_id, actor, movie_count) AS (
	SELECT * FROM
	OPENXML(@idoc,'/ROOT/results/data',2)
        WITH (actor_id int 'row[1]',
			actor nvarchar(max) 'row[2]',
			movie_count int 'row[3]')
)

-- Joining data from Neo4j and Table Storage
SELECT t1.actor, t1.movie_count, t2.phone_num
FROM ActorData AS t1
	JOIN #PhoneTemp AS t2 On t1.actor_id = t2.actor_id

-- removing document
EXEC sp_xml_removedocument @idoc;
GO