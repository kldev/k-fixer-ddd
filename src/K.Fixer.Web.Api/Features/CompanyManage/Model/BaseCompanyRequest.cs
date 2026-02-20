namespace K.Fixer.Web.Api.Features.CompanyManage.Model;

public abstract record BaseCompanyRequest(string Name, string TaxId, string ContactEmail, string? ContactPhone);