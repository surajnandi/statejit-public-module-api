using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Rendering;
using RabbitMQ.Client;
using sjam.Helpers;
using sjam.RabbitMQ.Common;
using sjam.RabbitMQ.Enums;
using sjam.RabbitMQ.Models;

namespace sjam.RabbitMQ.Services
{
    public class TestQueueConsumeService : IMessageProcessor<TestQueueConsumeDto>
    {
        private readonly ILogger<TestQueueConsumeService> _logger;
        private readonly IValidator<TestQueueConsumeDto> _validator;
        private readonly IMapper _mapper;
        private readonly IMQueueProcessingService _mQueueProcessingService;
        public TestQueueConsumeService
        (
            ILogger<TestQueueConsumeService> logger,
            IValidator<TestQueueConsumeDto> validator,
            IMapper mapper,
            IMQueueProcessingService mQueueProcessingService
        )
        {
            _logger = logger;
            _validator = validator;
            _mapper = mapper;
            _mQueueProcessingService = mQueueProcessingService;
        }
        public async Task ProcessMessage(TestQueueConsumeDto message, IReadOnlyBasicProperties readOnlyBasicProperties)
        {
            string payloadJson = JsonHelper.ObjectToJson(message);
            try
            {
                // Persist consumed data
                //await _jitFTORepository.insertAgencyDdoMappingDetails(payloadJson);
                try
                {
                    // Publish ONLY AFTER successful consume
                    //await _mQueueProcessingService.ProcessQueueAsync(RabbitMqQueueName.EbillingJitResponseAgencyDdoMapping);
                }
                catch (Exception ex)
                {
                    //MqErrorFileLogger.SaveErrorLocally(
                    //    "FailedLog",
                    //    ex.ToString(),
                    //    RabbitMqQueueName.EbillingJitResponseAgencyDdoMapping,
                    //    payloadJson
                    //);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ValidationResult> ValidateMessage(TestQueueConsumeDto message)
        {
            return await _validator.ValidateAsync(message);
        }
    }
}
