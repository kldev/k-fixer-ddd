using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Events;
using K.Fixer.Domain.IAM.Repositories;
using K.Fixer.Domain.Maintenance.Repositories;

namespace K.Fixer.Application.Maintenance.CancelRequest;

public sealed class CancelRequestHandler
{
    private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly IUserRepository               _userRepository;
    private readonly IDomainEventDispatcher        _eventDispatcher;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CancelRequestHandler(
        IMaintenanceRequestRepository requestRepository,
        IUserRepository userRepository,
        IDomainEventDispatcher eventDispatcher)
    {
        _requestRepository = requestRepository;
        _userRepository    = userRepository;
        _eventDispatcher   = eventDispatcher;
    }

    public async Task<Result<CancelRequestResult>> HandleAsync(
        CancelRequestCommand command,
        CancellationToken ct = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.MaintenanceRequestId, ct);
        if (request is null)
            return new NotFoundError("CancelRequest.notFound", "Maintenance request not found.");

        var buildings = await _requestRepository.GetBuildingsByIdsAsync(new[] { request.BuildingId }, ct);
        var building = buildings.FirstOrDefault();
        if (building is null || building.CompanyId != command.AdminCompanyId)
            return new NotFoundError("CancelRequest.notFound", "Maintenance request not found.");

        var user = await _userRepository.GetById(command.CancelByUserId, ct);
        if (user is null)
            return new NotFoundError("CancelRequest.userNotFound", "User not found.");

        var result = request.Cancel(command.CancelByUserId, user.Role.Value);
        if (result.IsFailure)
            return new ValidationError("CancelRequest.transitionFailed", result.Error!);

        await _requestRepository.SaveChangesAsync(ct);

        await _eventDispatcher.DispatchAsync(request.DomainEvents, ct);
        request.ClearDomainEvents();

        return new CancelRequestResult();
    }
}
