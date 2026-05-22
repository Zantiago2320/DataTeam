namespace DataTeam.Models;

/// <summary>
/// Roles del sistema
/// </summary>
public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Lider = "Lider";
    public const string User = "User";

    public static string[] AllRoles => new[] { SuperAdmin, Admin, Lider, User };

    public static string GetDisplayName(string role)
    {
        return role switch
        {
            SuperAdmin => "Super Administrador (Alexander)",
            Admin => "Administrador (Sergio)",
            Lider => "Líder de Equipo/Célula",
            User => "Usuario",
            _ => role
        };
    }

    public static string GetDescription(string role)
    {
        return role switch
        {
            SuperAdmin => "Acceso total - Puede crear, editar, eliminar y deshabilitar",
            Admin => "Puede crear y editar - NO puede eliminar ni deshabilitar",
            Lider => "Puede gestionar su equipo/célula - Visualización ampliada",
            User => "Solo visualización de datos",
            _ => "Sin descripción"
        };
    }
}
