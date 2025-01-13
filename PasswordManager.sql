-- Create database
CREATE DATABASE PasswordManager;
GO

USE PasswordManager;
GO

-- Create Roles table
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

-- Create Users table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    RoleId INT NOT NULL,
    TwoFactorEnabled BIT DEFAULT 0,
    TwoFactorSecret NVARCHAR(MAX) NULL,
    LastLoginDate DATETIME NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- Create StoredPasswords table
CREATE TABLE StoredPasswords (
    PasswordId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    SiteName NVARCHAR(100) NOT NULL,
    SiteUrl NVARCHAR(255) NULL,
    Username NVARCHAR(100) NOT NULL,
    EncryptedPassword NVARCHAR(MAX) NOT NULL,
    Notes NVARCHAR(MAX) NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    ExpirationDate DATETIME NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Create LoginAttempts table
CREATE TABLE LoginAttempts (
    AttemptId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    AttemptDate DATETIME DEFAULT GETDATE(),
    IsSuccessful BIT NOT NULL,
    IPAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(255) NULL
);

-- Create AuditLog table
CREATE TABLE AuditLog (
    LogId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NULL,
    ActionDate DATETIME DEFAULT GETDATE(),
    Action NVARCHAR(50) NOT NULL,
    Details NVARCHAR(MAX) NULL,
    IPAddress NVARCHAR(45) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Insert default roles
INSERT INTO Roles (RoleName) VALUES 
('User'),
('Administrator'),
('IT_Specialist');

-- Create indexes for better performance
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_StoredPasswords_UserId ON StoredPasswords(UserId);
CREATE INDEX IX_LoginAttempts_Username ON LoginAttempts(Username);
CREATE INDEX IX_LoginAttempts_AttemptDate ON LoginAttempts(AttemptDate);
CREATE INDEX IX_AuditLog_UserId ON AuditLog(UserId);
CREATE INDEX IX_AuditLog_ActionDate ON AuditLog(ActionDate);