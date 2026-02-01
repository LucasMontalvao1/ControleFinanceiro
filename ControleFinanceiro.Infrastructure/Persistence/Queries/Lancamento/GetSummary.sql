SELECT 
    SUM(CASE WHEN Tipo = 'Entrada' THEN Valor ELSE 0 END) as TotalEntradas,
    SUM(CASE WHEN Tipo = 'Saida' THEN Valor ELSE 0 END) as TotalSaidas
FROM Lancamentos 
WHERE UsuarioId = @UsuarioId 
AND Data BETWEEN @Start AND @End
