# Use Case Diyagramı

'''mermaid
flowchart LR
    S([Öğrenci])
    A([Admin])
    subgraph Sistem
        U1("Giriş yap / çıkış yap")
        U2("Ek-16 yükle")
        U3("Raporu görüntüle")
        U4("Staj günlerini seç")
        U5("Google Form URL gir")
        U6("Veri setini yükle")
        U7("Başvuruları listele ve filtrele")
        U8("Tamamlanan belgeleri görüntüle")
        U9("Ek-17 oluştur")
        U10("Belgeyi doğrula")
        U11("Evrak ve sonuçları incele")
    end
    S --> U1
    S --> U2
    S --> U3
    S --> U4
    S --> U5
    A --> U1
    A --> U6
    A --> U7
    A --> U8
    A --> U9
    A --> U11
    U2 -.->|include| U10
    '''