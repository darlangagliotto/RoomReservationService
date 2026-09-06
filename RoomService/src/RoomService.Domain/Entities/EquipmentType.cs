namespace RoomService.Domain.Entities
{
    /// <summary>
    /// Vocabulario controlado de tipos de equipamento.
    ///
    /// Texto livre nao mapeia para marcador na planta: "Monitor", "monitor" e
    /// "Tela" eram tres coisas distintas para o sistema. Ver docs/specs/004.
    /// </summary>
    public enum EquipmentType
    {
        Tv,
        Monitor,
        QuadroBranco,
        Projetor,
        ArCondicionado,
        Telefone,
        Notebook,
        Dock,
        Flipchart,
        Cadeira,
        Outro
    }
}
