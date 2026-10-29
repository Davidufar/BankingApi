
using BankingApiCore.DTOs;
using BankingApiCore.Models;
using BankingApiCore.Persistance;
using BCrypt.Net;
using Mapster;

namespace BankingApiInfrastructure.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository clientRepository;

    public ClientService(IClientRepository clientRepository) => this.clientRepository = clientRepository;

    public async Task<bool> CreateAsync(Client client)
    {
        client.Password = BCrypt.Net.BCrypt.HashPassword(client.Password);
        bool result = await clientRepository.CreateAsync(client);
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        bool result = await clientRepository.DeleteAsync(id);
        return result;
    }

    public async Task<List<ClientDTO>?> GetAllAsync()
    {
        var clients = await clientRepository.GetAllAsync();
        List<ClientDTO> result = new List<ClientDTO>();
        foreach (Client client in clients)
        {
            result.Add(client.Adapt<ClientDTO>());
        }
        return result;
    }

    public async Task<ClientDTO?> GetByEmailAsync(string email)
    {
        Client? client = await clientRepository.GetByEmailAsync(email);
        return client.Adapt<ClientDTO>();
    }

    public async Task<ClientDTO?> GetByIdAsync(int id)
    {
        Client? client = await clientRepository.GetByIdAsync(id);
        return client.Adapt<ClientDTO>();
        
    }
    public async Task<bool> UpdateAsync(Client client)
    {
        bool condition = await clientRepository.UpdateAsync(client);
        return condition;
    }
    public async Task<List<ClientDTO>> SearchByNameAsync(string? searchTerm)
    {
        IEnumerable<Client> clients = await clientRepository.SearchByNameAsync(searchTerm);
        List<ClientDTO> clientDTOs = new List<ClientDTO>();
        foreach(Client c in clients)
        {
            clientDTOs.Add(c.Adapt<ClientDTO>());
        }
        return clientDTOs;
    }
    public async  Task<List<ClientDTO>> SearchBySSNAsync(string? searchTerm)
    {
        IEnumerable<Client> clients = await clientRepository.SearchBySSNAsync(searchTerm);
        List<ClientDTO> clientDTOs = new List<ClientDTO>();
        foreach(Client c in clients)
        {
            clientDTOs.Add(c.Adapt<ClientDTO>());
        }
        return clientDTOs;
    }
}
