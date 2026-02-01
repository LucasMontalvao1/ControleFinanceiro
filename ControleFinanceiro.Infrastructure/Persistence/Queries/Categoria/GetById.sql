SELECT * FROM Categorias 
WHERE Id = @Id AND (UsuarioId = @UsuarioId OR IsDefault = TRUE)
