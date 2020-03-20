
namespace PubnubApi
{
    public class PNResult<T>
    {
        public T Result { get; set; }
        public PNStatus Status { get; set; }
    }
}
