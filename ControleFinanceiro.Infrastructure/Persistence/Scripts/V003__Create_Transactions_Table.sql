CREATE TABLE IF NOT EXISTS Lancamentos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Descricao VARCHAR(200) NOT NULL,
    Valor DECIMAL(18,2) NOT NULL,
    Data DATETIME NOT NULL,
    Tipo VARCHAR(10) NOT NULL, -- 'Entrada' ou 'Saida'
    UsuarioId INT NOT NULL,
    CategoriaId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Lancamentos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Lancamentos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
);
