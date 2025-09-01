using Microsoft.AspNetCore.Authorization;

namespace Api.Authorization;

public class EnergyCompanyManagerAttribute : AuthorizeAttribute
{
    public EnergyCompanyManagerAttribute()
    {
        Roles = "Energy Company Account Manager";
    }
}