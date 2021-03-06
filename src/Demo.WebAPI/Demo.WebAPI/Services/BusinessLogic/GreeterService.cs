using System.Threading.Tasks;
using Demo.gRPC.SPA.FileTransport;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Demo.WebAPI.Services.BusinessLogic {
    public class GreeterService : Greeter.GreeterBase {
        private readonly ILogger<GreeterService> logger;

        public GreeterService(ILogger<GreeterService> logger) {
            this.logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context) {
            logger.LogInformation("gRPC is WORK!!!");
            return Task.FromResult(new HelloReply {
                Message = "gRPC is WORK " + request.Name + "!!!"
            });
        }
    }
}