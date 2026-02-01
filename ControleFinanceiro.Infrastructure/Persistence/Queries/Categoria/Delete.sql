DELETE FROM Categorias 
WHERE Id = @Id AND UsuarioId = @UsuarioId AND IsDefault = FALSE
