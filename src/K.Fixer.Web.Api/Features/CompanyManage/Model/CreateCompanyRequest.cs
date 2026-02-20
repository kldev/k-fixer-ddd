namespace K.Fixer.Web.Api.Features.CompanyManage.Model;

public record CreateCompanyRequest(string Name, string TaxId, string ContactEmail, string? ContactPhone)
  : BaseCompanyRequest(Name, TaxId, ContactEmail, ContactPhone);