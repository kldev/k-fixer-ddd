using Common.Toolkit.ResultPattern;

using K.Fixer.Application.Events;
using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.IAM.Repositories;
using K.Fixer.Domain.Maintenance.Repositories;

namespace K.Fixer.Application.Maintenance.AssignTechnician;

public sealed class AssignTechnicianHandler
{
    private readonly IMaintenanceRequestRepository _requestRepository;
    private readonly IUserRepository               _userRepository;
    private readonly IDomainEventDispatcher        _eventDispatcher;

    // ReSharper disable once ConvertToPrimaryConstructor
    public AssignTechnicianHandler(
        IMaintenanceRequestRepository requestRepository,
        IUserRepository userRepository,
        IDomainEventDispatcher eventDispatcher)
    {
        _requestRepository = requestRepository;
        _userRepository    = userRepository;
        _eventDispatcher   = eventDispatcher;
    }

    public async Task<Result<AssignTechnicianResult>> HandleAsync(
        AssignTechnicianCommand command,
        CancellationToken ct = default)
    {
        var request = await _requestRepository.GetByIdAsync(command.MaintenanceRequestId, ct);
        if (request is null)
            return new NotFoundError("AssignTechnician.notFound", "Maintenance request not found.");

        var buildings = await _requestRepository.GetBuildingsByIdsAsync(new[] { request.BuildingId }, ct);
        var building = buildings.FirstOrDefault();
        if (building is null || building.CompanyId != command.AdminCompanyId)
            return new NotFoundError("AssignTechnician.notFound", "Maintenance request not found.");

        var technician = await _userRepository.GetById(command.TechnicianId, ct);
        if (technician is null)
            return new NotFoundError("AssignTechnician.technicianNotFound", "Technician not found.");

        if (technician.Role != UserRole.Technician)
            return new ValidationError("AssignTechnician.notATechnician", "The specified user is not a technician.");

        if (technician.CompanyId != command.AdminCompanyId)
            return new ValidationError("AssignTechnician.wrongCompany", "The technician belongs to a different company.");

        var result = request.AssignTo(command.TechnicianId, command.AssignedBy);
        if (result.IsFailure)
            return new ValidationError("AssignTechnician.transitionFailed", result.Error!);

        await _requestRepository.SaveChangesAsync(ct);

        await _eventDispatcher.DispatchAsync(request.DomainEvents, ct);
        request.ClearDomainEvents();

        return new AssignTechnicianResult();
    }
}
