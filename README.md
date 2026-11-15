# Staj Evrak Doğrulama ve Yönetim Sistemi

Programlama Laboratuvarı III, 2026-2027 Güz, Yazılım Mühendisliği.

Öğrenci, staj başvuru belgesi olan **Ek-16**'yı siteye yükler ve aynı belgenin bilgilerini bir form üzerinden de girer. Sistem belgeyi okur, belgeden okunan bilgi ile formdaki bilgiyi ve admin'in yüklediği veri setindeki resmi öğrenci kaydını karşılaştırır, kuralları çalıştırır ve sonucu rapor olarak gösterir. Hatasız belge **Tamamlandı** statüsüne geçer. Admin tüm başvuruları listeler, filtreler, inceler, düzenler ve siler.

Sistem yalnızca Ek-16'yı işler. Google Form'a otomatik kayıt, Ek-17 üretimi ve OCR final aşamasına bırakılmıştır.

## Rapor

Vize raporu (IEEE biçiminde): [Vize_Raporu.pdf](Rapor/Vize_Raporu.pdf)

## Teknolojiler

| Teknoloji | Kullanım |
|---|---|
| C#, .NET 10, ASP.NET Core MVC | Uygulama ve arayüz (Razor) |
| Entity Framework Core 9.0.0 + Pomelo MySQL 9.0.0 | Veri tabanı erişimi ve migration |
| MySQL 8 | Veri tabanı |
| BCrypt.Net-Next | Şifre hash'leme |
| JWT + cookie kimlik doğrulama | Oturum ve rol taşıma |
| DocumentFormat.OpenXml | .docx okuma |
| UglyToad.PdfPig | Metin tabanlı (taranmamış) .pdf okuma |
| LibreOffice (isteğe bağlı) | .doc dosyasını .docx'e çevirmek için |
| Bootstrap | Arayüz |

EF Core ve Pomelo paketleri `9.0.0` sürümüne sabittir, yükseltmeyin (paket sürüm çakışması çıkar). Proje yine .NET 10 üzerinde çalışır.

## Kurulum

### 1. Gerekenler

.NET 10 SDK, MySQL Community Server, Git. Migration komutları için bir kerelik:

```
dotnet tool install --global dotnet-ef --version 9.0.0
```

### 2. Depoyu indirin

```
git clone https://github.com/oyku2707/Prolab-3---Staj-Evrak-Do-rulama-ve-Y-netim-Sistemi.git
cd Prolab-3---Staj-Evrak-Do-rulama-ve-Y-netim-Sistemi/StajSistemi
```

### 3. Veri tabanını açın

MySQL Workbench ya da konsolda:

```sql
CREATE DATABASE staj_sistemi CHARACTER SET utf8mb4;
```

### 4. Gizli ayarlar (user-secrets)

Şifre ve anahtarlar repoya girmez, her bilgisayarda ayrı tanımlanır. Komutları `StajSistemi.csproj` dosyasının bulunduğu klasörde çalıştırın. Değerleri kendiniz belirleyin, `SIFRENIZ` gibi yer tutucuları değiştirin.

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=staj_sistemi;User=root;Password=SIFRENIZ;"
dotnet user-secrets set "Jwt:Key" "EN-AZ-32-KARAKTERLIK-RASTGELE-BIR-METIN"
dotnet user-secrets set "Seed:AdminPassword" "ADMIN-SIFRENIZ"
```

| Ayar | Ne için |
|---|---|
| `ConnectionStrings:DefaultConnection` | MySQL bağlantısı (zorunlu) |
| `Jwt:Key` | JWT imza anahtarı, en az 32 karakter (zorunlu, yoksa giriş hata verir) |
| `Seed:AdminPassword` | İlk admin hesabının şifresi (uygulama ilk açılışta `admin` kullanıcısını bu şifreyle oluşturur) |

Şifrede `;` ya da `"` kullanmayın, komut satırında sorun çıkarır.

### 5. Tabloları kurun ve çalıştırın

