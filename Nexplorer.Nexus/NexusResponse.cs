namespace Nexplorer.Nexus
{
    public interface INexusResponse
    {
        NexusError Error { get; set; }
        bool CanConnect { get; set; }
        bool IsNodeError { get; set; }
    }

    public class NexusResponse<T> : INexusResponse
    {
        public T Result { get; set; }
        public NexusError Error { get; set; }
        public bool CanConnect { get; set; }
        public bool IsNodeError { get; set; }

        public bool HasError => Result == null || Error != null || !CanConnect || IsNodeError;

        public NexusResponse()
        {
            CanConnect = true;
            IsNodeError = false;
        }

        public NexusResponse(T val, INexusResponse response)
        {
            Result = val;
            CanConnect = response.CanConnect;
            IsNodeError = response.IsNodeError;
            Error = response.Error;
        }
    }
}