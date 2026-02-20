using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Events;
using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.IAM.Repositories;
using K.Fixer.Domain.Maintenance.Repositories;

namespace K.Fixer.Application.Maintenance.StartWorkOnRequest;

public sealed class StartWorkOnRequestHandler
{
    private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly IUserRepository               _userRepository;
    private readonly IDomainEventDispatcher        _eventDispatcher;

    // ReSharper disable once ConvertToPrimaryConstructor
    public StartWorkOnRequestHandler(
        IMaintenanceRequestRepository requestRepository,
        IUserRepository userRepository,
        IDomainEventDispatcher eventDispatcher)
    {
        _requestRepository = requestRepository;
        _userRepository    = userRepository;
        _eventDispatcher   = eventDispatcher;
    }

    public async Task<Result<StartWorkOnRequestResult>> HandleAsync(
        StartWorkOnRequestCommand command,
        CancellationToken ct = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.MaintenanceRequestId, ct);
        if (request is null)
            return new NotFoundError("StartWorkOnRequest.notFound", "Maintenance request not found.");

        var technician = await _userRepository.GetById(command.TechnicianId, ct);
        if (technician is null)
            return new NotFoundError("StartWorkOnRequest.technicianNotFound", "Technician not found.");

        if (technician.Role != UserRole.Technician)
            return new ValidationError("StartWorkOnRequest.notATechnician", "The specified user is not a technician.");

        var result = request.StartWork(command.TechnicianId);
        if (result.IsFailure)
            return new ValidationError("StartWorkOnRequest.transitionFailed", result.Error!);

        await _requestRepository.SaveChangesAsync(ct);

        await _eventDispatcher.DispatchAsync(request.DomainEvents, ct);
        request.ClearDomainEvents();

        return new StartWorkOnRequestResult();
    }
}
