using StajSistemi.Domain.Entities;

namespace StajSistemi.Domain.Interfaces;

public interface IDocumentFieldExtractor
{
    bool Supports(string fileExtension);
    Ek16Data Extract(string filePath);
}