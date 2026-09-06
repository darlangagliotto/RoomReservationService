using RoomService.Application.UseCases.Common;
using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;

namespace RoomService.Application.UseCases.GetEquipments
{
    public class GetEquipmentsUseCase : IGetEquipmentsUseCase
    {
        private readonly IEquipmentRepository _equipmentRepository;

        public GetEquipmentsUseCase(IEquipmentRepository equipmentRepository)
        {
            _equipmentRepository = equipmentRepository;
        }

        public async Task<Result<List<EquipmentResponse>>> ExecuteAsync(GetEquipmentsRequest request)
        {
            EquipmentType? type = null;

            if (!string.IsNullOrWhiteSpace(request.Type))
            {
                if (!Enum.TryParse<EquipmentType>(request.Type, ignoreCase: true, out var parsed)
                    || !Enum.IsDefined(parsed))
                {
                    return Result<List<EquipmentResponse>>.Failure(
                        $"Tipo de equipamento desconhecido. Valores aceitos: {string.Join(", ", Enum.GetNames<EquipmentType>())}.");
                }

                type = parsed;
            }

            var allocations = await _equipmentRepository.GetAllAsync(type, request.Unassigned == true);

            // Vazio e resposta legitima, nao erro de negocio: divergencia
            // deliberada do ADR-011, igual a spec 003. Ver docs/specs/004.
            var responses = allocations
                .Select(a => new EquipmentResponse(
                    a.Equipment.Id,
                    a.Equipment.Type.ToString(),
                    a.Equipment.Placement.ToString(),
                    a.Equipment.Brand,
                    a.Equipment.SerialNumber,
                    a.Equipment.PurchaseDate,
                    a.RoomId
                ))
                .ToList();

            return Result<List<EquipmentResponse>>.Success(responses);
        }
    }
}
