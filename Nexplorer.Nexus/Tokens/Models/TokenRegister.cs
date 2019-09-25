using Nexplorer.Nexus.Enums;

namespace Nexplorer.Nexus.Tokens.Models
{
    public class TokenRegister : Token
    {
        public override string Type => "token";
        public override int TypeId => (int)TokenType.Register;

        public double Supply { get; set; }
    }
}