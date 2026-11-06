# Sıralama diyagramı, UML

```mermaid
  sequenceDiagram
    actor Ogrenci
    participant Ctrl as DocumentsController
    participant DS as DocumentService
    participant FS as FileStorage
    participant ST as DocumentStatusService
    participant VS as ValidationService
    participant Ext as DocxFieldExtractor
    participant Rule as IValidationRule
    participant DB as AppDbContext

    Ogrenci->>Ctrl: Formu doldurup dosyayı yükler
    Ctrl->>DS: UploadAsync(studentId, file, entered)
    DS->>DS: Dosya kontrolü (uzantı, boş, 10 MB)
    DS->>FS: SaveAsync(file)
    FS-->>DS: Dosya yolu
    DS->>DB: Document ekle (Yuklendi, EnteredData)
    DS->>ST: Transition(Inceleniyor)
    DS->>VS: ValidateAsync(documentId)
    VS->>DB: Belgeyi oku
    VS->>Ext: Extract(filePath)
    Ext-->>VS: Ek16Data
    loop her kural için
        VS->>Rule: Validate(context)
        Rule-->>VS: ValidationResult listesi
    end
    VS->>DB: ValidationResults kaydet
    VS-->>DS: Önerilen statü
    DS->>ST: Transition(önerilen statü)
    DS-->>Ctrl: UploadResult
    Ctrl-->>Ogrenci: Rapor ekranı
   ```