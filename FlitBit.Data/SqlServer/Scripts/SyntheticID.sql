------------------------------------------------------------------------------
-- Contains support for the SyntheticID types used by FlitBit Entities.
------------------------------------------------------------------------------

------------------------------------------------------------------------------
-- Create the [SyntheticID] Schema if it doesn't exist
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'SyntheticID')
	BEGIN
	DECLARE @statement NVARCHAR(MAX)
	SET @statement = 'CREATE SCHEMA [SyntheticID]'
	EXEC sp_executesql @statement	
	END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[LStrip]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[LStrip]
	PRINT 'Dropped FUNCTION [SyntheticID].[LStrip]'
	END
GO

-------------------------------------------------------------------------------
-- Strips occurences of a string from the beginning of a string.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[LStrip]
(
	@String VARCHAR(MAX),
	@Chars VARCHAR(255) = ' '
)
RETURNS VARCHAR(MAX)
AS 
	BEGIN
	SELECT @Chars = COALESCE(@Chars, ' ')
	IF LEN(@Chars) = 0
		RETURN LTRIM(@String)
		
	IF @String IS NULL
		RETURN @String
	WHILE PATINDEX('[' + @chars + ']%', @string) = 1
		BEGIN
		SELECT @String = RIGHT(@String,
		LEN(REPLACE(@String, ' ', '|')) - 1)
		END
	RETURN @String
	END
GO
PRINT 'CREATED FUNCTION [SyntheticID].[LStrip]'


IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[NibbleValue]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[NibbleValue]
	PRINT 'Dropped FUNCTION [SyntheticID].[NibbleValue]'
	END
GO
-------------------------------------------------------------------------------
-- Given a hexidecimal digit, returns the integer value.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[NibbleValue] (@hexdigit CHAR(1))
RETURNS INT
AS
	BEGIN
	DECLARE @ret INT
	DECLARE @nibble INT	
	SET @nibble = ASCII(@hexdigit) - 48
	IF @nibble >= 0 AND @nibble < 10 
		BEGIN
		-- includes '[0-9]'
		SET @ret = CAST(@nibble AS TINYINT)
		END
	ELSE IF (@nibble > 16 AND @nibble < 24) OR (@nibble > 48 AND @nibble < 56)
		BEGIN
		-- includes '[A-Za-z]'
		-- only use the first nibble, leaves us with 0x01-0x06
		SET @ret = 9 + (0x0F & @nibble)
		END
	ELSE SET @ret = null
	RETURN @ret
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[NibbleValue]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[IsValidID]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[IsValidID]
	PRINT 'Dropped FUNCTION [SyntheticID].[IsValidID]'
	END
GO
-------------------------------------------------------------------------------
-- Given a id (string), determines if it contains a valid identifier.
--
-- Valid identifiers are sequences of hexidecimal characters.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[IsValidID] (@id VARCHAR(100))
RETURNS BIT
AS
	BEGIN
	IF @id IS NULL GOTO __exit_false
	
	DECLARE @len INT
	DECLARE @i INT
	DECLARE @sum INT
	DECLARE @result CHAR(1)
	DECLARE @nibble INT
	DECLARE @hex CHAR(16)
	SET @len = LEN(@id)
		
	IF (@len > 2 AND SUBSTRING(@id, 1, 1) = '0' AND SUBSTRING(@id, 2, 1) = 'x')
	BEGIN
		SET @id = SUBSTRING(@id, 3, @len - 2) 
		SET @len = @len - 2
	END
	
	SET @i = @len - 1
	SET @sum = 0
	WHILE (@i >= 0)
		BEGIN
		SET @nibble = [SyntheticID].[NibbleValue](SUBSTRING(@id, @i + 1, 1))
		IF @nibble IS NULL GOTO __exit_false -- bad input			
		
		IF (@i % 2 = 1) SET @sum = @sum + ((@nibble * 2) % 0xf)
		ELSE SET @sum = @sum + @nibble
		SET @i = @i - 1
		END
	IF (@sum % 0xf = 0) 
		BEGIN
		SET @result = 1
		GOTO __exit
		END
		
__exit_false:
	SET @result = 0
