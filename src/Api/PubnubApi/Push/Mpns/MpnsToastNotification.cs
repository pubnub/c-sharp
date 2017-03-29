
namespace PubnubApi
{
    public class MpnsToastNotification
    {
        public string type { get; set; }
        public string text1 { get; set; }
        public string text2 { get; set; }
        public string param { get; set; }

        public MpnsToastNotification()
        {
            type = "toast";
            text1 = "";
            text2 = "";
            param = "";
        }
    }
}
