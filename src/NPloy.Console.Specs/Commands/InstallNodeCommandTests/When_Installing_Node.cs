using System.IO;
using System.Linq;
using System.Xml;
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
                var nodeXml = CreateNodeXml("test.node", "test", "test1.role", "test2.role");
                nPloyConfiguration.WhenToldTo(c => c.GetNodeXml("test.node")).Return(nodeXml);

                The<ICommandFactory>().WhenToldTo(f => f.GetCommand<StartNodeCommand>()).Return(The<StartNodeCommand>());
            };

        Because of = () =>
            {
                Subject.AutoStart = true;
                Subject.Run(new[] { "test.node" });
            };

        private It should_start_node = () => The<StartNodeCommand>().WasToldTo(c => c.Run(Param<string[]>.Matches(s => s.Any(x => x == "test.node"))));

        private static XmlDocument CreateNodeXml(string node, string enviroment, params string[] roles)
        {
            if (File.Exists(node))
                File.Delete(node);
            var content = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?><node environment=""{0}""><roles>", enviroment);
            foreach (var role in roles)
            {
                content += @"<role name=""" + role + @""" />";
            }
            content += @"</roles></node>";
            File.WriteAllText(node, content);
            var document = new XmlDocument();
            document.Load(node);
            return document;
        }
    }
}
