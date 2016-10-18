
namespace PubnubApi
{
    public class MpnsFlipTileNotification
    {
        public string type { get; set; } = "flip";
        public int delay { get; set; } = 0;
        public string title { get; set; } = "";
        public int? count { get; set; } = 0;
        public string small_background_image { get; set; } = "";
        public string background_image { get; set; } = "";
        public string back_background_image { get; set; } = "";
        public string back_content { get; set; } = "";
        public string back_title { get; set; } = "";
        public string wide_background_image { get; set; } = "";
        public string wide_back_background_image { get; set; } = "";
        public string wide_back_content { get; set; } = "";
    }
}
