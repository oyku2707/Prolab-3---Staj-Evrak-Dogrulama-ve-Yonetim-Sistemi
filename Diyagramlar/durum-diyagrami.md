# Durum diyagramı

```mermaid
stateDiagram-v2
    [*] --> Yuklendi
    Yuklendi --> Inceleniyor : doğrulama başlar
    Inceleniyor --> HataliEksik : hata bulundu
    Inceleniyor --> Onaylandi : hata yok
    HataliEksik --> Yuklendi : düzeltilip yeniden yüklenir
    Onaylandi --> Tamamlandi : onaylanan belge tamamlanır
    Tamamlandi --> [*]
    note right of Tamamlandi
        Öğrenci değiştiremez ve silemez.
        Güncelleme ve silme sadece admin sayfasından.
    end note
    ```