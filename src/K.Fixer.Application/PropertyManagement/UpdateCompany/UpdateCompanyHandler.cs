using Common.Toolkit.ResultPattern;

using K.Fixer.Domain.PropertyManagement.Repositories;

namespace K.Fixer.Application.PropertyManagement.UpdateCompany;

public sealed class UpdateCompanyHandler
{
    private readonly ICompanyRepository _companyRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UpdateCompanyHandler(ICompanyRepository companyRepository)
        => _companyRepository = companyRepository;

    public async Task<Result<UpdateCompanyResult>> HandleAsync(
        UpdateCompanyCommand command,
        CancellationToken ct = default)
    {
        var company = await _companyRepository.GetByIdAsync(command.CompanyId, ct);

        if (company is null)
            return new NotFoundError("Company.notFound", $"The company with id '{command.CompanyId}' not found");

        company.UpdateContactInfo(command.ContactEmail, command.ContactPhone);

        if (company.Name.Value != command.Name)
        {
            var isNameUnique = await _companyRepository.IsNameUniqueAsync(command.Name,null, ct);
            if (!isNameUnique)
                return  new BusinessLogicError("UpdateCompany.nameAlreadyInSystem",
                    $"The company name '{command.Name}' is already in system.");
            
            company.Rename(command.Name);
        }
        
        await _companyRepository.SaveChangesAsync(ct);
        
        return new UpdateCompanyResult();

    }
}