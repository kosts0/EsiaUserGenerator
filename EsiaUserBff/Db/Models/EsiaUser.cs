using System.Runtime.CompilerServices;

namespace EsiaUserGenerator.Db.Models;

public class EsiaUser
{
    public Guid Id { get; set; }
    
    public string? Oid { get; set; }
    public string? Status { get; set; }
    public DateTime DateTimeCreated { get; set; }

    public string? Login { get; set; }
    public string? Password { get; set; }

    public Guid? CreatedRequestId { get; set; }

    // Навигационные свойства
    public ICollection<CreatedHistory> CreatedHistories { get; set; } = new List<CreatedHistory>();
    public ICollection<RequestHistory> RequestHistories { get; set; } = new List<RequestHistory>();
}
