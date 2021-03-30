namespace BlazBooruCommon.Data
{
    public class BooruUploadedFile
    {
        public string MD5 { get; set; }
        public string Filename { get; set; }
        public string Content_Type { get; set; }
        public byte[] Data { get; set; }
    }
}