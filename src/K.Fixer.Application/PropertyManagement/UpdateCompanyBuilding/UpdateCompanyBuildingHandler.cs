using Common.Toolkit.ResultPattern;

using K.Fixer.Domain.PropertyManagement.Repositories;

namespace K.Fixer.Application.PropertyManagement.UpdateCompanyBuilding;

public sealed class UpdateCompanyBuildingHandler
{
    private readonly ICompanyBuildingRepository _buildingRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UpdateCompanyBuildingHandler(ICompanyBuildingRepository buildingRepository)
        => _buildingRepository = buildingRepository;

    public async Task<Result<UpdateCompanyBuildingResult>> HandleAsync(
        UpdateCompanyBuildingCommand command,
        CancellationToken ct = default)
    {
        var building = await _buildingRepository.GetByIdAsync(command.BuildingId, ct);

        if (building is null)
            return new NotFoundError("Building.notFound",
                $"The building with id '{command.BuildingId}' not found.");

        if (building.CompanyId != command.CompanyId)
            return new NotFoundError("Building.notFound",
                $"The building with id '{command.BuildingId}' not found.");
        
        var isNameUnique = await _buildingRepository.IsNameUniqueAsync(command.Name,building.CompanyId, building.Id, ct);
        if (!isNameUnique)
            return  new BusinessLogicError("UpdateCompanyBuilding.nameAlreadyInSystem",
                $"The building name '{command.Name}' is already in system.");

        var updateResult = building.Update(
            command.Name,
            command.Street,
            command.City,
            command.PostalCode,
            command.CountryCode);

        if (updateResult.IsFailure)
            return new ValidationError("UpdateCompanyBuilding.formErrors", updateResult.Error!);

        await _buildingRepository.SaveChangesAsync(ct);

        return new UpdateCompanyBuildingResult();
    }
}
