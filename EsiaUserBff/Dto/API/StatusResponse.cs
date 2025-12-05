namespace EsiaUserGenerator.Dto.API;

public class StatusResponse : ResponceBase<StatusData>
{
    
}

public class StatusData
{
    public Guid? RequestId { get; set; }
    public string RequestJsonData { get; set; }
    public string CurrentStauts { get; set; }
}