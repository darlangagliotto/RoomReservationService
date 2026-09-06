namespace ReservationService.Application.UseCases.GetAvailability
{
    /// <summary>
    /// Estado derivado das reservas — nunca um campo da sala. Uma sala nao "e"
    /// ocupada; ela esta ocupada num intervalo. Ver docs/specs/003.
    /// </summary>
    public enum RoomAvailabilityStatus
    {
        /// Nenhuma reserva no intervalo nem no resto do dia.
        Disponivel,

        /// Livre no intervalo, mas com reserva mais tarde no mesmo dia — pode
        /// nao servir para uma reuniao longa.
        Reservada,

        /// Ha reserva sobrepondo o intervalo consultado.
        EmUso
    }
}
