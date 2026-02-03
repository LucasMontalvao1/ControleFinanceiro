INSERT INTO Lancamentos (Descricao, Valor, Data, Tipo, UsuarioId, CategoriaId, RecorrenteId, CreatedAt) 
VALUES (@Descricao, @Valor, @Data, @Tipo, @UsuarioId, @CategoriaId, @RecorrenteId, @CreatedAt);
SELECT LAST_INSERT_ID();
