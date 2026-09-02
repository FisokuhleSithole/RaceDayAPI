-- =============================================
-- Database: RaceDay
-- Description: Creates the RaceDay database and tables
-- =============================================

USE master;
GO

-- Drop database if exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDay')
BEGIN
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
END
GO

-- Create database
CREATE DATABASE RaceDay;
GO

-- Use the new database
USE RaceDay;
GO

-- =============================================
-- Create Tables
-- =============================================

-- Table 1: User
CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    ProfilePictureUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Table 2: OrganiserProfile
CREATE TABLE OrganiserProfile (
    OrganiserId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE,
    OrganisationName NVARCHAR(255) NULL,
    PhoneNumber NVARCHAR(50) NULL
);
GO

-- Table 3: ParticipantProfile
CREATE TABLE ParticipantProfile (
    ParticipantId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(50) NULL,
    EmergencyContact NVARCHAR(255) NULL
);
GO

-- Table 4: Event
CREATE TABLE [Event] (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    [Date] DATETIME NOT NULL,
    [Location] NVARCHAR(255) NOT NULL,
    RouteDetails NVARCHAR(MAX) NULL,
    EventType NVARCHAR(50) NOT NULL,
    MaxParticipants INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);
GO

-- Table 5: Category
CREATE TABLE Category (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    EntryFee DECIMAL(18, 2) NOT NULL
);
GO

-- Table 6: WeatherInfo
CREATE TABLE WeatherInfo (
    WeatherId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    ForecastDate DATETIME NOT NULL,
    [Condition] NVARCHAR(100) NOT NULL,
    TemperatureCelsius DECIMAL(5, 2) NOT NULL,
    WindSpeedKmph DECIMAL(5, 2) NULL
);
GO

-- Table 7: Enrolment
CREATE TABLE Enrolment (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    ResultTime NVARCHAR(20) NULL,
    [Position] INT NULL
);
GO

-- =============================================
-- Add Foreign Key Constraints
-- =============================================

ALTER TABLE OrganiserProfile
ADD CONSTRAINT FK_OrganiserProfile_User 
FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE CASCADE;
GO

ALTER TABLE ParticipantProfile
ADD CONSTRAINT FK_ParticipantProfile_User 
FOREIGN KEY (UserId) REFERENCES [User](UserId) ON DELETE CASCADE;
GO

ALTER TABLE [Event]
ADD CONSTRAINT FK_Event_OrganiserProfile 
FOREIGN KEY (OrganiserId) REFERENCES OrganiserProfile(OrganiserId) ON DELETE CASCADE;
GO

ALTER TABLE Category
ADD CONSTRAINT FK_Category_Event 
FOREIGN KEY (EventId) REFERENCES [Event](EventId) ON DELETE CASCADE;
GO

ALTER TABLE WeatherInfo
ADD CONSTRAINT FK_WeatherInfo_Event 
FOREIGN KEY (EventId) REFERENCES [Event](EventId) ON DELETE CASCADE;
GO

ALTER TABLE Enrolment
ADD CONSTRAINT FK_Enrolment_ParticipantProfile 
FOREIGN KEY (ParticipantId) REFERENCES ParticipantProfile(ParticipantId) ON DELETE CASCADE;
GO

ALTER TABLE Enrolment
ADD CONSTRAINT FK_Enrolment_Category 
FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId) ON DELETE CASCADE;
GO

-- =============================================
-- Add Check Constraints
-- =============================================

ALTER TABLE [User]
ADD CONSTRAINT CK_User_Role 
CHECK (Role IN ('Organiser', 'Participant'));
GO

ALTER TABLE [Event]
ADD CONSTRAINT CK_Event_EventType 
CHECK (EventType IN ('Running', 'Walking', 'Cycling'));
GO

ALTER TABLE [Event]
ADD CONSTRAINT CK_Event_MaxParticipants 
CHECK (MaxParticipants > 0);
GO

ALTER TABLE Category
ADD CONSTRAINT CK_Category_EntryFee 
CHECK (EntryFee >= 0);
GO

ALTER TABLE Enrolment
ADD CONSTRAINT CK_Enrolment_Status 
CHECK ([Status] IN ('Pending', 'Confirmed', 'Cancelled'));
GO

-- =============================================
-- Add Indexes
-- =============================================

CREATE INDEX IX_User_Email ON [User](Email);
CREATE INDEX IX_User_Role ON [User](Role);
CREATE INDEX IX_Event_OrganiserId ON [Event](OrganiserId);
CREATE INDEX IX_Event_Date ON [Event]([Date]);
CREATE INDEX IX_Event_EventType ON [Event](EventType);
CREATE INDEX IX_Category_EventId ON Category(EventId);
CREATE INDEX IX_Enrolment_ParticipantId ON Enrolment(ParticipantId);
CREATE INDEX IX_Enrolment_CategoryId ON Enrolment(CategoryId);
CREATE INDEX IX_Enrolment_Status ON Enrolment([Status]);
CREATE INDEX IX_WeatherInfo_EventId ON WeatherInfo(EventId);
GO

-- =============================================
-- Add Trigger to Update UpdatedAt
-- =============================================

CREATE TRIGGER trg_Event_UpdateTimestamp
ON [Event]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [Event]
    SET UpdatedAt = GETDATE()
    FROM [Event] e
    INNER JOIN inserted i ON e.EventId = i.EventId;
END
GO

-- =============================================
-- Verification (Tables created)
-- =============================================

SELECT name AS TableName 
FROM sys.tables 
ORDER BY name;
GO