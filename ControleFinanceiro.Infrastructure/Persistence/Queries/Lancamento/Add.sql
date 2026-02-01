INSERT INTO Lancamentos (Descricao, Valor, Data, Tipo, UsuarioId, CategoriaId, CreatedAt) 
VALUES (@Descricao, @Valor, @Data, @Tipo, @UsuarioId, @CategoriaId, @CreatedAt);
SELECT LAST_INSERT_ID();
