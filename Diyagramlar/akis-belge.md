# Belge yükleme ve doğrulama akışı

```mermaid
flowchart TD
    A(["Öğrenci Evrak Yükle'ye basar"]) --> B{"Uzantı .doc, .docx, PDF, JPEG veya PNG mi?"}
    B -->|"hayır"| C["Uyarı göster, dosyayı reddet"]
    B -->|"evet"| D["Dosyayı kaydet, Document oluştur, durum Yüklendi"]
    D --> E["Durum İnceleniyor"]
    E --> F["IDocumentFieldExtractor ile alanları oku, Ek16Data doldur"]
    F --> G["ValidationContext hazırla: Ek16Data, StudentProfile, seçilen günler, teslim tarihi"]
    G --> H["Her IValidationRule için Validate çağır"]
    H --> I["Sonuçları ValidationResults tablosuna kaydet"]
    I --> J{"Error seviyesinde hata var mı?"}
    J -->|"evet"| K["Durum Hatalı veya Eksik"]
    K --> L["Öğrenci raporu görür, düzeltip yeniden yükler"]
    L --> A
    J -->|"hayır"| M["Durum Onaylandı"]
    M --> N["Durum Tamamlandı"]
    N --> O["Admin Tamamlandı sekmesinde görür"]
    O --> P(["Bitiş"])
    ```