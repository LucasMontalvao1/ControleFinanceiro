SELECT 
    c.Nome as Categoria,
    SUM(l.Valor) as Valor
FROM Lancamentos l
INNER JOIN Categorias c ON l.CategoriaId = c.Id
WHERE l.UsuarioId = @UsuarioId 
AND l.Tipo = 'Saida'
AND l.Data BETWEEN @Start AND @End
GROUP BY c.Nome
ORDER BY Valor DESC;