__exit:
	RETURN @result	
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[IsValidID]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[CalculateCheckDigit]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[CalculateCheckDigit]
	PRINT 'Dropped FUNCTION [SyntheticID].[CalculateCheckDigit]'
	END
GO
-------------------------------------------------------------------------------
-- Given an id (string), calculates a check digit.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[CalculateCheckDigit] (@id VARCHAR(100))
RETURNS CHAR(1)
AS
	BEGIN
	IF @id IS NULL GOTO __exit_null
	
	DECLARE @len INT
	DECLARE @i INT
	DECLARE @sum INT
	DECLARE @result CHAR(1)
	DECLARE @nibble INT
	DECLARE @hex CHAR(16)
	SET @len = LEN(@id)
		
	IF (@len > 2 AND SUBSTRING(@id, 1, 1) = '0' AND SUBSTRING(@id, 2, 1) = 'x')
	BEGIN
		SET @id = SUBSTRING(@id, 3, @len - 2) 
		SET @len = @len - 2
	END
	
	SET @i = 0
	SET @sum = 0
	WHILE (@i < @len)
		BEGIN
		SET @nibble = [SyntheticID].[NibbleValue](SUBSTRING(@id, @i + 1, 1))
		IF @nibble IS NULL GOTO __exit_null -- bad input			
		
		IF (@i % 2 = 1) SET @sum = @sum + ((@nibble * 2) % 0xf)
		ELSE SET @sum = @sum + @nibble
		SET @i = @i + 1
		END
	SET @hex = '0123456789ABCDEF'
	SET @result = SUBSTRING(@hex, (0xf - (@sum % 0xf)) + 1, 1)
	GOTO __exit
	
__exit_null:
	SET @result = NULL
__exit:
	RETURN @result	
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[CalculateCheckDigit]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[HexStrToBigint]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[HexStrToBigint]
	PRINT 'Dropped FUNCTION [SyntheticID].[HexStrToBigint]'
	END
GO
CREATE FUNCTION [SyntheticID].[HexStrToBigint] (@hexstr VARCHAR(22) = null)
RETURNS BIGINT
BEGIN
	IF @hexstr IS NULL GOTO __exit_null
	
	DECLARE @len INT
	DECLARE @i INT
	DECLARE @result BIGINT
	DECLARE @nibble BIGINT
	
	SET @result = 0
	SET @len = LEN(@hexstr)
	-- If the string begins with Ox, chop off the first two chars
	IF (@len > 2 AND SUBSTRING(@hexstr, 1, 1) = '0' AND SUBSTRING(@hexstr, 2, 1) = 'x')
	BEGIN
		SET @hexstr = SUBSTRING(@hexstr, 3, 20) 
		SET @len = LEN(@hexstr)
	END
	SET @i = 0
	WHILE (@i < @len)
		BEGIN
		SET @i = @i + 1
		SET @nibble = [SyntheticID].[NibbleValue](SUBSTRING(@hexstr, @i, 1))
		IF @nibble IS NULL GOTO __exit_null -- bad input				
		SET @result = (/*left shift 4 bits*/@result * 0x10) + @nibble
		END
	GOTO __exit
	
__exit_null:
	SET @result = NULL
__exit:
	RETURN @result
END
GO
PRINT 'Created FUNCTION [SyntheticID].[HexStrToBigint]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[HexStrToVarBinary]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[HexStrToVarBinary]
	PRINT 'Dropped FUNCTION [SyntheticID].[HexStrToVarBinary]'
	END
