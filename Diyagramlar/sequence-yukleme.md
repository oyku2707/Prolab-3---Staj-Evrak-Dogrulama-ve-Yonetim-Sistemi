# Sıralama diyagramı, UML

'''mermaid
sequenceDiagram
    actor Ogrenci
    participant Ctrl as DocumentsController
    participant DS as DocumentService
    participant Ext as IDocumentFieldExtractor
    participant VS as ValidationService
    participant Rule as IValidationRule
    participant DB as AppDbContext

    Ogrenci->>Ctrl: Ek-16 dosyasını yükler
    Ctrl->>DS: Belgeyi kaydet
    DS->>DB: Document ekle, durum Yuklendi
    DS->>Ext: Extract(filePath)
    Ext-->>DS: Ek16Data
    DS->>VS: Doğrula(ValidationContext)
    loop her kural için
        VS->>Rule: Validate(context)
        Rule-->>VS: ValidationResult
    end
    VS->>DB: ValidationResults kaydet
    VS-->>DS: Sonuç listesi
    DS->>DB: Durumu güncelle
    DS-->>Ctrl: Rapor
    Ctrl-->>Ogrenci: Rapor ekranı
    '''