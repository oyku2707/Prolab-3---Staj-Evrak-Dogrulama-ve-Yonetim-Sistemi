using StajSistemi.Domain.Enums;

namespace StajSistemi.Domain.Interfaces;

public interface IValidationService
{
    Task<DocumentStatus> ValidateAsync(int documentId);
}