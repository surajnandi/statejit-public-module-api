using FluentValidation;
using sjam.RabbitMQ.Models;

namespace sjam.RabbitMQ.Validators
{
    public class TestQueueConsumeDtoValidator : AbstractValidator<TestQueueConsumeDto>
    {
        public TestQueueConsumeDtoValidator()
        {

        }
    }
}
