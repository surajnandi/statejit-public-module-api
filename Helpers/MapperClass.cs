using AutoMapper;
using sjam.Dal.Entities;
using sjam.RabbitMQ.Models.RabbitMQModel;

namespace sjam.Helpers
{
    public class MapperClass : Profile
    {
        public MapperClass()
        {
            CreateMap<ConsumeLog, ConsumedLogModel>().ReverseMap();
            CreateMap<ConsumeFailedLog, ConsumedLogModel>().ReverseMap();
        }
    }
}