GO
CREATE FUNCTION [SyntheticID].[HexStrToVarBinary] (@hexstr VARCHAR(8000))
RETURNS VARBINARY(8000) 
AS
	BEGIN
	IF @hexstr IS NULL GOTO __exit_null
	
	DECLARE @input VARCHAR(8000)
	DECLARE @ret VARBINARY(8000)
	DECLARE @chr1 CHAR(1)
	DECLARE @i INT
	DECLARE @len INT	
	DECLARE @nibble INT
	DECLARE @value TINYINT	
	SET @len = LEN(@hexstr)
	-- If the string begins with Ox, chop off the first two chars
	IF (@len > 2 AND SUBSTRING(@hexstr, 2, 1) = 'x' AND SUBSTRING(@hexstr, 1, 1) = '0')
		BEGIN
		SET @len = @len - 2
		SET @input = SUBSTRING(@hexstr, 3, @len) 
		END
	ELSE SET @input = @hexstr		
	
	SET @i = 1	
	SET @ret = CAST('' AS VARBINARY)	
	WHILE (@i <= @len)   
		BEGIN
		SET @nibble = [SyntheticID].[NibbleValue](SUBSTRING(@input, @i, 1))
		IF @nibble IS NOT NULL
			BEGIN
			IF @i % 2 = 1
				SET @value = CAST(@nibble AS TINYINT) * 0x10 -- left shift 4 bits
			ELSE
				SET @ret = @ret + CAST(@value + CAST(@nibble AS TINYINT) AS VARBINARY)
			SET @i = @i + 1				
			END
		ELSE GOTO __exit_null -- bad input
		END		
	GOTO __exit
	
__exit_null:
	SET @ret = NULL
__exit:
	RETURN @ret;
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[HexStrToVarBinary]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[BigintToHexStr]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[BigintToHexStr]
	PRINT 'Dropped FUNCTION [SyntheticID].[BigintToHexStr]'
	END
GO
CREATE FUNCTION [SyntheticID].[BigintToHexStr] (@value BIGINT = 0)
RETURNS VARCHAR(22)
BEGIN
	DECLARE @binary VARBINARY(8)
	SET @binary = CONVERT(VARBINARY(8), @value)
	RETURN UPPER(MASTER.sys.fn_varbintohexsubstring(0,@binary,1,0))	 
END
GO
PRINT 'Created FUNCTION [SyntheticID].[BigintToHexStr]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[IntToHexStr]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[IntToHexStr]
	PRINT 'Dropped FUNCTION [SyntheticID].[IntToHexStr]'
	END
GO
CREATE FUNCTION [SyntheticID].[IntToHexStr] (@value INT = 0)
RETURNS VARCHAR(12)
BEGIN
	DECLARE @binary VARBINARY(4)
	SET @binary = CONVERT(VARBINARY(4), @value)
	RETURN UPPER(MASTER.sys.fn_varbintohexsubstring(0,@binary,1,0))	 
END
GO
PRINT 'Created FUNCTION [SyntheticID].[IntToHexStr]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[NextLinearCongruent31]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[NextLinearCongruent31]
	PRINT 'Dropped FUNCTION [SyntheticID].[NextLinearCongruent31]'
	END
GO
-------------------------------------------------------------------------------
-- Given an ID, generates the next 31 bit identity.
--
-- This is a linear congruential generator. It will produce all values between
-- 1 and 2147483647 without duplication. The order in which values are produced
-- are predictable but appear random; this ordering distributes the values 
-- and improves the btree index performance (better than sequential). -Phillip.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[NextLinearCongruent31]( @id INT )
RETURNS INT
AS
	BEGIN
	DECLARE @next INT
	SET @next = (@id / 2) + (((@id & 1) + ((@id / 8) & 1)) & 1) * 0x40000000
	RETURN @next
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[NextLinearCongruent31]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[NextLinearCongruent7]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[NextLinearCongruent7]
	PRINT 'Dropped FUNCTION [SyntheticID].[NextLinearCongruent7]'
	END
GO
-------------------------------------------------------------------------------
-- Given an ID, generates the next 7 bit identity.
--
-- This is a linear congruential generator. It will produce all values between
-- 1 and 127 without duplication.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[NextLinearCongruent7]( @id TINYINT )
RETURNS TINYINT
AS
	BEGIN
	DECLARE @next TINYINT
	SET @next = (@id / 2) | ((@id ^ @id / 2) & 1) * 0x40
	RETURN @next
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[NextLinearCongruent7]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[NextLinearCongruent8]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[NextLinearCongruent8]
	PRINT 'Dropped FUNCTION [SyntheticID].[NextLinearCongruent8]'
	END
