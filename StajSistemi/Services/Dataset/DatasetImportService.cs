using System.Globalization;
using Microsoft.EntityFrameworkCore;
using StajSistemi.Data;
using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;
using StajSistemi.DTOs;
using StajSistemi.Helpers;

namespace StajSistemi.Services.Dataset
{
    public class DatasetImportService
    {
        private readonly AppDbContext _context;
        private readonly IEnumerable<IDatasetReader> _readers;

        public DatasetImportService(AppDbContext context, IEnumerable<IDatasetReader> readers)
        {
            _context = context;
            _readers = readers;
        }

        public async Task<DatasetImportResult> ImportAsync(Stream stream, string extension)
        {
            var result = new DatasetImportResult();

            // Çok biçimlilik: uzantıya uygun okuyucu seçilir, hangisi olduğu bilinmez
            var reader = _readers.FirstOrDefault(r => r.Supports(extension));
            if (reader == null)
            {
                result.Error = "Desteklenmeyen dosya türü. Şu an sadece .csv kabul edilir.";
                return result;
            }

            var seenStudentNos = new HashSet<string>();
            var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in reader.Read(stream))
            {
                var f = row.Fields;

                // Sütun sırası: FullName, TcNo, Nationality, StudentNo, BirthDate, Address,
                // Department, PreviousInternshipDays, SubmissionYear, Email, Phone, HealthAidStatus
                if (f.Length < 12)
                {
                    Reject(result, row, $"Eksik sütun (12 bekleniyor, {f.Length} var).");
                    continue;
                }

                string fullName = f[0], tcNo = f[1], nationalityText = f[2], studentNo = f[3];
                string address = f[5], department = f[6], submissionYear = f[8];
                string email = f[9], phone = f[10];

                if (string.IsNullOrWhiteSpace(studentNo)) { Reject(result, row, "Öğrenci numarası boş."); continue; }
                if (string.IsNullOrWhiteSpace(fullName)) { Reject(result, row, "Ad soyad boş."); continue; }
                if (string.IsNullOrWhiteSpace(tcNo)) { Reject(result, row, "TC kimlik no boş."); continue; }

                if (!DateTime.TryParseExact(f[4], "dd.MM.yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime birthDate))
                { Reject(result, row, $"Doğum tarihi geçersiz: '{f[4]}' (gg.aa.yyyy bekleniyor)."); continue; }

                if (!int.TryParse(f[7], out int prevDays) || prevDays < 0)
                { Reject(result, row, $"Önceki staj gün sayısı geçersiz: '{f[7]}'."); continue; }

                if (!int.TryParse(f[11], out int healthAid) || healthAid < 1 || healthAid > 6)
                { Reject(result, row, $"Sağlık yardımı statüsü 1-6 olmalı: '{f[11]}'."); continue; }

                // Veri seti resmi referans kayıt: geçersiz kimlik numarası olan satır reddedilir
                var rowNationality = ParseNationality(nationalityText);
                var tcError = TcKimlikChecker.Validate(tcNo.Trim(), rowNationality);
                if (tcError != null)
                { Reject(result, row, $"Kimlik numarası geçersiz ({tcNo}): {tcError}"); continue; }

                // Telefon +90 xxx xxx xx xx biçimine çevrilir; çevrilemiyorsa satır reddedilir
                if (!PhoneNormalizer.TryNormalize(phone, out string normalizedPhone))
                { Reject(result, row, $"Telefon numarası geçersiz: '{phone}' (10 haneli numara bekleniyor)."); continue; }

                if (!seenStudentNos.Add(studentNo))
                { Reject(result, row, $"Öğrenci numarası dosyada tekrar ediyor: {studentNo}."); continue; }

                if (!string.IsNullOrWhiteSpace(email) && !seenEmails.Add(email))
                { Reject(result, row, $"E-posta dosyada tekrar ediyor: {email}."); continue; }

                var nationality = rowNationality;

                // Aynı öğrenci no varsa güncelle (StudentId'ye dokunma), yoksa oluştur
                var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.StudentNo == studentNo);
                bool isNew = profile == null;
                if (isNew)
                    profile = new StudentProfile { StudentNo = studentNo }; // StudentId boş kalır

                profile!.FullName = fullName;
                profile.TcNo = tcNo.Trim(); // metin olarak saklanır, baştaki sıfır kaybolmaz
                profile.Nationality = nationality;
                profile.BirthDate = birthDate;
                profile.Address = address;
                profile.Department = department;
                profile.PreviousInternshipDays = prevDays;
                profile.SubmissionYear = submissionYear;
                profile.Email = email;
                profile.Phone = normalizedPhone;
                profile.HealthAidStatus = healthAid;

                if (isNew)
                {
                    _context.StudentProfiles.Add(profile);
                    result.Added++;
                }
                else
                    result.Updated++;
            }

            await _context.SaveChangesAsync();
            return result;
        }

        private static Nationality ParseNationality(string text)
        {
            var t = text.Trim().ToLowerInvariant();
            return t is "tc" or "t.c." or "t.c" or "türkiye" or "turkiye"
                ? Nationality.TC
                : Nationality.Yabanci;
        }

        private static void Reject(DatasetImportResult result, DatasetRow row, string reason) =>
            result.Rejected.Add(new RejectedRow { LineNumber = row.LineNumber, Reason = reason });
    }
}