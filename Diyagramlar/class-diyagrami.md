# Class diyagramı: alan modeli ve doğrulama

Servis ve controller sınıfları için `class-diyagrami-servisler.md` dosyasına bakın.

```mermaid
classDiagram
    class User {
        <<abstract>>
        +int Id
        +string UserName
        +string Email
        -string PasswordHash
        -int FailedLoginCount
        -bool IsLocked
        +RoleType Role
        +SetPasswordHash(hash)
        +RegisterFailedLogin()
        +ResetFailedLogins()
        +Unlock()
    }
    class Student {
        +StudentProfile Profile
        +RoleType Role
    }
    class Admin {
        +RoleType Role
    }
    User <|-- Student
    User <|-- Admin

    class StudentProfile {
        +int Id
        +int? StudentId
        +string FullName
        +string TcNo
        +string StudentNo
        +Nationality Nationality
        +DateTime BirthDate
        +string Address
        +string Department
        +string SubmissionYear
        +int PreviousInternshipDays
        +string Email
        +string Phone
        +int HealthAidStatus
    }
    class Document {
        +int Id
        +int StudentId
        +string FilePath
        +string FileType
        +DateTime UploadedAt
        +DocumentStatus Status
        +Ek16Data ExtractedData
        +Ek16Data EnteredData
        +List~ValidationResultRecord~ Results
    }
    class Ek16Data {
        +string StudentFullName
        +string TcNo
        +Nationality Nationality
        +DateTime BirthDate
        +string StudentNo
        +string AcademicYear
        +string CompanyName
        +int EngineerCount
        +DateTime StartDate
        +DateTime EndDate
        +int InternshipDays
        +bool EmployerHasSignature
        +bool StudentApprovalFilled
    }
    class ValidationResultRecord {
        +int Id
        +int DocumentId
        +string RuleName
        +string FieldName
        +bool IsValid
        +Severity Severity
        +string Message
        +string Suggestion
    }
    class InternshipDaySelection {
        +int Id
        +int DocumentId
        +string WeekDays
        +DateTime RangeStart
        +DateTime RangeEnd
        +int CalculatedDays
    }
    class LoginAttempt {
        +int Id
        +string UserNameOrEmail
        +DateTime AttemptedAt
        +bool Success
        +string IpAddress
    }
    class Holiday {
        +int Id
        +DateTime Date
        +string Name
    }
    Student "1" --> "0..1" StudentProfile : Profile
    Student "1" --> "*" Document : sahibi
    Document "1" *-- "0..1" Ek16Data : ExtractedData
    Document "1" *-- "0..1" Ek16Data : EnteredData
    Document "1" --> "*" ValidationResultRecord : Results
    InternshipDaySelection ..> Document : DocumentId

    class DocumentStatus {
        <<enumeration>>
        Yuklendi
        Inceleniyor
        HataliEksik
        Onaylandi
        Tamamlandi
    }
    class RoleType {
        <<enumeration>>
        Student
        Admin
    }
    class Nationality {
        <<enumeration>>
        TC
        Yabanci
    }
    class Severity {
        <<enumeration>>
        Error
        Warning
    }
    Document --> DocumentStatus
    User --> RoleType
    StudentProfile --> Nationality
    Ek16Data --> Nationality
    ValidationResultRecord --> Severity

    class IValidationRule {
        <<interface>>
        +string Name
        +Validate(ValidationContext) IEnumerable~ValidationResult~
    }
    class ValidationRuleBase {
        <<abstract>>
        +string Name
        #Pass(field) ValidationResult
        #Fail(field, message) ValidationResult
    }
    class TcKimlikRule
    class PhoneRule
    class EmailRule
    class EngineerCountRule
    class FormVsDocumentRule
    class ProfileMatchRule
    IValidationRule <|.. ValidationRuleBase
    ValidationRuleBase <|-- TcKimlikRule
    ValidationRuleBase <|-- PhoneRule
    ValidationRuleBase <|-- EmailRule
    ValidationRuleBase <|-- EngineerCountRule
    ValidationRuleBase <|-- FormVsDocumentRule
    ValidationRuleBase <|-- ProfileMatchRule

    class ValidationContext {
        +Ek16Data Data
        +Ek16Data Entered
        +StudentProfile Profile
        +List~DayOfWeek~ SelectedWeekDays
        +DateTime SubmissionDate
    }
    class ValidationResult {
        +string RuleName
        +string FieldName
        +bool IsValid
        +Severity Severity
        +string Message
        +string Suggestion
    }
    IValidationRule ..> ValidationContext : kullanır
    IValidationRule ..> ValidationResult : üretir
    ValidationContext --> Ek16Data
    ValidationContext --> StudentProfile

    class IValidationService {
        <<interface>>
        +ValidateAsync(documentId) Task~DocumentStatus~
    }
    class ValidationService {
        +RunRules(context) List~ValidationResult~
        +ValidateAsync(documentId) Task~DocumentStatus~
    }
    class FakeValidationService
    IValidationService <|.. ValidationService
    IValidationService <|.. FakeValidationService
    ValidationService o-- IValidationRule : tüm kurallar
    ValidationService o-- IDocumentFieldExtractor : uygun okuyucu

    class IDocumentFieldExtractor {
        <<interface>>
        +Supports(fileExtension) bool
        +Extract(filePath) Ek16Data
    }
    class DocxFieldExtractor
    class DocFieldExtractor
    class PdfFieldExtractor
    class JpegFieldExtractor
    IDocumentFieldExtractor <|.. DocxFieldExtractor
    IDocumentFieldExtractor <|.. DocFieldExtractor
    IDocumentFieldExtractor <|.. PdfFieldExtractor
    IDocumentFieldExtractor <|.. JpegFieldExtractor
    DocFieldExtractor --> DocxFieldExtractor : .docx'e çevirip okur
    IDocumentFieldExtractor ..> Ek16Data : üretir
```

`Ek16Data` toplam 35 alan içerir (öğrenci, staj yapılan yer, işveren, onay bölümleri). Diyagramda temsili alanlar gösterilmiştir, tamamı `Domain/Entities/Ek16Data.cs` dosyasındadır. `DocFieldExtractor`, `.doc` dosyasını LibreOffice ile geçici bir `.docx`'e çevirip `DocxFieldExtractor`'a devreder. `JpegFieldExtractor` şimdilik iskelettir: OCR yazılmadı, boş `Ek16Data` döndürür. `FakeValidationService` geliştirme için yazılmış geçici sınıftır, `Program.cs` içinde kayıtlı değildir.