```
dotnet ef database update
dotnet build
dotnet run
```

Terminaldeki `Now listening on` adresini açın (varsayılan `http://localhost:5049`). Kod değiştirdikten sonra uygulamayı durdurup (`Ctrl + C`) yeniden çalıştırın. Tabloları her zaman migration ile kurun, elle kurulmuş tablolar migration ile çakışır.

## İlk kullanım

1. `admin` kullanıcı adı ve `Seed:AdminPassword` değeri ile giriş yapın.
2. **Veri Seti Yükle** sayfasından `StajSistemi/Data/students.csv` dosyasını yükleyin. Dosya bilerek hatalı satırlar içerir, reddedilen satırlar numarasıyla listelenir.
3. Çıkış yapıp **Kayıt Ol**'dan öğrenci hesabı açın. Öğrenci numarası olarak veri setindeki bir numarayı (örneğin `220229001`) girerseniz profil hesabınıza bağlanır.
4. Öğrenci olarak **Evrak Yükle**'den `StajSistemi/TestBelgeleri` klasöründeki örnek belgelerden birini yükleyin.

Test belgelerinde dikkat: sistem formdaki bilgiyi belgeyle karşılaştırır. Formu belgenin içindeki değerlerle aynen doldurmazsanız belge **Hatalı / Eksik** olur, raporda hangi alanın uyuşmadığı yazar. Telefonlar belgede `+90 xxx xxx xx xx` biçiminde olmalıdır.

| Dosya | Beklenen sonuç |
|---|---|
| `ornek1.docx` (form belgeyle aynı doldurulursa) | Tamamlandı |
| `ornek2_hatali.docx` | Hatalı / Eksik |
| `ornek3_ahmet.docx` ve `ornek3_ahmet.pdf` (öğrenci numarası `220229001` ile kayıtlı hesap, form belgeyle aynı doldurulursa) | Tamamlandı |
| `.txt`, `.exe` gibi uzantılar | Yükleme reddedilir, uyarı gösterilir |
| `.jpg`, `.jpeg` | Yüklenir. JPEG okuyucusu şimdilik iskelettir, belgeden alan okumaz. Alanlar boş sayıldığı için belge Hatalı / Eksik olur (OCR final aşamasında) |
| `.png` | Yüklenir ama otomatik okunamaz, Hatalı / Eksik olur (OCR final aşamasında) |

Desteklenen biçimler: `.docx`, `.doc` (LibreOffice gerekir) ve metin tabanlı `.pdf`. PDF okuyucu yazıların sayfadaki konumuna göre çalıştığı için Ek-16 şablonunun düzenini bekler. Fotoğraftan veya taramadan üretilmiş PDF'ler okunamaz.

## Roller ve sayfalar

| Rol | Yapabilecekleri |
|---|---|
| Öğrenci (`ROLE_STUDENT`) | Kayıt, giriş, Ek-16 yükleme, rapor görüntüleme, hatalı belgeyi yeniden yükleme, staj günü seçme, Google Form URL kontrolü |
| Admin (`ROLE_ADMIN`) | Veri seti yükleme, tüm başvuruları listeleme ve filtreleme, Tamamlananlar sekmesi, belge detayı (formda girilen, belgeden okunan, veri setindeki bilgi yan yana), belge düzenleme ve silme |

| Sayfa | Adres |
|---|---|
| Giriş / Kayıt / Çıkış | `/Auth/Login`, `/Auth/Register`, `/Auth/Logout` (POST) |
| Yetkisiz erişim | `/Auth/AccessDenied` (403, 3 saniye sonra girişe yönlendirir) |
| Öğrenci belge listesi | `/Student` |
| Evrak yükle | `/Documents/Upload` |
| Rapor | `/Documents/Report/{id}` |
| Staj günü seçimi | `/Documents/InternshipDays/{id}` |
| Google Form URL | `/Documents/GoogleForm/{id}` |
| Admin başvurular (filtre, sayfalama) | `/Admin/Documents` |
| Admin tamamlananlar | `/Admin/Completed` |
| Admin detay / düzenle | `/Admin/Details/{id}`, `/Admin/Edit/{id}` |
| Veri seti yükle | `/Dataset/Upload` |

