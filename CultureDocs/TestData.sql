-- ============================================================
-- Скрипт создания БД CultureDocs с тестовыми данными
-- Выполнить один раз в SSMS
-- ============================================================

-- 1. Создаём БД (если ещё нет)
IF DB_ID('CultureDocs') IS NULL
    CREATE DATABASE CultureDocs;
GO

USE CultureDocs;
GO

-- 2. Удаляем старые таблицы (если есть)
IF OBJECT_ID('Files') IS NOT NULL DROP TABLE Files;
IF OBJECT_ID('Files') IS NOT NULL DROP TABLE Files;
IF OBJECT_ID('Versions') IS NOT NULL DROP TABLE Versions;
IF OBJECT_ID('Documents') IS NOT NULL DROP TABLE Documents;
IF OBJECT_ID('Users') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('Categories') IS NOT NULL DROP TABLE Categories;
IF OBJECT_ID('Events') IS NOT NULL DROP TABLE Events;
IF OBJECT_ID('Authors') IS NOT NULL DROP TABLE Authors;
IF OBJECT_ID('Roles') IS NOT NULL DROP TABLE Roles;
GO

-- 3. Создаём таблицы
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL
);

CREATE TABLE Authors (
    AuthorID INT IDENTITY(1,1) PRIMARY KEY,
    LastName NVARCHAR(50) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    MiddleName NVARCHAR(50) NULL,
    Position NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL
);

CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(200) NOT NULL,
    EventDate DATE NOT NULL,
    Place NVARCHAR(200) NULL,
    EventType NVARCHAR(50) NULL
);

CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL
);

CREATE TABLE Documents (
    DocumentID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(300) NOT NULL,
    DocType NVARCHAR(50) NOT NULL,
    Genre NVARCHAR(50) NULL,
    Description NVARCHAR(1000) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(30) NOT NULL,
    AuthorID INT NOT NULL,
    EventID INT NULL,
    CategoryID INT NULL,
    FOREIGN KEY (AuthorID) REFERENCES Authors(AuthorID),
    FOREIGN KEY (EventID) REFERENCES Events(EventID),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

CREATE TABLE Versions (
    VersionID INT IDENTITY(1,1) PRIMARY KEY,
    DocumentID INT NOT NULL,
    VersionNumber INT NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    AuthorID INT NOT NULL,
    Comment NVARCHAR(500) NULL,
    FilePath NVARCHAR(500) NULL,
    FOREIGN KEY (DocumentID) REFERENCES Documents(DocumentID),
    FOREIGN KEY (AuthorID) REFERENCES Authors(AuthorID)
);

CREATE TABLE Files (
    FileID INT IDENTITY(1,1) PRIMARY KEY,
    VersionID INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FileType NVARCHAR(50) NULL,
    FileSize INT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FOREIGN KEY (VersionID) REFERENCES Versions(VersionID)
);

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Login NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    RoleID INT NOT NULL,
    AuthorID INT NULL,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID),
    FOREIGN KEY (AuthorID) REFERENCES Authors(AuthorID)
);

-- Индексы
CREATE INDEX IX_Documents_Title ON Documents(Title);
CREATE INDEX IX_Documents_DocType ON Documents(DocType);
CREATE INDEX IX_Documents_AuthorID ON Documents(AuthorID);
CREATE INDEX IX_Documents_Status ON Documents(Status);
CREATE INDEX IX_Versions_DocumentID ON Versions(DocumentID);
GO

-- 4. Роли
INSERT INTO Roles (RoleName, Description) VALUES
('Администратор', 'Полный доступ ко всем функциям'),
('Методист', 'Создание, редактирование и удаление документов'),
('Режиссёр', 'Создание и редактирование документов, работа с версиями'),
('Гость', 'Только просмотр документов и отчётов');
GO

-- 5. Авторы (5 сотрудников)
INSERT INTO Authors (LastName, FirstName, MiddleName, Position, Phone, Email) VALUES
('Иванов', 'Иван', 'Иванович', 'Режиссёр', '8-900-111-11-11', 'ivanov@dk.ru'),
('Петров', 'Пётр', 'Петрович', 'Методист', '8-900-222-22-22', 'petrov@dk.ru'),
('Сидоров', 'Сергей', 'Сергеевич', 'Сценарист', '8-900-333-33-33', 'sidorov@dk.ru'),
('Кузнецов', 'Алексей', 'Николаевич', 'Художественный руководитель', '8-900-444-44-44', 'kuznetsov@dk.ru'),
('Смирнова', 'Анна', 'Викторовна', 'Методист', '8-900-555-55-55', 'smirnova@dk.ru');
GO

-- 6. Мероприятия (5 штук)
INSERT INTO Events (EventName, EventDate, Place, EventType) VALUES
('День Победы', '2026-05-09', 'Площадь Ленина', 'Праздник'),
('Новогодний концерт', '2026-12-31', 'ДК «Металлург»', 'Концерт'),
('8 Марта', '2026-03-08', 'ДК «Металлург»', 'Концерт'),
('День города', '2026-06-12', 'Городской парк', 'Гуляние'),
('Конкурс «Весна»', '2026-04-20', 'Актовый зал', 'Конкурс');
GO

