SELECT 
    DATE(Data) as Data,
    SUM(CASE WHEN Tipo = 'Entrada' THEN Valor ELSE 0 END) as Entradas,
    SUM(CASE WHEN Tipo = 'Saida' THEN Valor ELSE 0 END) as Saidas
FROM Lancamentos
WHERE UsuarioId = @UsuarioId 
AND Data BETWEEN @Start AND @End
GROUP BY DATE(Data)
ORDER BY Data ASC;
