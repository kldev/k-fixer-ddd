using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Events;
using K.Fixer.Domain.IAM.Repositories;
using K.Fixer.Domain.Maintenance.Repositories;

namespace K.Fixer.Application.Maintenance.ReopenRequest;

public sealed class ReopenRequestHandler
{
    private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly IUserRepository               _userRepository;
    private readonly IDomainEventDispatcher        _eventDispatcher;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ReopenRequestHandler(
        IMaintenanceRequestRepository requestRepository,
        IUserRepository userRepository,
        IDomainEventDispatcher eventDispatcher)
    {
        _requestRepository = requestRepository;
        _userRepository    = userRepository;
        _eventDispatcher   = eventDispatcher;
    }

    public async Task<Result<ReopenRequestResult>> HandleAsync(
        ReopenRequestCommand command,
        CancellationToken ct = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.MaintenanceRequestId, ct);
        if (request is null)
            return new NotFoundError("ReopenRequest.notFound", "Maintenance request not found.");

        var admin = await _userRepository.GetById(command.AdminId, ct);
        if (admin is null)
            return new NotFoundError("ReopenRequest.adminNotFound", "User not found.");

        var result = request.Reopen(command.AdminId, command.Reason);
        if (result.IsFailure)
            return new ValidationError("ReopenRequest.transitionFailed", result.Error!);

        await _requestRepository.SaveChangesAsync(ct);

        await _eventDispatcher.DispatchAsync(request.DomainEvents, ct);
        request.ClearDomainEvents();

        return new ReopenRequestResult();
    }
}
