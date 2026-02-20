using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Events;
using K.Fixer.Domain.Maintenance.Aggregates.MaintenanceRequest;
using K.Fixer.Domain.Maintenance.Repositories;
using K.Fixer.Domain.Maintenance.Services;

namespace K.Fixer.Application.Maintenance.CreateMaintenanceRequest;

public sealed class CreateMaintenanceRequestHandler
{
    private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly ResidentEligibilityService    _eligibilityService;
    private readonly IDomainEventDispatcher        _eventDispatcher;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CreateMaintenanceRequestHandler(
        IMaintenanceRequestRepository requestRepository,
        ResidentEligibilityService eligibilityService,
        IDomainEventDispatcher eventDispatcher)
    {
        _requestRepository  = requestRepository;
        _eligibilityService = eligibilityService;
        _eventDispatcher    = eventDispatcher;
    }

    public async Task<Result<CreateMaintenanceRequestResult>> HandleAsync(
        CreateMaintenanceRequestCommand command,
        CancellationToken ct = default)
    {
        
        Guid? buildingId;

        if (command.BuildingId == null)
        {
            buildingId = await _requestRepository.GetResidentActiveBuildingAsync(command.ResidentId, ct);
        }
        else
        {
            buildingId = command.BuildingId!.Value;
        }
        
        if (!buildingId.HasValue)
            return new ValidationError("CreateMaintenance.noActiveBuildingForResident", "The is not active assigment for resident");
        
        var eligibilityResult = await _eligibilityService.CanCreateRequestAsync(
            command.ResidentId, buildingId!.Value, ct);

        if (eligibilityResult.IsFailure)
            return new ValidationError("EligibilityResult.notAllowed", eligibilityResult.Error!);
        
        var requestResult = MaintenanceRequest.Create(
            buildingId.Value,
            command.ResidentId,
            command.Title,
            command.Description);

        if (requestResult.IsFailure)
            return new ValidationError("CreateMaintenance.invalidForm", eligibilityResult.Error!);
        var request = requestResult.Value!;
        
        await _requestRepository.AddAsync(request, ct);
        await _requestRepository.SaveChangesAsync(ct);

   
        await _eventDispatcher.DispatchAsync(request.DomainEvents, ct);
        request.ClearDomainEvents();

        return new CreateMaintenanceRequestResult(request.Id);
    }
}