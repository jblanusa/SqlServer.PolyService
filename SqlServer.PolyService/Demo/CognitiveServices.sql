
declare @apiKey varchar(40) = '<<key>>'

declare @request NVARCHAR(MAX) = 
(SELECT top 100 'en' as language, id, cast(text as varchar(4000)) as text
FROM Comments
WHERE [Text] IS NOT NULL
FOR JSON PATH, ROOT('documents'))

print @request

declare @curl AS RestService = 
	'https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment'

declare @sentiments NVARCHAR(MAX) = @curl
       .AddHeader('Content-Type','application/json')
       .AddHeader('Ocp-Apim-Subscription-Key', @apiKey)
       .Post(@request)

print @sentiments
select *
from OPENJSON(@sentiments, '$.documents')
              WITH (id int, score float)


set @curl = 
	'https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases'

declare @keyPhrases NVARCHAR(MAX) = @curl
       .AddHeader('Content-Type','application/json')
       .AddHeader('Ocp-Apim-Subscription-Key',@apiKey)
       .Post(@request)




update Comments
set KeyPhrases = phrases.keyPhrases,
	Sentiment = sentiments.score
from Comments
	join OPENJSON(@keyPhrases, '$.documents')
              WITH (id int, keyPhrases NVARCHAR(MAX) AS JSON) as phrases
			  on Comments.ID = phrases.id
	join OPENJSON(@sentiments, '$.documents')
              WITH (id int, score float) as sentiments
			  on Comments.ID = sentiments.id		  


select * from comments