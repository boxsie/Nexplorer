using Nexplorer.Nexus.Enums;

namespace Nexplorer.Nexus.Tokens.Models
{
    public class TokenAccount : Token
    {
        public override string Type => "account";
        public override int TypeId => (int)TokenType.Account;
    }
}