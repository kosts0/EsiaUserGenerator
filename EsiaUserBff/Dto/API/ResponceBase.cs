namespace EsiaUserGenerator.Dto;

public abstract class ResponceBase<TResponseData>
{
    public int? Code { get; set; }
    public string? CodeStatus { get; set; }
    public TResponseData Data { get; set; }
    public string? Exception { get; set; }
    
    
    
}