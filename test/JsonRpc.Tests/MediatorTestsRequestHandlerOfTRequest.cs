using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.JsonRpc.Server;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace JsonRpc.Tests
{
    public class MediatorTestsRequestHandlerOfTRequest : AutoTestBase
    {
        [Method("workspace/executeCommand")]
        public interface IExecuteCommandHandler : IJsonRpcRequestHandler<ExecuteCommandParams> { }

        public class ExecuteCommandParams : IRequest
        {
            public string Command { get; set; }
        }

        public MediatorTestsRequestHandlerOfTRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Services
                .AddJsonRpcMediatR(new[] { typeof(MediatorTestsNotificationHandler).Assembly })
                .AddSingleton<ISerializer>(new Serializer());
        }

        [Fact]
        public async Task ExecutesHandler()
        {
            var executeCommandHandler = Substitute.For<IExecuteCommandHandler>();
            var mediator = Substitute.For<IMediator>();

            var collection = new HandlerCollection { executeCommandHandler };
            AutoSubstitute.Provide(collection);
            var router = AutoSubstitute.Resolve<RequestRouter>();

            var id = Guid.NewGuid().ToString();
            var @params = new ExecuteCommandParams() { Command = "123" };
            var request = new Request(id, "workspace/executeCommand", JObject.Parse(JsonConvert.SerializeObject(@params)));

            var response = await router.RouteRequest(router.GetDescriptor(request), request);

            await executeCommandHandler.Received(1).Handle(Arg.Any<ExecuteCommandParams>(), Arg.Any<CancellationToken>());


        }
    }
}
