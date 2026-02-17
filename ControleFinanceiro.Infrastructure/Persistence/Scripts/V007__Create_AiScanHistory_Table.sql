CREATE TABLE AiScanHistory (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CorrelationId VARCHAR(50) NOT NULL,
    UsuarioId INT NOT NULL,
    Status VARCHAR(20) NOT NULL,
    LatencyMs BIGINT NOT NULL,
    RawJson LONGTEXT,
    ParseError TEXT,
    ProcessadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_AiScanHistory_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
);

CREATE INDEX IX_AiScanHistory_CorrelationId ON AiScanHistory(CorrelationId);
CREATE INDEX IX_AiScanHistory_UsuarioId ON AiScanHistory(UsuarioId);
