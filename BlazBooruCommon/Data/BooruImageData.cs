using LiteDB;

namespace BlazBooruCommon.Data
{
    public class BooruImageData
    {
        public int ID { get; set; }
        public string Image { get; set; }
        public string MD5 { get; set; }
        public string Original_Name { get; set; }
        public string Site_Origin { get; set; }
        public string Score { get; set; }
    }
}
