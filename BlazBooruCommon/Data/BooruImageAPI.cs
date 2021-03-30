using System;

namespace BlazBooruCommon.Data
{
    public class BooruImageAPI
    {
        public int ID { get; set; }
        public string Image { get; set; }
        public string MD5 { get; set; }
        public string Original_Name { get; set; }
        public string Site_Origin { get; set; }
        public string Score { get; set; }
        public BooruTagData[] Tags { get; set; }

        public static implicit operator BooruImageAPI(BooruImageData d) =>
            new BooruImageAPI() {
                ID = d.ID,
                Image = d.Image,
                MD5 = d.MD5,
                Original_Name = d.Original_Name,
                Site_Origin = d.Site_Origin,
                Score = d.Score
            };

        public static implicit operator BooruImageData(BooruImageAPI d) =>
            new BooruImageData() {
                ID = d.ID,
                Image = d.Image,
                MD5 = d.MD5,
                Original_Name = d.Original_Name,
                Site_Origin = d.Site_Origin,
                Score = d.Score
            };
    }
}
