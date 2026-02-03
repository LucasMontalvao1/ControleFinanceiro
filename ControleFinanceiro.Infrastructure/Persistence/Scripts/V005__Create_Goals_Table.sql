CREATE TABLE IF NOT EXISTS Metas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UsuarioId INT NOT NULL,
    CategoriaId INT NOT NULL,
    ValorLimite DECIMAL(18, 2) NOT NULL,
    Mes INT NOT NULL,
    Ano INT NOT NULL,
    UNIQUE KEY (UsuarioId, CategoriaId, Mes, Ano),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
);
