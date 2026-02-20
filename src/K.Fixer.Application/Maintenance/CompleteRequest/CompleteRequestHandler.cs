using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Events;
using K.Fixer.Application.Maintenance.CloseRequest;
using K.Fixer.Domain.IAM.Repositories;
using K.Fixer.Domain.Maintenance.Repositories;

namespace K.Fixer.Application.Maintenance.CompleteRequest;

public class CompleteRequestHandler
{
    private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly IUserRepository               _userRepository;
    private readonly IDomainEventDispatcher        _eventDispatcher;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompleteRequestHandler(
        IMaintenanceRequestRepository requestRepository,
        IUserRepository userRepository,
        IDomainEventDispatcher eventDispatcher)
    {
        _requestRepository = requestRepository;
        _userRepository    = userRepository;
        _eventDispatcher   = eventDispatcher;
    }

    public async Task<Result<CompleteRequestResult>> HandleAsync(
        CompleteRequestCommand command,
        CancellationToken ct = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.MaintenanceRequestId, ct);
        if (request is null)
            return new NotFoundError("CompleteRequest.notFound", "Maintenance request not found.");

        var technician = await _userRepository.GetById(command.TechnicianId, ct);
        if (technician is null)
            return new NotFoundError("CompleteRequest.adminNotFound", "User not found.");

        var result = request.Complete(command.TechnicianId, command.ResolutionNote);
        if (result.IsFailure)
            return new ValidationError("CompleteRequest.transitionFailed", result.Error!);

        await _requestRepository.AddLogsAsync(request.StatusLog);
        await _requestRepository.SaveChangesAsync(ct);
        await _eventDispatcher.DispatchAsync(request.DomainEvents, ct);
        request.ClearDomainEvents();

        return new CompleteRequestResult();
    }
}