GO
-------------------------------------------------------------------------------
-- Given an ID, generates the next 8 bit identity.
--
-- This is a linear congruential generator. It will produce all values between
-- 1 and 255 without duplication.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[NextLinearCongruent8]( @id TINYINT )
RETURNS TINYINT
AS
	BEGIN
	DECLARE @next TINYINT
	SET @next = (@id / 2) | (((@id ^ @id / 4) & 1) ^ ((@id ^ @id / 8) & 1) ^ ((@id ^ @id / 16) & 1)) * 0x80	
	RETURN @next
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[NextLinearCongruent8]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[NextLinearCongruent16]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[NextLinearCongruent16]
	PRINT 'Dropped FUNCTION [SyntheticID].[NextLinearCongruent16]'
	END
GO
-------------------------------------------------------------------------------
-- Given an ID, generates the next 16 bit identity.
--
-- This is a linear congruential generator. It will produce all values between
-- 1 and 65535 in a semi-random order without duplication.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[NextLinearCongruent16]( @id INT )
RETURNS INT
AS
	BEGIN
	DECLARE @next INT
	SET @next = (@id / 2) | (((@id ^ @id / 4) & 1) ^ ((@id ^ @id / 8) & 1) ^ ((@id ^ @id / 32) & 1)) * 0x8000
	RETURN @next
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[NextLinearCongruent16]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[NextLinearCongruent13]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[NextLinearCongruent13]
	PRINT 'Dropped FUNCTION [SyntheticID].[NextLinearCongruent13]'
	END
GO
-------------------------------------------------------------------------------
-- Given an ID, generates the next 13 bit identity.
--
-- This is a linear congruential generator. It will produce all values between
-- 1 and 8191 in a semi-random order without duplication.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[NextLinearCongruent13]( @id INT )
RETURNS INT
AS
	BEGIN
	DECLARE @next INT
	SET @next = (@id / 2) | (((@id ^ @id / 512) & 1) ^ ((@id ^ @id / 1024) & 1) ^ ((@id ^ @id / 4096) & 1)) * 0x1000
	RETURN @next
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[NextLinearCongruent13]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[NextLinearCongruent15]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[NextLinearCongruent15]
	PRINT 'Dropped FUNCTION [SyntheticID].[NextLinearCongruent15]'
	END
GO
-------------------------------------------------------------------------------
-- Given an ID, generates the next 15 bit identity.
--
-- This is a linear congruential generator. It will produce all values between
-- 1 and 32767 in a semi-random order without duplication.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[NextLinearCongruent15]( @id SMALLINT )
RETURNS SMALLINT
AS
	BEGIN
	DECLARE @next SMALLINT
	SET @next = (@id / 2) + (((@id & 1) + ((@id / 2) & 1)) & 1) * 0x4000
	RETURN @next
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[NextLinearCongruent15]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[NextLinearCongruent48]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[NextLinearCongruent48]
	PRINT 'Dropped FUNCTION [SyntheticID].[NextLinearCongruent48]'
	END
GO
-------------------------------------------------------------------------------
-- Given an ID, generates the next 48 bit identity.
--
-- This is a linear congruential generator. It will produce all values between
-- 1 and 281,474,976,710,655 in a semi-random order without duplication.
-------------------------------------------------------------------------------
CREATE FUNCTION [SyntheticID].[NextLinearCongruent48]( @id BIGINT )
RETURNS BIGINT
AS
	BEGIN
	DECLARE @next BIGINT
	SET @next = (@id / 2) | (((@id ^ @id / 2) & 1) ^ ((@id ^ @id / 0x4000000) & 1) ^ ((@id ^ @id / 0x8000000) & 1)) * 0x800000000000
	RETURN @next
	END
GO
PRINT 'Created FUNCTION [SyntheticID].[NextLinearCongruent48]'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[NextLinearCongruent63]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
	BEGIN
	DROP FUNCTION [SyntheticID].[NextLinearCongruent63]
	PRINT 'Dropped FUNCTION [SyntheticID].[NextLinearCongruent63]'
	END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SyntheticID].[GenerateSyntheticIDGenerator]') AND type in (N'P', N'PC'))
	BEGIN
	DROP PROCEDURE [SyntheticID].[GenerateSyntheticIDGenerator]
	PRINT 'Dropped PROCEDURE [SyntheticID].[GenerateSyntheticIDGenerator]'
	END