Öğrenci yalnızca kendi belgesini görebilir. Admin sayfalarına öğrenciyle girilirse 403 döner.

## Belge statü akışı

```
Yüklendi → İnceleniyor → Hatalı/Eksik → (düzeltilip yeniden yüklenir) → Onaylandı → Tamamlandı
```

Geçişler `DocumentStatusService` içinde tek yerde denetlenir, geçersiz geçiş hata verir. **Tamamlandı** belge öğrenci tarafından değiştirilemez ve silinemez, bu kontrol sunucu tarafındadır. Admin Tamamlandı belgeyi düzenleyebilir ve silebilir. Hatalı/Eksik belge yeniden yüklenirken aynı kayıt güncellenir ve form eski bilgilerle dolu gelir.

## Doğrulama kuralları

| Kural | Ne kontrol eder |
|---|---|
| Kimlik numarası | 11 hane, T.C. için ilk hane 0 olamaz ve algoritma, yabancı için `99` ile başlama |
| Telefon biçimi | `+90 xxx xxx xx xx` |
| E-posta biçimi | Öğrenci, işyeri ve işveren e-postaları |
| Mühendis sayısı | Boş ya da 1'in altı hata, 5'in altı uyarı |
| Form ve belge karşılaştırma | Formdaki bilgi ile belgeden okunan bilgi |
| Veri seti karşılaştırma | Belgedeki bilgi ile veri setindeki resmi kayıt (uyarı düzeyinde, admin detayında ayrı gösterilir, öğrenci raporunda gizlidir) |

Yükleme sırasında dosya biçimi (`.doc`, `.docx`, PDF, JPEG, PNG), boş dosya ve 10 MB sınırı kontrol edilir. `.docx` doğrudan, `.doc` LibreOffice ile çevrilerek okunur. Metin tabanlı `.pdf` okunur. JPEG ve PNG yüklenir ama içerikleri okunamadığı için belge Hatalı / Eksik olur (JPEG okuyucusu iskelettir, PNG için okuyucu yoktur).

## Proje yapısı

```
Diyagramlar/          Mermaid diyagram dosyaları
Rapor/                Vize raporu (PDF)
StajSistemi/
  Controllers/        Ekran mantığı (Auth, Student, Documents, Admin, Dataset, Home)
  Views/              Razor ekranları
  Models/             Ekrana özel ViewModel sınıfları
  DTOs/               Giriş, kayıt ve veri seti sonuç nesneleri
  Domain/             Entities, Enums, Interfaces (ortak çekirdek)
  Validation/         Doğrulama altyapısı ve Rules/ altında tek tek kurallar
  Services/           İş mantığı (Auth, Documents, Dataset, Extractors)
  Data/               AppDbContext, DbSeeder, örnek veri seti (students.csv)
  Migrations/         Tablo planları
  TestBelgeleri/      Örnek Ek-16 belgeleri
  Uploads/            Yüklenen belgeler (git'e girmez)
```

## NYP prensipleri

| Prensip | Örnek |
|---|---|
| Kapsülleme | `User` içinde `PasswordHash`, `FailedLoginCount`, `IsLocked` alanları `private set`, yalnızca `RegisterFailedLogin`, `Unlock` gibi metotlarla değişir. `DocumentStatusService` geçişleri ve Tamamlandı kilidini içinde tutar. `ValidationResult` alanları `init` |
| Soyutlama | `abstract User`, `IValidationRule`, `IDocumentFieldExtractor`, `IValidationService`, `ICurrentUserService`, `IDocumentService`, `IFileStorage` |
| Kalıtım | `Student : User`, `Admin : User`, her kural `ValidationRuleBase`'den türer |
| Çok biçimlilik | `Role` override'ları, doğrulama servisinin tüm `IValidationRule` nesnelerini aynı çağrıyla çalıştırması, `IDatasetReader` ve `IDocumentFieldExtractor` için uzantıya göre uygulama seçimi, `ClaimsCurrentUserService` ve `FakeCurrentUserService` |

