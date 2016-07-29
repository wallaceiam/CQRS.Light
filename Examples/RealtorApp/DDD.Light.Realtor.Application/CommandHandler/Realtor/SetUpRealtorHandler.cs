using DDD.Light.Realtor.API.Command.Realtor;
using CQRS.Light.Core;
using System.Threading.Tasks;

namespace DDD.Light.Realtor.Application.CommandHandler.Realtor
{
    public class SetUpRealtorHandler : CommandHandler<SetUpRealtor>
    {
        public override Task HandleAsync(SetUpRealtor command)
        {
            new Domain.Model.Realtor.Realtor(command.RealtorId);
            return Task.FromResult<object>(null);
        }
    }
}
