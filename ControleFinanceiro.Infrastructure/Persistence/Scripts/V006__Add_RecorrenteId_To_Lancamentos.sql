ALTER TABLE Lancamentos ADD COLUMN RecorrenteId INT NULL;
ALTER TABLE Lancamentos ADD CONSTRAINT FK_Lancamentos_Recorrentes FOREIGN KEY (RecorrenteId) REFERENCES Recorrentes(Id) ON DELETE SET NULL;
