
namespace PubnubApi
{
    public class MpnsIconicTileNotification
    {
        public string type { get; set; } = "iconic";
        public int delay { get; set; } = 0;
        public string title { get; set; } = "";
        public int? count { get; set; } = 0;
        public string icon_image { get; set; } = "";
        public string small_icon_image { get; set; } = "";
        public string background_color { get; set; } = "";
        public string wide_content_1 { get; set; } = "";
        public string wide_content_2 { get; set; } = "";
        public string wide_content_3 { get; set; } = "";
    }
}
