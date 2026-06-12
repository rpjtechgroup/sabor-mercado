namespace SaborMercado.Web.Domain.Status;

/// <summary>Tipo de mutação que disparou a avaliação de status.</summary>
public enum CartMutation
{
    SessionStarted,
    ItemAdded,
    ItemAddedOcr,
    ItemUpdated,
    ItemRemoved,
    SessionFinished,
}
