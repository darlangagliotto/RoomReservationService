namespace RoomService.Domain.Entities
{
    /// <summary>
    /// Ancora padrao por tipo. E conhecimento de dominio, nao de interface: um
    /// projetor ESTA no teto. O frontend recebe a ancora pronta e nao replica
    /// esta tabela. Ver docs/product/information-architecture.md.
    ///
    /// Derivada do tipo, nunca armazenada por equipamento — so a sobrescrita
    /// opcional em RoomEquipment.Placement e persistida.
    /// </summary>
    public static class EquipmentPlacements
    {
        public static EquipmentPlacement For(EquipmentType type) => type switch
        {
            EquipmentType.Tv or EquipmentType.Monitor or EquipmentType.QuadroBranco
                => EquipmentPlacement.Parede,

            EquipmentType.Projetor or EquipmentType.ArCondicionado
                => EquipmentPlacement.Teto,

            EquipmentType.Telefone or EquipmentType.Notebook or EquipmentType.Dock
                => EquipmentPlacement.Mesa,

            _ => EquipmentPlacement.Piso
        };
    }
}
