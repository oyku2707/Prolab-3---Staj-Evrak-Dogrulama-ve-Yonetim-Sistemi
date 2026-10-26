using StajSistemi.Domain.Enums;

namespace StajSistemi.Domain.Entities;

public class Admin : User
{
    public override RoleType Role => RoleType.Admin;

}