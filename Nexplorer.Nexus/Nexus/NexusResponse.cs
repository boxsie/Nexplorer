namespace Nexplorer.Nexus.Nexus
{
    public class NexusResponse<T>
    {
        public T Result { get; set; }
        public NexusError Error { get; set; }

        public void SetError(string message)
        {
            Error = new NexusError {Message = message};
        }
    }
}