## Diyagramlar

| Dosya | İçerik |
|---|---|
| [use-case.md](Diyagramlar/use-case.md) | Use case |
| [class-diyagrami.md](Diyagramlar/class-diyagrami.md) | Class: alan modeli ve doğrulama |
| [class-diyagrami-servisler.md](Diyagramlar/class-diyagrami-servisler.md) | Class: servisler ve controller'lar |
| [er-diyagrami.md](Diyagramlar/er-diyagrami.md) | Veri tabanı (ER) |
| [durum-diyagrami.md](Diyagramlar/durum-diyagrami.md) | UML durum: belge statüleri |
| [sequence-yukleme.md](Diyagramlar/sequence-yukleme.md) | UML sıralama: belge yükleme |
| [akis-belge.md](Diyagramlar/akis-belge.md) | Akış: yükleme ve doğrulama |
| [akis-giris.md](Diyagramlar/akis-giris.md) | Akış: giriş ve kilitleme |
| [akis-kayit.md](Diyagramlar/akis-kayit.md) | Akış: kayıt |
| [akis-veriseti.md](Diyagramlar/akis-veriseti.md) | Akış: veri seti yükleme |
| [akis-staj-gunu.md](Diyagramlar/akis-staj-gunu.md) | Akış: staj günü hesabı (final için tasarım) |

GitHub bu dosyaları açınca diyagramları çizilmiş olarak gösterir.

## Bilinen sınırlamalar

- Staj günü ekranı seçimi kaydeder, `CalculatedDays` hesaplanmaz ve gün sayısı kuralı yoktur (final).
- Google Form URL yalnızca doğrulanır, kaydedilmez. Form'a otomatik kayıt ve Ek-17 üretimi finale kalmıştır, Ek-17 butonu pasiftir.
- Otomatik okuma `.docx` için doğrudan, `.doc` için LibreOffice ile `.docx`'e çevrilerek çalışır (LibreOffice kurulu olmalı, yoksa raporda "belge okunamadı" görünür). Metin tabanlı PDF okunur. Taranmış PDF, JPEG ve PNG için OCR finalde eklenecektir, `JpegFieldExtractor` şimdilik iskelettir ve boş veri döndürür.
- Admin düzenlemesi doğrulamayı yeniden çalıştırmaz ve statüyü değiştirmez.
- Doğrulama çökerse belge İnceleniyor'da takılmasın diye Hatalı/Eksik'e düşer, sebep rapora yazılmaz.
- Çıkışta cookie silinir, JWT sunucuda ayrıca iptal edilmez.
- Kayıtta yalnızca öğrenci numarası eşleşmesi aranır, e-posta eşleşmesi şart değildir.
- `UserName` ve `Email` sütunları MySQL'de `longtext` olduğundan benzersiz indeks yoktur, tekrar kontrolü kodda yapılır.
- Zaman damgaları UTC saklanır.
- Cookie `Secure` bayrağı canlı ortam için ayrıca düşünülmelidir.
- `MySqlConnector` paketi için `NU1902` uyarısı çıkar. Pomelo ve EF Core 9.0.0'a sabit olduğundan paket yükseltilmedi.

## Geliştiriciler

| Ad Soyad | Hat | Sorumluluk |
|---|---|---|
| Sena Aygün | Kişi 1 | Kimlik, güvenlik, veri seti: giriş, şifre hash'leme, kilit, roller, kayıt, veri seti yükleme |
| Öykü Ertuna | Kişi 2 | Doğrulama motoru: kurallar, .docx ve PDF okuma, karşılaştırmalar, ortak veri modeli |
| Fatih Mutlu | Kişi 3 | Arayüz, evrak yükleme, statü akışı, rapor ekranı, admin sayfaları |
