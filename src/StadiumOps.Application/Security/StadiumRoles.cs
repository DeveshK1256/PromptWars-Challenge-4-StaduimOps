namespace StadiumOps.Application.Security;

public static class StadiumRoles
{
    public const string RegisteredFan = "RegisteredFan";
    public const string Volunteer = "Volunteer";
    public const string StadiumStaff = "StadiumStaff";
    public const string SecurityOfficer = "SecurityOfficer";
    public const string MedicalTeam = "MedicalTeam";
    public const string TransportationOperator = "TransportationOperator";
    public const string SustainabilityOfficer = "SustainabilityOfficer";
    public const string OperationsManager = "OperationsManager";
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";

    public static readonly string[] All =
    [
        RegisteredFan,
        Volunteer,
        StadiumStaff,
        SecurityOfficer,
        MedicalTeam,
        TransportationOperator,
        SustainabilityOfficer,
        OperationsManager,
        Admin,
        SuperAdmin
    ];

    public static readonly string[] SelfRegistrable =
    [
        RegisteredFan,
        Volunteer
    ];

    public static readonly string[] OperationsAccess =
    [
        OperationsManager,
        Admin,
        SuperAdmin,
        SecurityOfficer,
        MedicalTeam,
        TransportationOperator,
        SustainabilityOfficer
    ];

    public static readonly string[] IncidentAccess =
    [
        SecurityOfficer,
        MedicalTeam,
        OperationsManager,
        Admin,
        SuperAdmin
    ];
}
