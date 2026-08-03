namespace DirectService.Infrastructure.BackgroundServices.SoftDelete;

public class SoftDeleteOptions
{
    public const string SOFT_DELETE = "SoftDelete";

    public int ExpiredDaysToRemove { get; set; }
    
    public int DeleteExpiredServiceTimeOutMinutes { get; set; }
}