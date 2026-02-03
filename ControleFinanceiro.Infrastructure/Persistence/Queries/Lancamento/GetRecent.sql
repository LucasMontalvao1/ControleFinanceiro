SELECT l.*, c.Nome as CategoriaNome 
FROM Lancamentos l
LEFT JOIN Categorias c ON l.CategoriaId = c.Id
WHERE l.UsuarioId = @UsuarioId
AND l.Data BETWEEN @Start AND @End
ORDER BY l.Data DESC, l.Id DESC
LIMIT @Take
