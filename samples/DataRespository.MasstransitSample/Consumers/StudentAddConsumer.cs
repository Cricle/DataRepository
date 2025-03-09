using DataRepository.Masstransit.Models;
using MassTransit;

namespace DataRespository.MasstransitSample.Consumers
{
    public class StudentAddConsumer : IConsumer<AddedResult<Student>>, IConsumer<DeletedResult<Student>>, IConsumer<UpdatedResult<Student>>
    {
        private readonly ILogger<StudentAddConsumer> logger;

        public StudentAddConsumer(ILogger<StudentAddConsumer> logger)
        {
            this.logger = logger;
        }

        public Task Consume(ConsumeContext<AddedResult<Student>> context)
        {
            var data = context.Message.Datas[0];
            logger.LogInformation("The student {id} named {name} inserted", data.Id, data.Name);
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<UpdatedResult<Student>> context)
        {
            logger.LogInformation("The student {id} name was updated to {name} - result {result}", context.Message.Conditions[0], context.Message.Conditions[1], context.Message.Count);
            return Task.CompletedTask;

        }

        public Task Consume(ConsumeContext<DeletedResult<Student>> context)
        {
            logger.LogInformation("The student {id} was deleted - result {result}", context.Message.Conditions[0], context.Message.Count);
            return Task.CompletedTask;
        }
    }
}
