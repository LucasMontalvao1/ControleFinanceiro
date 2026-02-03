INSERT INTO Recorrentes (UsuarioId, CategoriaId, Descricao, Valor, DiaVencimento, Tipo, Ativo, CreatedAt)
VALUES (@UsuarioId, @CategoriaId, @Descricao, @Valor, @DiaVencimento, @Tipo, @Ativo, @CreatedAt);
SELECT LAST_INSERT_ID();
