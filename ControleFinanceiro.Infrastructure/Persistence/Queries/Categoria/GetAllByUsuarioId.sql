SELECT * FROM Categorias 
WHERE UsuarioId = @UsuarioId OR IsDefault = TRUE 
ORDER BY Nome
