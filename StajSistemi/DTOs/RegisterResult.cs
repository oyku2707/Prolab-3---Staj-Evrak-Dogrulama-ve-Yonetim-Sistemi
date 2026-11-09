namespace StajSistemi.DTOs
{
    public enum RegisterStatus
    {
        Success,
        UserNameTaken,
        EmailTaken,
        StudentNoAlreadyLinked
    }

    public class RegisterResult
    {
        public RegisterStatus Status { get; set; }

        // Öğrenci numarası veri setinde bulundu ve hesaba bağlandı mı?
        public bool ProfileLinked { get; set; }
    }
}