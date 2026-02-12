namespace OrderService.Api.Features.Common.Exceptions;

public class ExistsException : Exception 
{
    public ExistsException(string name, string key) 
        : base($"Entity {name} with key {key} exists.")
    {
    }
}