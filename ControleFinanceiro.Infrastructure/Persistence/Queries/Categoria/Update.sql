UPDATE Categorias 
SET Nome = @Nome, Tipo = @Tipo 
WHERE Id = @Id AND UsuarioId = @UsuarioId AND IsDefault = FALSE
