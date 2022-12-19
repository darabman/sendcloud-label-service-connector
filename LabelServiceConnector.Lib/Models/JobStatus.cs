namespace LabelServiceConnector.Lib.Models
{
    public enum JobStatus
    {
        Error = -1,
        None = 0,
        Queued,
        Fetching,
        Printing,
        Saving,
        Complete
    }
}
