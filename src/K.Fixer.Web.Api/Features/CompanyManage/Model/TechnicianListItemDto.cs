namespace K.Fixer.Web.Api.Features.CompanyManage.Model;

public class TechnicianListItemDto
{
    public Guid Gid { get; set; }
    public string FullName { get; set; } = string.Empty; 
    
    public string Email { get; set; } = string.Empty;
}