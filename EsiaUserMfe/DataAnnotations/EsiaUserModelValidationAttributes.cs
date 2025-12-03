using System;
using System.ComponentModel.DataAnnotations;

namespace EsiaUserMfe.DataAnnotations;

public class SnilsAttribute : RegularExpressionAttribute
{
    public SnilsAttribute() : base(@"^\d{3}-\d{3}-\d{3} \d{2}$")
    {
        ErrorMessage = "Неверный формат СНИЛС";
    }
}

public class PhoneAttribute : RegularExpressionAttribute
{
    public PhoneAttribute() : base(@"^\+7\(\d{3}\)\d{7}$")
    {
        ErrorMessage = "Формат телефона должен быть: +7(999)9999999";
    }
}

public class PassportDivisionAttribute : RegularExpressionAttribute
{
    public PassportDivisionAttribute() : base(@"^\d{3}-\d{3}$")
    {
        ErrorMessage = "Код подразделения: 000-000";
    }
}

