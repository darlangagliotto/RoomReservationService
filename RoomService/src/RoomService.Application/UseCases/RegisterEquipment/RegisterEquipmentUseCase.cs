using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;

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
            var existingEquipment = await _equipmentRepository.GetBySerialNumberAsync(request.SerialNumber);

            if (existingEquipment is not null)
            {
                return Result<RegisterEquipmentResponse>
                    .Failure("Equipamento já cadastrado!");
            }

            Equipment equipment;
            try
            {
                equipment = CreateEquipment(request);
            }
            catch (DomainException ex)
            {
                return Result<RegisterEquipmentResponse>.Failure(ex.Message);
            }

            await _equipmentRepository.AddSync(equipment);

            return Result<RegisterEquipmentResponse>.Success(
                new RegisterEquipmentResponse(
                    equipment.Id,
                    equipment.Type,
                    equipment.Brand,
                    equipment.SerialNumber,
                    equipment.PurchaseDate
                )
            );            
        }

        private Equipment CreateEquipment(RegisterEquipmentRequest request) =>
            new Equipment(request.Type, request.Brand, request.SerialNumber, request.PurchaseDate);
    }
}