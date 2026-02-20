using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Events;
using K.Fixer.Domain.IAM.Repositories;
using K.Fixer.Domain.Maintenance.Repositories;

namespace K.Fixer.Application.Maintenance.ChangeRequestPriority;

public sealed class ChangeRequestPriorityHandler
{
    private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly IUserRepository               _userRepository;
    private readonly IDomainEventDispatcher        _eventDispatcher;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ChangeRequestPriorityHandler(
        IMaintenanceRequestRepository requestRepository,
        IUserRepository userRepository,
        IDomainEventDispatcher eventDispatcher)
    {
        _requestRepository = requestRepository;
        _userRepository    = userRepository;
        _eventDispatcher   = eventDispatcher;
    }

    public async Task<Result<ChangeRequestPriorityResult>> HandleAsync(
        ChangeRequestPriorityCommand command,
        CancellationToken ct = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.MaintenanceRequestId, ct);
        if (request is null)
            return new NotFoundError("ChangeRequestPriority.notFound", "Maintenance request not found.");

        var buildings = await _requestRepository.GetBuildingsByIdsAsync(new[] { request.BuildingId }, ct);
        var building = buildings.FirstOrDefault();
        if (building is null || building.CompanyId != command.AdminCompanyId)
            return new NotFoundError("ChangeRequestPriority.notFound", "Maintenance request not found.");

        var user = await _userRepository.GetById(command.ChangedById, ct);
        if (user is null)
            return new NotFoundError("ChangeRequestPriority.userNotFound", "User not found.");

        var result = request.ChangePriority(command.NewPriority, command.ChangedById);
        if (result.IsFailure)
            return new ValidationError("ChangeRequestPriority.failed", result.Error!);

        await _requestRepository.SaveChangesAsync(ct);

        await _eventDispatcher.DispatchAsync(request.DomainEvents, ct);
        request.ClearDomainEvents();

        return new ChangeRequestPriorityResult();
    }
}
