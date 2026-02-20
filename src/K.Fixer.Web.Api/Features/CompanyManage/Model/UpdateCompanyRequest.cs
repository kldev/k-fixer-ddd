namespace K.Fixer.Web.Api.Features.CompanyManage.Model;

public record UpdateCompanyRequest(string Name, string ContactEmail, string? ContactPhone)
  : BaseCompanyRequest(Name, "", ContactEmail, ContactPhone);