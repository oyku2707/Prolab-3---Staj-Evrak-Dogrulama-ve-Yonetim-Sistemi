# Giriş akışı

'''mermaid
flowchart TD
    A(["Başla"]) --> B["Kullanıcı adı veya e-posta ve şifre girilir"]
    B --> C{"Kullanıcı var mı?"}
    C -->|"hayır"| D["Başarısız denemeyi LoginAttempts'e yaz"]
    D --> E["Hata mesajı göster"]
    C -->|"evet"| F{"Hesap kilitli mi?"}
    F -->|"evet"| G["Hesap kilitli mesajı göster"]
    F -->|"hayır"| H{"Şifre hash'i eşleşiyor mu?"}
    H -->|"hayır"| I["Başarısız deneme sayacını artır ve logla"]
    I --> J{"Sayaç 5'e ulaştı mı?"}
    J -->|"evet"| K["Hesabı kilitle"]
    J -->|"hayır"| E
    K --> G
    H -->|"evet"| L["Sayacı sıfırla ve başarılı girişi logla"]
    L --> M["Rolü session veya JWT içine yaz"]
    M --> N{"Rol nedir?"}
    N -->|"ROLE_STUDENT"| O["Öğrenci ana sayfası"]
    N -->|"ROLE_ADMIN"| P["Admin paneli"]
    '''