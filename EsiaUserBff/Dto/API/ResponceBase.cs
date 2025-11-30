namespace EsiaUserGenerator.Dto.API;

public abstract class ResponceBase<TResponseData>
{
    public int? Code { get; set; }
    public string? CodeStatus { get; set; }
    public TResponseData Data { get; set; }
    public string? ExceptionMessage { get; set; }
    public  System.Exception Exception { get; set; }
    
    
    
}