-- 7. Категории (6 штук)
INSERT INTO Categories (CategoryName, Description) VALUES
('Сценарий', 'Сценарии мероприятий'),
('Методическая разработка', 'Методические материалы'),
('Положение', 'Положения о конкурсах'),
('План', 'Планы работы'),
('Отчёт', 'Отчёты о мероприятиях'),
('Репертуар', 'Репертуарные планы');
GO

-- 8. Документы (12 штук)
INSERT INTO Documents (Title, DocType, Genre, Description, Status, AuthorID, EventID, CategoryID) VALUES
('Сценарий праздника 9 мая', 'Сценарий', 'Театрализованное представление', 'Сценарий для площади', 'утверждён', 1, 1, 1),
('Сценарий Нового года', 'Сценарий', 'Концерт', 'Новогодняя программа', 'на согласовании', 1, 2, 1),
('Сценарий 8 марта', 'Сценарий', 'Концерт', 'Праздничный концерт', 'утверждён', 1, 3, 1),
('Методичка по работе с детьми', 'Методическая разработка', 'Методика', 'Рекомендации для руководителей', 'утверждён', 2, NULL, 2),
('Методичка по вокалу', 'Методическая разработка', 'Методика', 'Упражнения для вокалистов', 'черновик', 2, NULL, 2),
('Положение о конкурсе «Весна»', 'Положение', 'Конкурс', 'Условия проведения', 'утверждён', 3, 5, 3),
('Положение о фестивале', 'Положение', 'Фестиваль', 'Порядок участия', 'черновик', 3, NULL, 3),
('План работы на май', 'План', 'План', 'Календарный план', 'утверждён', 4, NULL, 4),
('План работы на июнь', 'План', 'План', 'Календарный план', 'черновик', 4, NULL, 4),
('Отчёт о Дне города', 'Отчёт', 'Отчёт', 'Итоги мероприятия', 'архив', 5, 4, 5),
('Отчёт о конкурсе', 'Отчёт', 'Отчёт', 'Итоги конкурса', 'утверждён', 5, 5, 5),
('Репертуарный план на год', 'Репертуар', 'План', 'Список номеров', 'черновик', 5, NULL, 6);
GO

-- 9. Версии документов (25 штук)
-- Документ 1 — 3 версии
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(1, 1, 1, 'Первая версия'),
(1, 2, 1, 'Добавлены номера'),
(1, 3, 4, 'Согласовано, финальная версия');

-- Документ 2 — 2 версии
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(2, 1, 1, 'Черновик'),
(2, 2, 1, 'Правки после обсуждения');

-- Документ 3 — 2 версии
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(3, 1, 1, 'Первая версия'),
(3, 2, 4, 'Утверждено');

-- Документ 4 — 2 версии
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(4, 1, 2, 'Первая версия'),
(4, 2, 2, 'Дополнено примерами');

-- Документ 5 — 1 версия
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(5, 1, 2, 'Черновик');

-- Документ 6 — 2 версии
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(6, 1, 3, 'Первая версия'),
(6, 2, 4, 'Утверждено');

-- Документ 7 — 1 версия
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(7, 1, 3, 'Черновик');

-- Документ 8 — 2 версии
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(8, 1, 4, 'Первая версия'),
(8, 2, 4, 'Утверждено');

-- Документ 9 — 1 версия
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(9, 1, 4, 'Черновик');

-- Документ 10 — 2 версии
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(10, 1, 5, 'Первая версия'),
(10, 2, 5, 'Архивировано');

-- Документ 11 — 2 версии
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(11, 1, 5, 'Первая версия'),
(11, 2, 4, 'Утверждено');

-- Документ 12 — 1 версия
INSERT INTO Versions (DocumentID, VersionNumber, AuthorID, Comment) VALUES
(12, 1, 5, 'Черновик');
GO

-- 10. Пользователи (4 учётные записи)
-- Пароли:
--   admin   → SHA-256: 8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
--   12345   → SHA-256: 5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5
--   guest   → SHA-256: 84983c60f7daadc1cb8698621f802c0d9f9a3c3c3f8e6c2e5b5c8a9a4c7b5d3e

INSERT INTO Users (Login, PasswordHash, RoleID, AuthorID) VALUES
('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 1, NULL),
('ivanov', '5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5', 2, 1),
('petrov', '5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5', 3, 2),
('guest',  '84983c60f7daadc1cb8698621f802c0d9f9a3c3c3f8e6c2e5b5c8a9a4c7b5d3e', 4, NULL);
GO

PRINT '========================================';
PRINT 'База данных CultureDocs создана!';
PRINT 'Пользователи для входа:';
PRINT '  admin  / admin   (Администратор)';
PRINT '  ivanov / 12345   (Методист)';
PRINT '  petrov / 12345   (Режиссёр)';
PRINT '  guest  / guest   (Гость)';
PRINT '========================================';
GO