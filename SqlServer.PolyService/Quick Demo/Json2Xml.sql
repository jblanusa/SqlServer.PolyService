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