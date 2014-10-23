using System.Linq;
using System.Xml.Linq;
using Machine.Specifications;
using Machine.Fakes;
using NPloy.Commands;
using NPloy.Support;

namespace NPloy.Console.Specs.Commands.InstallNodeCommandTests
{
    [Subject("Install Node")]
    public class When_Installing_Node : WithSubject<InstallNodeCommand>
    {
        private Establish context = () =>
            {
                var nPloyConfiguration = The<INPloyConfiguration>();
                nPloyConfiguration.WhenToldTo(c => c.FileExists("test.node")).Return(true);
                var nodeXml = CreateNodeFileForEnvironment("test.node", "test", "test1.role", "test2.role");
                nPloyConfiguration.WhenToldTo(c => c.GetNodeXml("test.node")).Return(nodeXml);

                The<ICommandFactory>().WhenToldTo(f => f.GetCommand<StartNodeCommand>()).Return(The<StartNodeCommand>());
            };

        Because of = () =>
            {
                Subject.AutoStart = true;
                Subject.Run(new[] { "test.node" });
            };

        private It should_start_node = () => The<StartNodeCommand>().WasToldTo(c => c.Run(Param<string[]>.Matches(s => s.Any(x => x == "test.node"))));

        private static XDocument CreateNodeFileForEnvironment(string node, string enviroment, params string[] roles)
        {
            var roleElements = roles.Select(role => new XElement("role", new XAttribute("name", role))).ToList();

            var element = new XElement("node",
                new XAttribute("environment", enviroment),
                new XElement("roles", roleElements)
                );

            var doc = new XDocument(element);
            return doc;
        }
    }
}
