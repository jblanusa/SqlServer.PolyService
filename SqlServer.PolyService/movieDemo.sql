-------------------------------------------------------------------
---	Creating temp table that from Person table in Table Storage ---
-------------------------------------------------------------------

Declare @numbers nvarchar(max),
		@idoc int

Set @numbers = dbo.Json2Xml(
					  dbo.TableStorage().Returns('ActorID, Actor, PhoneNum')
										.FromTable('PhoneBook')
										.Get()
							 )
							 
EXEC sp_xml_preparedocument @idoc OUTPUT, @numbers; 
SELECT * INTO #PhoneTemp
FROM
	OPENXML(@idoc,'/ROOT/value',2)
		WITH (actor_id int  'ActorID',
			  actor nvarchar(max) 'Actor',
			  phone_num nvarchar(max) 'PhoneNum'
			)
EXEC sp_xml_removedocument @idoc;

select * from #PhoneTemp
Order by actor_id

---------------------------------------------
-- Text Analytics over Movie Reviews --------
---------------------------------------------

--Movie reviews are stored in blob
select *, dbo.TextAnalytics().GetSentiment(
								SUBSTRING(
									dbo.TextDecompress(dbo.GetBlobContent(Movie))
														,1,500)
											) as ReviewGrade
from dbo.Reviews

---------------------------------------------
-- Azure Search
---------------------------------------------

DECLARE @doc AS NVARCHAR(MAX),
		@idoc as int

SET @doc = dbo.Json2Xml(
			dbo.AzureSearch('reviews')
						.Search('World War II')
						.SearchModeAll()
						.Get()
						)

EXEC sp_xml_preparedocument @idoc OUTPUT, @doc; 

	SELECT *,dbo.TextDecompress(dbo.GetBlobContent(movie)) as Review
	FROM OPENXML(@idoc,'/ROOT/value',2)
         WITH (movieId int,
		       movie nvarchar(40),
			   score float 'search.score'
				)
EXEC sp_xml_removedocument @idoc;

---------------------------------------
-- Neo4j and Table Storage
---------------------------------------

DECLARE @doc as nvarchar(max),
		@idoc as int;

SET @doc = dbo.Json2Xml(
						  dbo.Neo4j().MatchPattern('(movie) <-[@r1]- (director)')
									 .Relationship('r1').AsType('DIRECTED')
									 .Returns('movie.title, director.name, id(director)')
									 .Run()
						 )

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


SELECT t1.movie, t1.director, t2.phone_num
FROM MovieData AS t1
	JOIN #PhoneTemp AS t2 On t2.actor = t1.director

EXEC sp_xml_removedocument @idoc;
GO

---------------------------------------
-- All in One Example
---------------------------------------

IF OBJECT_ID ('tempdb..#MovieTemp') IS NOT NULL
DROP TABLE #MovieTemp;
GO

DECLARE @search AS NVARCHAR(MAX),
		@graph  AS NVARCHAR(MAX),
		@isr    as int,
		@igr    as int

DECLARE @movie as nvarchar(50)

--Searching top result movie for some keyword
SET @search = dbo.Json2Xml(
						  dbo.AzureSearch('reviews')
								     .Search('World War II')    --Santa, revenge, Christian Bale, World War II
									 .SearchModeAll()
									 .Skip(1)
									 .Get()
						  )

EXEC sp_xml_preparedocument @isr OUTPUT, @search; 
SET @movie = (
				SELECT TOP 1 movie
				FROM OPENXML(@isr,'/ROOT/value',2)
				WITH (movie nvarchar(40))
			 );
EXEC sp_xml_removedocument @isr;

--Searching for actors and directors related to that movie
SET @graph = dbo.Json2Xml(
						  dbo.Neo4j().MatchPattern('(actor)-[@r1]->(@movie)<-[@r2]-(director)')
									 .Node('movie').WithProperty('title',@movie)
									 .Relationship('r1').AsType('ACTED_IN')
									 .Relationship('r2').AsType('DIRECTED')
									 .Returns('movie.title, director.name, id(director), actor.name, id(actor)')
									 .Run()
						 )

EXEC sp_xml_preparedocument @igr OUTPUT, @graph; 

	SELECT * INTO #MovieTemp
	 FROM
		OPENXML(@igr,'/ROOT/results/data',2)
         WITH (
			   movie nvarchar(max) 'row[1]',
			   director nvarchar(max) 'row[2]',
			   director_id int  'row[3]',
			   actor nvarchar(max) 'row[4]',
			   actor_id int  'row[5]'
			   )


SELECT @movie as movie, dbo.TextDecompress(dbo.GetBlobContent(@movie)) as Review, dbo.TextAnalytics().GetSentiment(SUBSTRING(dbo.TextDecompress(dbo.GetBlobContent(@movie)),1,500)) as ReviewGrade 

SELECT DISTINCT t1.movie, t1.actor, t2.phone_num as ActorPhoneNumber 
FROM #MovieTemp AS t1
	JOIN #PhoneTemp AS t2 On t2.actor = t1.actor

SELECT DISTINCT t1.movie, t1.director, t2.phone_num as DirectorPhoneNumber 
FROM #MovieTemp AS t1
	JOIN #PhoneTemp AS t2 On t2.actor = t1.director

EXEC sp_xml_removedocument @igr;
