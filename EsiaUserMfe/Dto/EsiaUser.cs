namespace EsiaUserMfe.Dto;

public class EsiaUser
{
    public string? Oid { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? Status { get; set; }
    public DateTime? DateTimeCreated { get; set; }
}