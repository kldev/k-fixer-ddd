using Common.Toolkit.ResultPattern;

using K.Fixer.Domain.PropertyManagement.Aggregates.Building;
using K.Fixer.Domain.PropertyManagement.Repositories;

namespace K.Fixer.Application.PropertyManagement.CreateCompanyBuilding;

public sealed class CreateCompanyBuildingHandler
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyBuildingRepository _buildingRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CreateCompanyBuildingHandler(
        ICompanyRepository companyRepository,
        ICompanyBuildingRepository buildingRepository)
    {
        _companyRepository = companyRepository;
        _buildingRepository = buildingRepository;
    }

    public async Task<Result<CreateCompanyBuildingResult>> HandleAsync(
        CreateCompanyBuildingCommand command,
        CancellationToken ct = default)
    {
        var company = await _companyRepository.GetByIdAsync(command.CompanyId, ct);
        if (company is null)
            return new NotFoundError("Company.notFound",
                $"The company with id '{command.CompanyId}' not found.");
        
        var isNameUnique = await _buildingRepository.IsNameUniqueAsync(command.Name,company.Id, null, ct);
        if (!isNameUnique)
            return  new BusinessLogicError("CreateCompanyBuilding.nameAlreadyInSystem",
                $"The building name '{command.Name}' is already in system.");

        var buildingResult = Building.Create(
            command.CompanyId,
            command.Name,
            command.Street,
            command.City,
            command.PostalCode,
            command.CountryCode);

        if (buildingResult.IsFailure)
            return new ValidationError("CreateCompanyBuilding.formErrors", buildingResult.Error!);

        var building = buildingResult.Value!;

        await _buildingRepository.AddAsync(building, ct);
        await _buildingRepository.SaveChangesAsync(ct);

        return new CreateCompanyBuildingResult(building.Id);
    }
}
