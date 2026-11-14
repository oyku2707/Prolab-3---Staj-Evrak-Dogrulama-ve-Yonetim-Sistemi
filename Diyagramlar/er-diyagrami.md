# Veri Tabanı (ER) Diyagramı

Tablolar `AppDbContext` ve migration dosyalarından çıkarılmıştır. `Student` ve `Admin` aynı `Users` tablosunda tutulur (TPH, `Discriminator` sütunu). `Ek16Data` ayrı tablo değildir: `Documents` tablosuna `ExtractedData_` ve `EnteredData_` önekli sütunlar olarak gömülüdür.

```mermaid
erDiagram
    Users |o--o| StudentProfiles : "StudentId (bos olabilir)"
    Users ||--o{ Documents : "StudentId"
    Documents ||--o{ ValidationResults : "DocumentId"
    Documents ||--o{ InternshipDaySelections : "DocumentId (FK kisiti yok)"

    Users {
        int Id PK
        string UserName
        string Email
        string PasswordHash
        int FailedLoginCount
        bool IsLocked
        string Discriminator "Student veya Admin"
    }
    StudentProfiles {
        int Id PK
        int StudentId FK "null olabilir"
        string FullName
        string TcNo
        string StudentNo
        string Nationality
        date BirthDate
        string Address
        string Department
        string SubmissionYear
        int PreviousInternshipDays
        string Email
        string Phone
        int HealthAidStatus
    }
    Documents {
        int Id PK
        int StudentId FK
        string FilePath
        string FileType
        datetime UploadedAt
        string Status
        string ExtractedData_Ek16_alanlari "belgeden okunan, 35 sutun"
        string EnteredData_Ek16_alanlari "formdan girilen, 35 sutun"
    }
    ValidationResults {
        int Id PK
        int DocumentId FK
        string RuleName
        string FieldName
        bool IsValid
        string Severity
        string Message
        string Suggestion
    }
    InternshipDaySelections {
        int Id PK
        int DocumentId
        string WeekDays
        date RangeStart
        date RangeEnd
        int CalculatedDays
    }
    LoginAttempts {
        int Id PK
        string UserNameOrEmail
        datetime AttemptedAt
        bool Success
        string IpAddress
    }
    Holidays {
        int Id PK
        date Date
        string Name
    }
```

`LoginAttempts` ve `Holidays` diğer tablolara bağlı değildir. `Holidays` tablosunu staj günü kuralı (final) dolduracaktır. EF Core'un kendi tablosu `__EFMigrationsHistory` diyagrama alınmamıştır.
