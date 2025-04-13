namespace PlateSecure.Application.DTOs;

public class StatisticsResponse
{
    public string Period { get; set; }
    public int TotalEvents { get; set; }
    public double TotalRevenue { get; set; }
}