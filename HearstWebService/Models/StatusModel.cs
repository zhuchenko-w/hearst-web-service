
namespace HearstWebService.Models
{
    public enum RequestTypes
    {
        CreateReport,
        ApproveData,
        ActualToPm,
        TransferData,
        LockScenario
    }

    public class StatusModel
    {
        public RequestTypes RequestType { get; set; }
        public string Url { get; set; }
    }
}