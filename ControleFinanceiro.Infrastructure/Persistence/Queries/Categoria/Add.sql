INSERT INTO Categorias (Nome, Tipo, UsuarioId, IsDefault, CreatedAt) 
VALUES (@Nome, @Tipo, @UsuarioId, @IsDefault, @CreatedAt);
SELECT LAST_INSERT_ID();
