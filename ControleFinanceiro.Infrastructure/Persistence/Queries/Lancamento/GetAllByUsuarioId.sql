SELECT * FROM Lancamentos 
WHERE UsuarioId = @UsuarioId 
AND Data BETWEEN @Start AND @End 
ORDER BY Data DESC
