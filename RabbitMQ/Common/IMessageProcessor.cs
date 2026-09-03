using FluentValidation.Results;
using RabbitMQ.Client;
using sjam.RabbitMQ.Models;

namespace sjam.RabbitMQ.Common
{
    public interface IMessageProcessor<T> where T : class
    {
        Task<ValidationResult> ValidateMessage(T message);
        Task ProcessMessage(T message, IReadOnlyBasicProperties readOnlyBasicProperties);
    }
}
