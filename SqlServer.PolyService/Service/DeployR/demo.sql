-- Use demo database
use __json

CREATE TYPE DeployR
EXTERNAL NAME PolyService.[PolyService.Service.DeployR];

GO

DECLARE @deployR AS dbo.DeployR = N'http://10.190.13.50:7400/deployr/r/project/execute/code'
SET @deployR = @deployR.SetProject('PROJECT-b00a533f-f0f9-4be2-847c-5a4c4156e392').SetCookie('2700D5199AA22A680E8DDB6F1B90EB69')

DECLARE @json NVARCHAR(MAX) =
			 @deployR.RObjects('zbir,x_numeric_vector')
						.Inputs('{"input_x":{"type":"primitive","value":20},"input_y":{"type":"primitive","value":500}}')
						.ExecuteQuery(
'require(RevoScriptTools)

revoInput(''{""name"":""input_x"",""default"":2,""render"":""integer""}'')
revoInput(''{""name"":""input_y"",""default"":5,""render"":""integer""}'')

zbir <- input_x + input_y
x_numeric_vector <- as.double(c(10:25))
');

SELECT * FROM OPENJSON(@json, '$.deployr.response.workspace.objects')
WITH (name nvarchar(100), value nvarchar(max))

GO
DROP TYPE dbo.DeployR


