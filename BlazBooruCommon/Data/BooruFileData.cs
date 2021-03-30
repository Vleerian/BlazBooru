namespace BlazBooruCommon.Data
{
    public class BooruFileData
    {
        public int ID { get; set; }
        public string Filename { get; set; }
        public string Uploader { get; set; }
        public string Date_Uploaded { get; set; }
        public string MD5 { get; set; }
        public string Content_Type { get; set; }
        public byte[] Data { get; set; }
    }
}