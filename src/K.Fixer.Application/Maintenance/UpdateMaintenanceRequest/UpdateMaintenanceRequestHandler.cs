using Common.Toolkit.ResultPattern;
using K.Fixer.Application.Events;
using K.Fixer.Domain.Maintenance.Repositories;

namespace K.Fixer.Application.Maintenance.UpdateMaintenanceRequest;

public sealed class UpdateMaintenanceRequestHandler
{
     private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly IDomainEventDispatcher        _eventDispatcher;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UpdateMaintenanceRequestHandler(
        IMaintenanceRequestRepository requestRepository,
        IDomainEventDispatcher eventDispatcher)
    {
        _requestRepository  = requestRepository;
        _eventDispatcher    = eventDispatcher;
    }

    public async Task<Result<UpdateMaintenanceRequestResult>> HandleAsync(
        UpdateMaintenanceRequestCommand command,
        CancellationToken ct = default)
    {
        var maintenanceRequest = await _requestRepository.GetByIdAsync(command.RequestId, ct);
        
        if (maintenanceRequest == null)
            return new NotFoundError("UpdateMaintenanceRequest.notFound", "Maintenance request not found");
        
        var result = maintenanceRequest.Edit(command.ResidentId, command.Title, command.Description);
        
        if (result.IsFailure)
            return new ValidationError("UpdateMaintenanceRequest.formInvalid", result.Error!); 
        
        await _requestRepository.SaveChangesAsync(ct);
        
        await _eventDispatcher.DispatchAsync(maintenanceRequest.DomainEvents, ct);
        maintenanceRequest.ClearDomainEvents();

        return new UpdateMaintenanceRequestResult();
    }
}