using System.Runtime.CompilerServices;

namespace EsiaUserGenerator.Db.Models;

public class EsiaUser
{
    public Guid Id { get; set; }
    
    public string? Oid { get; set; }

    public string? Login { get; set; }
    public string? Password { get; set; }

    public Guid? CreatedRequestId { get; set; }

    public RequestHistory RequestData { get; set; } = null!;
}
