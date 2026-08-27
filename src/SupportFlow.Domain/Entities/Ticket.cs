using SupportFlow.Domain.Enums;
namespace SupportFlow.Domain.Entities;


public class Ticket
{
    public Guid Id { get; private set; }
    public int TicketNumber { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public Priority Priority { get; private set; }
    public TicketStatus Status { get; private set; }
    public DateTime CreateAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }

    public Ticket(
        string title,
        string? description,
        Priority priority,
        Guid createdByUserId
    )
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be  empty", nameof(title));

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Priority = priority;
        CreatedByUserId = createdByUserId;
        Status = TicketStatus.Pending;
        CreateAt = DateTime.UtcNow;
    }
    

    public void Resolve()
    {
        if (Status != TicketStatus.InProgress)
            throw new InvalidOperationException("Only a ticket in progress can be resolved.");
        Status = TicketStatus.Resolved;
    }

    public void Close()
    {
        if (Status != TicketStatus.Resolved)
            throw new InvalidOperationException("Only resolved tickets can be closed. ");
        Status = TicketStatus.Closed;
    }

    public void Reopen()
    {
        if (Status != TicketStatus.Closed) 
            throw new InvalidOperationException("Only closed tickets can be reopened. ");
        Status = TicketStatus.Reopened;
    }

    public void AssignTo(User technician)
    {
        if (technician.Role != Role.Technician)
            throw new InvalidOperationException("Only technician can be  assigned to tickets.");
        
        if(Status != TicketStatus.Pending && Status != TicketStatus.Reopened)
                throw new InvalidOperationException("Only pending or reopened tickets can be assigned.");
        
        
        AssignedToUserId = technician.Id;
        Status = TicketStatus.InProgress;
    }
    
}