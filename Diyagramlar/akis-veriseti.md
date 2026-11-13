# Veri seti yükleme akışı

Kaynak kod: `DatasetController.Upload`, `DatasetImportService.ImportAsync`, `CsvDatasetReader`

```mermaid
flowchart TD
    A(["Admin Veri Seti Yükle sayfasını açar"]) --> B{"Dosya seçildi mi?"}
    B -->|"hayır"| C["Uyarı: lütfen dosya seçin"]
    B -->|"evet"| D{"Uzantıya uygun okuyucu var mı? (.csv)"}
    D -->|"hayır"| E["Hata: desteklenmeyen dosya türü"]
    D -->|"evet"| F["Sıradaki satırı oku"]
    F --> G{"Satır doğrulaması geçti mi?"}
    G -->|"hayır"| H["Satırı numarasıyla reddedilenlere ekle"]
    G -->|"evet"| I{"Aynı öğrenci no profilde var mı?"}
    I -->|"evet"| J["Profili güncelle (StudentId'ye dokunma)"]
    I -->|"hayır"| K["Yeni profil ekle (StudentId boş)"]
    H --> L{"Başka satır var mı?"}
    J --> L
    K --> L
    L -->|"evet"| F
    L -->|"hayır"| M["SaveChanges, sonuç ekranı: eklenen, güncellenen, reddedilen satırlar"]
```

Satır doğrulaması sırasıyla şunları kontrol eder: 12 sütun, öğrenci no, ad soyad ve TC kimlik no boş mu, doğum tarihi `gg.aa.yyyy` mi, önceki staj günü sayı ve 0 veya büyük mü, sağlık yardımı 1-6 mı, dosyada tekrar eden öğrenci no, dosyada tekrar eden e-posta.