GO
CREATE PROCEDURE [SyntheticID].[GenerateSyntheticIDGenerator] 
	@Schema NVARCHAR(128),
	@Table NVARCHAR(128),
	@Column NVARCHAR(128),
	@BitCount INT,
	@IsNumeric BIT,
	@UsesDiscriminator BIT = 0,
	@UsesCheckDigit BIT = 0,
	@Discriminator CHAR(2) = NULL,
	@Seed32 INT = 1,
	@Seed64 BIGINT = 1
AS
	BEGIN
	SET NOCOUNT ON

	DECLARE @statement NVARCHAR(MAX)
	DECLARE @bits NVARCHAR(3)
	DECLARE @generator_table NVARCHAR(2000)
	DECLARE @schema_table_column NVARCHAR(400)
	DECLARE @where_statement NVARCHAR(200)
	DECLARE @generated_type NVARCHAR(100)
	DECLARE @id_type NVARCHAR(100)
	DECLARE @id_conversion_fn NVARCHAR(100)
	DECLARE @seed_as_text NVARCHAR(20)
	
	IF @BitCount NOT IN (7, 8, 13, 15, 16, 31, 48, 63)
		RAISERROR ('Invalid value in @BitCount, valid values are 7, 8, 13, 15, 16, 31, 48, 63', 16, 1)
	
	SET @bits = CONVERT(NVARCHAR(3), @BitCount)
	SET @generator_table = '[' + @Schema + '].[' + @Table + '_' + @Column + ']'		
	SET @schema_table_column = @Schema + '_' + @Table + '_' + @Column
	
	
	IF @BitCount > 31
		BEGIN
		SET @generated_type = 'BIGINT'
		SET @id_conversion_fn = '[SyntheticID].[BigIntToHexStr]'
		SET @id_type = 'BIGINT'
		SET @seed_as_text = CONVERT(NVARCHAR, @Seed64)
		END
	ELSE 
		BEGIN
		SET @generated_type = 'INT'
		SET @id_conversion_fn = '[SyntheticID].[IntToHexStr]'
		IF @BitCount <= 16 SET @id_type = 'SMALLINT'
		ELSE SET @id_type = 'INT'				
		SET @seed_as_text = CONVERT(NVARCHAR, @Seed32)
		END
	
	IF @UsesDiscriminator = 1
		SET @statement = 'CREATE TABLE ' + @generator_table + '
