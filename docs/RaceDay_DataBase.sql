-- =============================================
-- Database: RaceDay
-- Description: Creates and populates the RaceDay database schema.
-- =============================================

USE master;
GO

-- Drop the database if it exists to start clean 
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDay')
BEGIN
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
END
GO

-- Create the new database
CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

-- =============================================
-- Table: User
-- Base table for all users (Organisers and Participants)
-- =============================================
CREATE TABLE [User] (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL CHECK (Role IN ('Organiser', 'Participant')),
    ProfilePictureUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- =============================================
-- Table: OrganiserProfile
-- Extends User for Organisers
-- =============================================
CREATE TABLE OrganiserProfile (
    OrganiserId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES [User](UserId) ON DELETE CASCADE,
    OrganisationName NVARCHAR(255) NULL,
    PhoneNumber NVARCHAR(50) NULL
);
GO

-- =============================================
-- Table: ParticipantProfile
-- Extends User for Participants
-- =============================================
CREATE TABLE ParticipantProfile (
    ParticipantId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES [User](UserId) ON DELETE CASCADE,
    PhoneNumber NVARCHAR(50) NULL,
    EmergencyContact NVARCHAR(255) NULL
);
GO

-- =============================================
-- Table: Event
-- Core entity for races/events
-- =============================================
CREATE TABLE [Event] (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL FOREIGN KEY REFERENCES OrganiserProfile(OrganiserId) ON DELETE CASCADE,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    [Date] DATETIME NOT NULL,
    [Location] NVARCHAR(255) NOT NULL,
    RouteDetails NVARCHAR(MAX) NULL,
    EventType NVARCHAR(50) NOT NULL CHECK (EventType IN ('Running', 'Walking', 'Cycling')),
    MaxParticipants INT NULL CHECK (MaxParticipants > 0),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);
GO

-- =============================================
-- Table: Category
-- Specific entry options for an Event (e.g., 5km, 10km)
-- =============================================
CREATE TABLE Category (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL FOREIGN KEY REFERENCES [Event](EventId) ON DELETE CASCADE,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    EntryFee DECIMAL(18, 2) NOT NULL CHECK (EntryFee >= 0)
);
GO

-- =============================================
-- Table: WeatherInfo
-- Forecast details for an Event
-- =============================================
CREATE TABLE WeatherInfo (
    WeatherId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL FOREIGN KEY REFERENCES [Event](EventId) ON DELETE CASCADE,
    ForecastDate DATETIME NOT NULL,
    [Condition] NVARCHAR(100) NOT NULL,
    TemperatureCelsius DECIMAL(5, 2) NOT NULL,
    WindSpeedKmph DECIMAL(5, 2) NULL
);
GO

-- =============================================
-- Table: Enrolment
-- Links a Participant to a Category for an Event
-- =============================================
CREATE TABLE Enrolment (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL FOREIGN KEY REFERENCES ParticipantProfile(ParticipantId) ON DELETE CASCADE,
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES Category(CategoryId) ON DELETE CASCADE,
    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending' CHECK ([Status] IN ('Pending', 'Confirmed', 'Cancelled')),
    ResultTime NVARCHAR(20) NULL, -- e.g., '01:23:45'
    [Position] INT NULL
);
GO

-- =============================================
-- SEED DATA: Populating the tables with sample data
-- =============================================

-- Seed Users
INSERT INTO [User] (Email, PasswordHash, FirstName, LastName, Role, ProfilePictureUrl) VALUES
('thabo@runjoburg.com', 'hashed_password_1', 'Thabo', 'Nkosi', 'Organiser', NULL),
('sarah@capetowncycle.com', 'hashed_password_2', 'Sarah', 'Van der Merwe', 'Organiser', NULL),
('john.craig@fit.com', 'hashed_password_3', 'John', 'Craig', 'Participant', NULL),
('jane.smith@race.com', 'hashed_password_4', 'Jane', 'Smith', 'Participant', NULL);
GO

-- Seed OrganiserProfiles
INSERT INTO OrganiserProfile (UserId, OrganisationName, PhoneNumber) VALUES
( (SELECT UserId FROM [User] WHERE Email = 'thabo@runjoburg.com'), 'Run Joburg Events', '011-555-0199'),
( (SELECT UserId FROM [User] WHERE Email = 'sarah@capetowncycle.com'), 'Cape Town Cycle Tours', '021-555-0188');
GO

-- Seed ParticipantProfiles
INSERT INTO ParticipantProfile (UserId, PhoneNumber, EmergencyContact) VALUES
( (SELECT UserId FROM [User] WHERE Email = 'john.craig@fit.com'), '082-555-0123', 'Lizette Craig - 082-555-0456'),
( (SELECT UserId FROM [User] WHERE Email = 'jane.smith@race.com'), '083-555-0456', 'Fiso Smith - 083-555-0123');
GO

-- Seed Events
INSERT INTO [Event] (OrganiserId, Name, Description, [Date], [Location], RouteDetails, EventType, MaxParticipants) VALUES
( (SELECT OrganiserId FROM OrganiserProfile WHERE UserId = (SELECT UserId FROM [User] WHERE Email = 'thabo@runjoburg.com')),
  'Soweto Marathon', 'A challenging 42.2km race through the vibrant streets of Soweto.', '2026-09-15 06:00:00',
  'Soweto, Johannesburg', 'Starts at the Orlando Stadium, goes through Vilakazi Street and back.', 'Running', 25000),
( (SELECT OrganiserId FROM OrganiserProfile WHERE UserId = (SELECT UserId FROM [User] WHERE Email = 'sarah@capetowncycle.com')),
  'Cape Town Cycle Tour', 'The world''s largest timed cycle race around the Cape Peninsula.', '2026-09-22 07:00:00',
  'Cape Town', 'A 109km route from the city centre along the M3 to Chapman''s Peak and back.', 'Cycling', 35000),
( (SELECT OrganiserId FROM OrganiserProfile WHERE UserId = (SELECT UserId FROM [User] WHERE Email = 'thabo@runjoburg.com')),
  'Joburg 10km City Run', 'A fast, flat 10km run through the heart of Johannesburg.', '2026-10-01 08:00:00',
  'Sandton, Johannesburg', 'A 10km loop starting and ending at the Sandton Convention Centre.', 'Running', 10000);
GO

-- Seed Categories for each Event
-- For Soweto Marathon (EventId 1)
INSERT INTO Category (EventId, Name, Description, EntryFee) VALUES
(1, 'Full Marathon', '42.2km', 200.00),
(1, 'Half Marathon', '21.1km', 150.00),
(1, '10km Run', '10km', 80.00);

-- For Cape Town Cycle Tour (EventId 2)
INSERT INTO Category (EventId, Name, Description, EntryFee) VALUES
(2, 'Elite Cycle', 'For licensed cyclists', 350.00),
(2, 'Amateur Cycle', 'For leisure and club cyclists', 250.00);

-- For Joburg 10km City Run (EventId 3)
INSERT INTO Category (EventId, Name, Description, EntryFee) VALUES
(3, '10km Run', '10km', 90.00),
(3, '5km Fun Run', '5km', 50.00);
GO

-- Seed Weather Information
INSERT INTO WeatherInfo (EventId, ForecastDate, [Condition], TemperatureCelsius, WindSpeedKmph) VALUES
(1, '2026-09-15 06:00:00', 'Sunny', 18.5, 12.5),
(1, '2026-09-15 12:00:00', 'Partly Cloudy', 24.0, 15.0),
(2, '2026-09-22 07:00:00', 'Sunny', 16.0, 8.0),
(2, '2026-09-22 12:00:00', 'Sunny', 22.5, 14.0),
(3, '2026-10-01 08:00:00', 'Cloudy', 15.0, 10.0);
GO

-- Seed Enrolments
INSERT INTO Enrolment (ParticipantId, CategoryId, [Status], ResultTime, [Position]) VALUES
( (SELECT ParticipantId FROM ParticipantProfile WHERE UserId = (SELECT UserId FROM [User] WHERE Email = 'john.craig@fit.com')),
  (SELECT CategoryId FROM Category WHERE EventId = 1 AND Name = 'Half Marathon'),
  'Confirmed', '01:55:30', 120),
( (SELECT ParticipantId FROM ParticipantProfile WHERE UserId = (SELECT UserId FROM [User] WHERE Email = 'jane.smith@race.com')),
  (SELECT CategoryId FROM Category WHERE EventId = 2 AND Name = 'Amateur Cycle'),
  'Confirmed', '03:45:22', 300),
( (SELECT ParticipantId FROM ParticipantProfile WHERE UserId = (SELECT UserId FROM [User] WHERE Email = 'john.craig@fit.com')),
  (SELECT CategoryId FROM Category WHERE EventId = 3 AND Name = '10km Run'),
  'Pending', NULL, NULL);
GO

-- =============================================
-- Verification Queries
-- =============================================
-- SELECT * FROM [User];
-- SELECT * FROM OrganiserProfile;
-- SELECT * FROM ParticipantProfile;
-- SELECT * FROM [Event];
-- SELECT * FROM Category;
-- SELECT * FROM WeatherInfo;
-- SELECT * FROM Enrolment;
-- GO