using LiteDB;

namespace BlazBooruCommon.Data
{
    public class BooruTagData
    {
        public int Image { get; set; }
        public string Tag { get; set; }
        public string Type { get; set; }
        public int Refs { get; set; }
    }
}