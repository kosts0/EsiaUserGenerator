using EsiaUserGenerator.Dto;
using EsiaUserGenerator.Dto.Model;
using Bogus;
using Bogus.DataSets;

namespace EsiaUserGenerator.Utils;

public static class DefaultValuesGeneratior
{
    public static CreateUserData GenarateDefaultValues(this CreateUserData data)
    {
        var person = new Person(locale: "ru");
        data.EsiaAuthInfo ??= new();
        data.EsiaAuthInfo.Phone ??= "+7"+person.Phone.Replace("-","");
        data.EsiaUserInfo ??= new EsiaUserInfo();
       
        data.EsiaUserInfo.FirstName ??= person.FirstName;
        data.EsiaUserInfo.LastName ??= person.LastName;
        data.EsiaUserInfo.Gender ??= person.Gender == Name.Gender.Male ? "M" : "F";
        data.EsiaUserInfo.BirthDate ??= person.DateOfBirth;
        data.EsiaUserInfo.BirthPlace ??= person.Address.City;
        data.EsiaUserInfo.Citizenship ??= "RUS";
        data.EsiaUserInfo.Snils ??= GenerateSnils();
        data.EsiaAuthInfo.Password ??= "Test123456!";
        data.EsiaUserInfo.Documents ??= new EsiaUserInfo.DocumentsInfo()
        {
            Elements = new []
            {
                new EsiaUserInfo.DocumentsInfo.Element()
                {
                    Type = "RF_PASSPORT",
                }
            }
        };
        foreach (var document in data.EsiaUserInfo.Documents.Elements)
        {
            document.GenerateDefaultDocumentValue();
        }
        return data;
    }

    private static EsiaUserInfo.DocumentsInfo.Element GenerateDefaultDocumentValue(this EsiaUserInfo.DocumentsInfo.Element documentInfo)
    {
        documentInfo.Type ??= "RF_PASSPORT";
        switch (documentInfo.Type)
        {
            case "RF_PASSPORT":
                documentInfo.Number ??= _faker.Random.ReplaceNumbers("######");
                documentInfo.Series ??= _faker.Random.ReplaceNumbers("####");
                documentInfo.IssueDate ??=  _faker.Date.Recent();
                documentInfo.IssueId  ??= _faker.Random.ReplaceNumbers("######");
                documentInfo.IssuedBy ??= $"умвд автотестирования №{_faker.Random.Int(1, 100)}";
                return documentInfo;
            default: throw new NotImplementedException($"Not implemented for document type {documentInfo.Type}"); 
        }
    }
    private static readonly Faker _faker;
    private static readonly Random _rnd;

    // Сгенерировать корректный СНИЛС в виде "XXX-XXX-XXX YY"
    public static string GenerateSnils()
    {
        int ComputeChecksumFor9Digits(int[] digits)
        {
            if (digits == null || digits.Length != 9) throw new ArgumentException("должно быть 9 цифр");

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                int weight = 9 - i; // веса 9,8,...,1
                sum += digits[i] * weight;
            }

            if (sum < 100) return sum;
            if (sum == 100 || sum == 101) return 0;
            int rem = sum % 101;
            if (rem == 100) return 0;
            return rem;
        }
        // Генерируем 9 цифр; избегаем "000000000"
        int[] digits;
        do
        {
            digits = Enumerable.Range(0, 9).Select(_ => _rnd.Next(0, 10)).ToArray();
        } while (digits.All(d => d == 0));

        int checksum = ComputeChecksumFor9Digits(digits);
        string numberPart = string.Concat(digits);
        string checkPart = checksum.ToString("D2"); // два разряда

        // Форматирование XXX-XXX-XXX YY
        string formatted =
            $"{numberPart.Substring(0,3)}-{numberPart.Substring(3,3)}-{numberPart.Substring(6,3)} {checkPart}";
        return formatted;
    }
    static DefaultValuesGeneratior()
    {
        _faker = new Faker("ru");
        _rnd = new Random();
    }
}