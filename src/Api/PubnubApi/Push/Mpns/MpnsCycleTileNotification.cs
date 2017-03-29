
namespace PubnubApi
{
    public class MpnsCycleTileNotification
    {
        public string type { get; set; } = "cycle";
        public int delay { get; set; } = 0;
        public string title { get; set; } = "";
        public int? count { get; set; } = 0;
        public string small_background_image { get; set; } = "";
        public string[] images { get; set; } = null;
    }
}
