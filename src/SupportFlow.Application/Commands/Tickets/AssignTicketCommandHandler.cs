using SupportFlow.Application.Interfaces;
using SupportFlow.Domain.Entities;


namespace SupportFlow.Application.Commands.Tickets;

public class AssignTicketCommandHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;

    public AssignTicketCommandHandler(
        ITicketRepository ticketRepository,
        IUserRepository userRepository)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
    }

    public async Task<Ticket> Handle(AssignTicketCommand command)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId);

        if (ticket is null)
            throw new KeyNotFoundException("Ticket not found.");
        var technician = await _userRepository.GetByIdAsync(command.TechnicianId);

        if (technician is null)
            throw new KeyNotFoundException("Technician not found.");
        ticket.AssignTo(technician);
        
        await _ticketRepository.UpdateAsync(ticket);

        return ticket;
    }
}