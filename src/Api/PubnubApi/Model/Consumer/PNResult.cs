
namespace PubnubApi
{
    public class PNResult<T>
    {
        public T Result { get; internal set; }
        public PNStatus Status { get; internal set; }
    }
}
