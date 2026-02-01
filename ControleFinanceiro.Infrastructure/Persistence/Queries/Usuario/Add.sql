INSERT INTO Usuarios (Nome, Username, Email, PasswordHash, CreatedAt) 
VALUES (@Nome, @Username, @Email, @PasswordHash, @CreatedAt);
SELECT LAST_INSERT_ID();
