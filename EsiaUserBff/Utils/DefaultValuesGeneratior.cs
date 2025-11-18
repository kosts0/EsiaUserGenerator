using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Dto.Model;
using Bogus;
namespace EsiaUserGenerator.Utils;

public static class DefaultValuesGeneratior
{
    public static CreateUserData GenarateDefaultValues(this CreateUserData data)
    {
        data.EsiaAuthInfo ??= new();
        data.EsiaAuthInfo.Phone ??= _faker.Phone.PhoneNumber("+7(###)#######");
        data.EsiaUserInfo ??= new EsiaUserInfo();
        data.EsiaUserInfo.FirstName = _faker.Person.FirstName;
        data.EsiaUserInfo.LastName = _faker.Person.LastName;
        data.EsiaAuthInfo.Password ??= "Test123456!";
        return data;
    }

    private static Faker _faker;

    static DefaultValuesGeneratior()
    {
        _faker = new Faker("ru");
    }
}