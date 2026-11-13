# Staj günü hesabı akışı

> Durum: Staj günü ekranı şu an yalnızca seçimi `InternshipDaySelections` tablosuna kaydeder (`WeekDays`, `RangeStart`, `RangeEnd`). Aşağıdaki hesap ve karşılaştırma (`CalculatedDays`) final aşamasında yazılacak kural için tasarlanmış akıştır.

```mermaid
flowchart TD
    A(["Başla"]) --> B["Seçilen haftalık günleri ve tarih aralığını al"]
    B --> C["Sayaç = 0, gün = aralığın ilk günü"]
    C --> D{"gün aralığın sonunu geçti mi?"}
    D -->|"evet"| H["Hesaplanan gün sayısı = sayaç"]
    D -->|"hayır"| E{"gün, seçilen haftalık günlerden biri mi?"}
    E -->|"hayır"| G["Bir sonraki güne geç"]
    E -->|"evet"| F{"gün resmi tatil mi?"}
    F -->|"evet"| G
    F -->|"hayır"| F2["Sayaç artır"]
    F2 --> G
    G --> D
    H --> I{"Hesaplanan sayı belgedeki staj gün sayısına eşit mi?"}
    I -->|"evet"| J["Sonuç: geçti"]
    I -->|"hayır"| K["Sonuç: uyuşmazlık, raporda göster"]
    J --> L(["Bitiş"])
    K --> L
```
