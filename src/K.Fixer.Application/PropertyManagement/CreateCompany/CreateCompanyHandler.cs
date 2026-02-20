using Common.Toolkit.ResultPattern;

using K.Fixer.Domain.PropertyManagement.Aggregates.Company;
using K.Fixer.Domain.PropertyManagement.Repositories;

namespace K.Fixer.Application.PropertyManagement.CreateCompany;

public sealed class CreateCompanyHandler
{
    private readonly ICompanyRepository _companyRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CreateCompanyHandler(ICompanyRepository companyRepository)
        => _companyRepository = companyRepository;

    public async Task<Result<CreateCompanyResult>> HandleAsync(
        CreateCompanyCommand command,
        CancellationToken ct = default)
    {
        var isNameUnique = await _companyRepository.IsNameUniqueAsync(command.Name,null, ct);
        if (!isNameUnique)
            return  new BusinessLogicError("CreateCompany.nameAlreadyInSystem",
                $"The company name '{command.Name}' is already in system.");
        
        var isTaxIdUnique = await _companyRepository.IsTaxIdUniqueAsync(
            command.TaxIdCountryCode, command.TaxIdNumber, ct);
        if (!isTaxIdUnique)
            return new BusinessLogicError("CreateCompany.taxIdAlreadyInSystem",
                "The companny with tax id aleady in system.");
        
        var companyResult = Company.Create(
            command.Name,
            command.TaxIdCountryCode,
            command.TaxIdNumber,
            command.ContactEmail,
            command.ContactPhone);

        if (companyResult.IsFailure)
            return new ValidationError("CreateCompany.formErrors", companyResult.Error!);

        var company = companyResult.Value!;
        
        await _companyRepository.AddAsync(company, ct);
        await _companyRepository.SaveChangesAsync(ct);

        return new CreateCompanyResult(company.Id);
    }
}