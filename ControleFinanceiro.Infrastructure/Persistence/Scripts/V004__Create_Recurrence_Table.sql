CREATE TABLE IF NOT EXISTS Recorrentes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UsuarioId INT NOT NULL,
    CategoriaId INT NOT NULL,
    Descricao VARCHAR(255) NOT NULL,
    Valor DECIMAL(18, 2) NOT NULL,
    DiaVencimento INT NOT NULL,
    Tipo VARCHAR(10) NOT NULL, -- 'Entrada' ou 'Saida'
    Ativo BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
);
