IF DB_ID(N'OHairGanicDB') IS NULL CREATE DATABASE OHairGanicDB;
GO
IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'hosttest')
    CREATE LOGIN [hosttest] WITH PASSWORD = 'Aa123456!', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;
ELSE
    ALTER LOGIN [hosttest] WITH PASSWORD = 'Aa123456!';
GO
USE OHairGanicDB;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'hosttest')
    CREATE USER [hosttest] FOR LOGIN [hosttest];
ALTER ROLE db_owner ADD MEMBER [hosttest];
ALTER USER hosttest WITH DEFAULT_SCHEMA = dbo;
