using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;

namespace StajSistemi.Services.Documents;

public interface IDocumentStatusService
{
    bool CanTransition(DocumentStatus from, DocumentStatus to);
    void Transition(Document document, DocumentStatus to);
    bool CanStudentModify(Document document);
}