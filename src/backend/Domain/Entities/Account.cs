namespace Domain.Entities;

public class Account
{
    public int AccountId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
    
    public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
}