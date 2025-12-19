namespace ReSys.Shop.Core.Common.Domain.Events;

public interface IDomainEventHandler<in TDomainEvent> : IEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent;