using DDD.Light.Realtor.API.Command.Realtor;
using CQRS.Light.Core;
using System.Threading.Tasks;
using CQRS.Light.Contracts;

namespace DDD.Light.Realtor.Application.CommandHandler.Realtor
{
    public class SetUpRealtorHandler : CommandHandler<SetUpRealtor>
    {
        public SetUpRealtorHandler(ICommandBus commandBus)
            :base(commandBus)
        {

        }
        public override Task HandleAsync(SetUpRealtor command)
        {
            new Domain.Model.Realtor.Realtor(command.RealtorId);
            return Task.FromResult<object>(null);
        }
    }
}
