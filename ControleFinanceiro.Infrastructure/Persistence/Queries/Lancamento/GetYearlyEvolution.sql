WITH RECURSIVE Meses AS (
    -- Start 5 months ago to get a total of 6 months (5 previous + current)
    SELECT DATE_FORMAT(DATE_SUB(LAST_DAY(@End), INTERVAL 5 MONTH), '%Y-%m-01') as DataMes
    UNION ALL
    SELECT DATE_ADD(DataMes, INTERVAL 1 MONTH)
    FROM Meses
    WHERE DataMes < DATE_FORMAT(@End, '%Y-%m-01')
)
SELECT 
    DATE_FORMAT(m.DataMes, '%Y-%m') as Mes,
    COALESCE(SUM(CASE WHEN l.Tipo = 'Entrada' THEN l.Valor ELSE 0 END), 0) as Entradas,
    COALESCE(SUM(CASE WHEN l.Tipo = 'Saida' THEN l.Valor ELSE 0 END), 0) as Saidas,
    COALESCE(SUM(CASE WHEN l.Tipo = 'Entrada' THEN l.Valor ELSE -l.Valor END), 0) as Saldo
FROM Meses m
LEFT JOIN Lancamentos l ON DATE_FORMAT(l.Data, '%Y-%m') = DATE_FORMAT(m.DataMes, '%Y-%m') AND l.UsuarioId = @UsuarioId
GROUP BY m.DataMes
ORDER BY m.DataMes ASC;
