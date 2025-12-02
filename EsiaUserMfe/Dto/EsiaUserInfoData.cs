using System;
using System.ComponentModel.DataAnnotations;
using EsiaUserMfe.DataAnnotations;

namespace EsiaUserMfe.Dto;

public class PassportInfo
{
    public string? Number { get; set; }
    public string? Series { get; set; }
    public string? Type { get; set; } = "RF_PASSPORT";
    public string? IssueId { get; set; }
    [PassportDivision]
    public string? IssuedBy { get; set; }
    public DateTime? IssueDate { get; set; }
}

public class EsiaUserInfo
{
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? Gender { get; set; }          // M / F
    public DateTime? BirthDate { get; set; }
    public string? BirthPlace { get; set; }
    public string? Citizenship { get; set; }
    [Snils]
    public string? Snils { get; set; }
    public PassportInfo Passport { get; set; } = new();
}

public class AuthInfo
{
    [System.ComponentModel.DataAnnotations.Phone]
    public string? Phone { get; set; }
    public string? Password { get; set; }
}

public class EsiaUserRequest
{
    public EsiaUserInfo EsiaUserInfo { get; set; } = new();
    public AuthInfo AuthInfo { get; set; } = new();
}

