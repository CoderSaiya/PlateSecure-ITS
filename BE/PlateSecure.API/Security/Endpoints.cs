namespace PlateSecure.Security;

public class Endpoints
{
    public static readonly string[] Public =
    [
        "/api/auth/login"
    ];

    public static readonly string[] Staff =
    [
        "/api/detection/entry",
        "/api/detection/exit",
        
    ];

    public static readonly string[] Admin =
    [
        "/api/auth/register",
        "/api/detection/*"
    ];
}