using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;


namespace SupportFlow.Application.Commands.Tickets;

public class CreateTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IUserRepository userRepository)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
    }
    
    public async Task<Ticket> Handle(CreateTicketCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.CreatedByUserId);
        if (user is null)
            throw new InvalidOperationException("The user who created the ticket does not exist.");
        var ticket = new Ticket(
            command.Title,
            command.Description,
            command.Priority,
            command.CreatedByUserId
        );
        await _ticketRepository.AddAsync(ticket);
        return ticket;

    }
}

