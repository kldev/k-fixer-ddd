using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.IAM.Repositories;
using K.Fixer.Domain.PropertyManagement.Repositories;
using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.Maintenance.Services;

public sealed class ResidentEligibilityService
{
    private readonly ICompanyBuildingRepository _buildingRepository;
    private readonly IUserRepository     _userRepository;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ResidentEligibilityService(
        ICompanyBuildingRepository buildingRepository,
        IUserRepository userRepository)
    {
        _buildingRepository = buildingRepository;
        _userRepository     = userRepository;
    }

    public async Task<Result> CanCreateRequestAsync(
        Guid residentId,
        Guid buildingId,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetById(residentId, ct);
        if (user is null)
            return Result.Failure("User does not exist.");

        if (!user.CanLogin())
            return Result.Failure("User account is inactive or blocked.");

        if (user.Role != UserRole.Resident)
            return Result.Failure("Only residents can create maintenance requests.");

        var building = await _buildingRepository
            .GetByIdWithOccupanciesAsync(buildingId, ct);

        if (building is null)
            return Result.Failure("Building does not exist.");

        if (!building.IsActive)
            return Result.Failure("Building is inactive.");

        if (!building.HasActiveResident(residentId))
            return Result.Failure(
                "Resident does not have an active assignment to this building. " +
                "Only current residents can create requests.");

        return Result.Success();
    }
}