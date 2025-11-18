namespace EsiaUserGenerator.Dto;

public class ResponceBase<TResponseData>
{
    public int? Code { get; set; }
    public int? StatusCode { get; set; }
    public TResponseData Data { get; set; }
    public Exception? Exception { get; set; }
    
    
    
}