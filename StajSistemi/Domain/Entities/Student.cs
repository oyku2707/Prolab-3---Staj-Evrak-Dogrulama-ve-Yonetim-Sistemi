using StajSistemi.Domain.Enums;

namespace StajSistemi.Domain.Entities;

public class Student : User
{
    public override RoleType Role => RoleType.Student;

    public StudentProfile? Profile { get; set; }
}