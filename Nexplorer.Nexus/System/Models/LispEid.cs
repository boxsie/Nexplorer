namespace Nexplorer.Nexus.System.Models
{
    public class LispEid
    {
        public long InstanceId { get; set; }
        public string Eid { get; set; }
        public Rloc[] Rlocs { get; set; }

        public class Rloc
        {
            public string Interface { get; set; }
            public string RlocName { get; set; }
            public string RlocRloc { get; set; }
        }
    }
}