(	
	-- Enforce SINGLETON record
	[Discriminator] CHAR(2) NOT NULL
		CONSTRAINT PK_' + @schema_table_column + 'Discriminator PRIMARY KEY
		CONSTRAINT DF_' + @schema_table_column + 'Discriminator DEFAULT(''' + @Discriminator + ''')
		CONSTRAINT CK_' + @schema_table_column + 'Discriminator CHECK([Discriminator] = ''' + @Discriminator + '''),'
	ELSE
		SET @statement = 'CREATE TABLE ' + @generator_table + '
(	
	-- Enforce SINGLETON record
	[Singleton] BIT NOT NULL
		CONSTRAINT PK_' + @schema_table_column + 'Singleton PRIMARY KEY
		CONSTRAINT DF_' + @schema_table_column + 'Singleton DEFAULT (1)
		CONSTRAINT CK_' + @schema_table_column + 'Singleton CHECK ([Singleton] = 1),'
		
	SET @statement = @statement + '
	-- Most recent value, used as seed value for function [SyntheticID].[NextLinearCongruent' + @bits + ']
	-- to produce successive semi-random values
	[MostRecent] ' + @generated_type + ' NOT NULL
)
'
	
	IF @UsesDiscriminator = 1
		BEGIN
		SET @statement = @statement + 'INSERT INTO ' + @generator_table + ' VALUES(''' + @Discriminator + ''',' + @seed_as_text + ')'
		SET @where_statement = ' WHERE [Discriminator] = ''' + @Discriminator + ''''
		END
	ELSE
		BEGIN
		SET @statement = @statement + 'INSERT INTO ' + @generator_table + ' VALUES(1,' + @seed_as_text + ')'
		SET @where_statement = ' WHERE [Singleton] = 1'
		END
	EXEC sp_executesql @statement	
	
	SET @statement = 'CREATE PROCEDURE [' + @Schema + '].[' + @Table + '_' + @Column + '_Generator] '
	
	IF @IsNumeric = 1
		SET @statement = @statement + ' @id ' + @id_type + ' OUT'
	ELSE
		SET @statement = @statement + ' @id VARCHAR(20) OUT'
		
		SET @statement = @statement + '
AS
	BEGIN
	SET NOCOUNT ON
	SET XACT_ABORT ON
	DECLARE @flx_returncode INT
	DECLARE @flx_txcount INT
	DECLARE @flx_xactstate INT
	DECLARE @local_id ' + @generated_type + '
	
	SET @flx_returnCode = 0	
	SET @flx_txcount = @@TRANCOUNT
	
	BEGIN TRY
		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
		BEGIN TRANSACTION flx_LocalTrans			
		
		UPDATE ' + @generator_table + ' SET [MostRecent] = [SyntheticID].[NextLinearCongruent' + @bits + ']([MostRecent])' + @where_statement + '
		SELECT @local_id = [MostRecent] FROM ' + @generator_table + @where_statement + '
		
		COMMIT TRANSACTION flx_LocalTrans
		'
		
	IF @IsNumeric = 1
		BEGIN		
		IF @id_type = @generated_type
			SET @statement = @statement + 'SET @id = @local_id
		'
		ELSE
			SET @statement = @statement + 'SET @id = CONVERT(' + @id_type + ', @local_id)
		'
		END
	ELSE
		BEGIN
		IF @UsesDiscriminator = 1
			SET @statement = @statement + 'SET @id = ''' + @Discriminator + ''' + ' + @id_conversion_fn + '(@local_id)
		'
		ELSE
			SET @statement = @statement + 'SET @id = ' + @id_conversion_fn + '(@local_id)
		'
		IF @IsNumeric = 0 AND @UsesCheckDigit = 1
			SET @statement = @statement + 'SET @id = @id + [SyntheticID].[CalculateCheckDigit](@id)'

		END		
	SET @statement = @statement + '		
	END TRY
	BEGIN CATCH
		DECLARE @flx_ErrorMessage NVARCHAR(2047)
		DECLARE @flx_ErrorNumber INT
		DECLARE @flx_ErrorSeverity INT
		DECLARE @flx_ErrorState INT
		
		SELECT @flx_ErrorMessage = ERROR_MESSAGE(),
			@flx_ErrorNumber = ERROR_NUMBER(),
			@flx_ErrorSeverity = ERROR_SEVERITY(),
			@flx_ErrorState = ERROR_STATE()
		
		SET @flx_xactstate = XACT_STATE()		
		IF @flx_xactstate = -1 AND @@TRANCOUNT > @flx_txcount
			ROLLBACK TRANSACTION flx_LocalTrans
		SET TRANSACTION ISOLATION LEVEL READ COMMITTED
		SET @flx_returncode = 1
	
		-- Reraise the error
		RAISERROR (''|%d| %s'', -- formatted for [num] msg
			@flx_ErrorSeverity, -- Severity.
			@flx_ErrorState, -- State.
			@flx_ErrorNumber, -- Message number
			@flx_ErrorMessage -- Message text.        
			) 
	END CATCH	
	
	/* Commit or abort the transaction. */
	SET @flx_xactstate = XACT_STATE()
	IF @flx_xactstate <> 0 AND @@TRANCOUNT > @flx_txcount
		BEGIN
		IF @flx_xactstate = -1
			BEGIN
			ROLLBACK TRANSACTION flx_LocalTrans
			SET @flx_returncode = 1
			END
		ELSE IF @flx_xactstate = 1
			COMMIT TRANSACTION flx_LocalTrans			
		END
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED
	RETURN(@flx_returncode)
	END'
	EXEC sp_executesql @statement	
	END
GO
PRINT 'Created PROCEDURE [SyntheticID].[GenerateSyntheticIDGenerator]'

