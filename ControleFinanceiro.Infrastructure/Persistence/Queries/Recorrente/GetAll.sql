SELECT r.*, c.Nome as CategoriaNome 
FROM Recorrentes r
JOIN Categorias c ON r.CategoriaId = c.Id
WHERE r.UsuarioId = @UsuarioId
ORDER BY r.DiaVencimento;
