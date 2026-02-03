UPDATE Recorrentes 
SET CategoriaId = @CategoriaId, 
    Descricao = @Descricao, 
    Valor = @Valor, 
    DiaVencimento = @DiaVencimento, 
    Tipo = @Tipo, 
    Ativo = @Ativo
WHERE Id = @Id AND UsuarioId = @UsuarioId;
