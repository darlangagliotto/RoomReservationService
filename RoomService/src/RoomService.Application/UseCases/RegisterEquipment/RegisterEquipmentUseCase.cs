using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using RoomService.Application.UseCases.Common;

namespace RoomService.Application.UseCases.RegisterEquipment
{
    public class RegisterEquipmentUseCase : IRegisterEquipmentUseCase
    {
        private readonly IEquipmentRepository _equipmentRepository;

        public RegisterEquipmentUseCase(
            IEquipmentRepository equipmentRepository)
        {
            _equipmentRepository = equipmentRepository;
        }

        public async Task<Result<RegisterEquipmentResponse>> ExecuteAsync(RegisterEquipmentRequest request)
        {
            if (!TryParseType(request.Type, out var type))
            {
                return Result<RegisterEquipmentResponse>.Failure(
                    $"Tipo de equipamento desconhecido. Valores aceitos: {string.Join(", ", Enum.GetNames<EquipmentType>())}.");
            }

            var existingEquipment = await _equipmentRepository.GetBySerialNumberAsync(request.SerialNumber);

            if (existingEquipment is not null)
            {
                return Result<RegisterEquipmentResponse>
                    .Failure("Este equipamento já está cadastrado.");
            }

            Equipment equipment;
            try
            {
                equipment = new Equipment(type, request.Brand, request.SerialNumber, request.PurchaseDate);
            }
            catch (DomainException ex)
            {
                return Result<RegisterEquipmentResponse>.Failure(ex.Message);
            }

            await _equipmentRepository.AddSync(equipment);

            return Result<RegisterEquipmentResponse>.Success(
                new RegisterEquipmentResponse(
                    new EquipmentResponse(
                        equipment.Id,
                        equipment.Type.ToString(),
                        equipment.Placement.ToString(),
                        equipment.Brand,
                        equipment.SerialNumber,
                        equipment.PurchaseDate,
                        // Recem cadastrado nunca esta alocado.
                        null
                    )
                )
            );
        }

        private static bool TryParseType(string? value, out EquipmentType type)
            => Enum.TryParse(value, ignoreCase: true, out type) && Enum.IsDefined(type);
    }
}
