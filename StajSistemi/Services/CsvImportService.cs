using System.IO;
using System.Linq;
using System.Text;
using StajSistemi.Data;
using StajSistemi.Domain.Entities;

namespace StajSistemi.Services
{
    public class CsvImportService
    {
        private readonly AppDbContext _context;

        public CsvImportService(AppDbContext context)
        {
            _context = context;
        }

        public void ImportStudents(string filePath)
        {
            _context.Database.EnsureCreated();

            // Türkçe karakterlerin düzgün okunması için Encoding.UTF8 kullanıyoruz
            var lines = File.ReadAllLines(filePath, Encoding.UTF8).Skip(1);

            foreach (var line in lines)
            {
                // Boş satırları veya '#' ile başlayan yorum/açıklama satırlarını atla
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                // Satır sonundaki inline yorumları temizle (örneğin # TEST 1 kısmını ayıkla)
                string cleanLine = line.Split('#')[0].Trim();

                var columns = cleanLine.Split(',');

                // HATA KONTROLÜ 1: Eksik sütun sayısı (En az 12 sütun olmalı)
                if (columns.Length < 12)
                    continue;

                string fullName = columns[0].Trim();
                string tcNo = columns[1].Trim();
                string nationality = columns[2].Trim();
                string studentNo = columns[3].Trim();
                string birthDate = columns[4].Trim();
                string address = columns[5].Trim();
                string department = columns[6].Trim();
                int.TryParse(columns[7].Trim(), out int prevInternshipDays);
                string submissionYear = columns[8].Trim();
                string email = columns[9].Trim();
                string phone = columns[10].Trim();
                int.TryParse(columns[11].Trim(), out int healthAidStatus);

                // HATA KONTROLÜ 2: Öğrenci numarası boşsa satırı atla
                if (string.IsNullOrWhiteSpace(studentNo))
                    continue;

                // HATA KONTROLÜ 3: Aynı öğrenci numarası zaten varsa tekrar ekleme
                var existingStudent = _context.Students.FirstOrDefault(s => s.UserName == studentNo);
                if (existingStudent != null)
                    continue;

                
                var student = new Student
                {
                    UserName = studentNo,
                    Email = string.IsNullOrEmpty(email) ? $"{studentNo}@kocaeli.edu.tr" : email
                };

                _context.Students.Add(student);
                _context.SaveChanges();

                
                var profile = new StudentProfile
                {
                    StudentId = student.Id,
                    StudentNo = studentNo,
                    FullName = fullName,
                    Department = department
                };

                _context.StudentProfiles.Add(profile);
            }

            _context.SaveChanges();
        }
    }
}