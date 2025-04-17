namespace PlateSecure.Domain.Specifications;

public class UserFilter
{
    public string? Username { get; set; } = null;
    public string? Password { get; set; } = null;
    public string? Role { get; set; } = null;
    public DateTime? StartDate { get; set; } = null;
    public DateTime? EndDate { get; set; } = null;
    public string SortBy { get; set; } = "CreateDate";
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}