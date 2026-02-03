UPDATE Lancamentos 
SET Descricao = @Descricao, 
    Valor = @Valor, 
    Data = @Data, 
    Tipo = @Tipo, 
    CategoriaId = @CategoriaId,
    RecorrenteId = @RecorrenteId
WHERE Id = @Id AND UsuarioId = @UsuarioId;
