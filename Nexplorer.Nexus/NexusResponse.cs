namespace Nexplorer.Nexus
{
    public class NexusResponse<T>
    {
        public T Result { get; set; }
        public NexusError Error { get; set; }
        public bool CanConnect { get; set; }
        public bool IsNodeError { get; set; }

        public NexusResponse()
        {
            CanConnect = true;
            IsNodeError = false;
        }
    }
}