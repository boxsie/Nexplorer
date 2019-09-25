namespace Nexplorer.Nexus.Nexus
{
    public class NexusNode
    {
        public INexusClient Client { get; private set; }
        public NexusNodeEndpoint Endpoint { get; private set; }

        public NexusNode(INexusClient nexusClient, NexusNodeEndpoint endpoint)
        {
            Client = nexusClient;
            Endpoint = endpoint;

            Client.ConfigureHttpClient(endpoint);
        }
    }
}