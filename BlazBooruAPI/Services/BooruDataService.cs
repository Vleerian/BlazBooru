using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Data.SQLite;

using BlazBooruCommon.Data;

namespace BlazBooruAPI.Services
{
    public class BooruDataService
    {
        private readonly SQLiteConnection Booru_Storage;
        private readonly SQLiteConnection File_Storage;

        public BooruDataService()
        {
            Booru_Storage = Extensions.SetupDatabase("Booru_Storage");
            File_Storage = Extensions.SetupDatabase("File_Storage");
        }

        ~BooruDataService()
        {
            Booru_Storage.Dispose();
            File_Storage.Dispose();
        }

        public async Task<int> AddPost(BooruImageAPI Post)
        {
            var cmd = Booru_Storage.CreateCommand("INSERT INTO Image_Data (Image, MD5, Original_Name, Site_Origin, Score) VALUES ($image, $md5, $filename, $origin, 0)");
            cmd.Parameters.AddWithValue("$image", Post.Image);
            cmd.Parameters.AddWithValue("$md5", Post.MD5);
            cmd.Parameters.AddWithValue("$filename", Post.Image);
            cmd.Parameters.AddWithValue("$origin", Post.Original_Name);
            await cmd.ExecuteNonQueryAsync();
            int Post_ID = (int)Booru_Storage.LastInsertRowId;
            Post.ID = Post_ID;

            var Tasks = Post.Tags.Select(T => InsertTag(T, Post));
            await Task.WhenAll(Tasks);

            return Post_ID;
        }

        public async Task<BooruTagData> GetTagDataAsync(string Tag)
        {
            var cmd = Booru_Storage.CreateCommand("SELECT * FROM Tags_Data WHERE Tag = $tag");
            cmd.Parameters.AddWithValue("$tag", Tag);
            var result = await cmd.SQLCastAsync<BooruTagData>();
            return result.FirstOrDefault();
        }

        private async Task InsertTag(BooruTagData tag, BooruImageData image)
        {
            var cmd = Booru_Storage.CreateCommand("INSERT INTO Tag_Refs (Image, Tag) VALUES ($image, $tag)");
            cmd.Parameters.AddWithValue("$image", image.ID);
            cmd.Parameters.AddWithValue("$tag", tag.Tag);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task InsertTags(IEnumerable<BooruTagData> tags, BooruImageData image)
        {
            var tasks = tags.Select(tag => InsertTag(tag, image));
            await Task.WhenAll(tasks);
        }

        private async Task RemoveTag(BooruTagData tag, BooruImageData image)
        {
            var cmd = Booru_Storage.CreateCommand("DELETE FROM Tag_Refs WHERE Tag = $tag AND Image = $image");
            cmd.Parameters.AddWithValue("$image", image.ID);
            cmd.Parameters.AddWithValue("$tag", tag.Tag);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task RemoveTags(IEnumerable<BooruTagData> tags, BooruImageData image)
        {
            var tasks = tags.Select(tag => RemoveTag(tag, image));
            await Task.WhenAll(tasks);
        }

        public async Task<BooruImageAPI> GetPostByID(string PostID)
        {
            var cmd = Booru_Storage.CreateCommand("SELECT * FROM Image_Data WHERE ID = $id");
            cmd.Parameters.AddWithValue("$id", PostID);
            var tmp = (await cmd.SQLCastAsync<BooruImageData>()).FirstOrDefault();
            if(tmp == null)
                return null;
                
            var result = (BooruImageAPI)tmp;
            cmd = Booru_Storage.CreateCommand("SELECT * FROM Tags_View WHERE Image = $id");
            cmd.Parameters.AddWithValue("$id", PostID);
            var Tags = await cmd.SQLCastAsync<BooruTagData>();
            result.Tags = Tags.ToArray();
            return result;
        }

        public async Task<BooruImageAPI[]> GetRecent(int Count = 42)
        {
            var cmd = Booru_Storage.CreateCommand("SELECT Image FROM Image_Data ORDER BY ID DESC LIMIT $count");
            cmd.Parameters.AddWithValue("$count", Count);
            var DataReader = await cmd.ExecuteReaderAsync();
            var ImgIDs = new List<string>();
            while(DataReader.Read())
            {
                ImgIDs.Add(DataReader.GetString(0));
            }

            var FetchTasks = ImgIDs.Select(I => GetPostByID(I));
            return (await Task.WhenAll(FetchTasks)).ToArray();
        }

        public async Task<BooruImageAPI[]> SearchTags(string[] tags, int count = 42)
        {
            var cmd = Booru_Storage.CreateCommand();
            var sql = "SELECT Image FROM Tag_Refs WHERE ";
            for (int i = 0; i < tags.Length; i++)
            {
                sql += $"Tag = $tag{i} AND ";
                cmd.Parameters.AddWithValue($"$tag{i}", tags[i]);
            }
            sql = sql.Substring(0, sql.Length - 4) + " GROUP BY Image;";
            cmd.CommandText = sql;

            var DataReader = await cmd.ExecuteReaderAsync();
            var ImgIDs = new List<int>();
            while(DataReader.Read())
            {
                ImgIDs.Add(DataReader.GetInt32(0));
            }

            var FetchTasks = ImgIDs.Select(I => GetPostByID(I.ToString()));
            return (await Task.WhenAll(FetchTasks)).ToArray();
        }

        public async Task<bool> RemoveImage(string PostID)
        {
            // We need to retrieve the image data so we know what to delete from file storage
            var PostData = await GetPostByID(PostID);
            if(PostData == null)
                return false;

            // Delete all tag references to the image
            var cmd = File_Storage.CreateCommand("DELETE FROM Tag_Refs WHERE Image = $ID");
            cmd.Parameters.AddWithValue("$ID", PostID);
            await cmd.ExecuteNonQueryAsync();

            // Delete the post
            cmd = Booru_Storage.CreateCommand("DELETE FROM Image_Data WHERE ID = $id");
            cmd.Parameters.AddWithValue("$ID", PostID);
            await cmd.ExecuteNonQueryAsync();

            // Delete the image
            cmd = File_Storage.CreateCommand("DELETE FROM Files WHERE ID = $id");
            cmd.Parameters.AddWithValue("$ID", PostData.Image);
            await cmd.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<BooruFileData> UploadFile(BooruUploadedFile file)
        {
            string Hash = "";
            using(MD5 md5 = MD5.Create())
            {
                var hashbyte = md5.ComputeHash(file.Data);
                hashbyte.ForEach(I => Hash += I.ToString("X2"));
            }

            var cmd = File_Storage.CreateCommand("INSERT INTO Files (Filename, Uploader, MD5, Content_Type, Data) VALUES ($Filename, $Uploader, $MD5, $Content_Type, $Data)",
            new KeyValuePair<string, object>[]{
                new("$Filename", file.Filename),
                new("$Uploader", "site"),
                new("$MD5", Hash),
                new("$Content_Type", file.Content_Type),
                new("$Data", file.Data)
            });

            await cmd.ExecuteNonQueryAsync();
            return new BooruFileData {
                ID = (int)File_Storage.LastInsertRowId,
                Filename = file.Filename,
                Uploader = "site",
                MD5 = Hash,
                Content_Type = file.Content_Type,
                Data = file.Data
            };
        }

        public async Task<BooruFileData> DownloadFile(string FileID, bool NoData = false)
        {
            string SQL;
            if(NoData)
                SQL = "SELECT ID, Filename, Uploader, Date_Uploaded, MD5, Content_Type FROM Files WHERE ID = $id";
            else
                SQL = "SELECT * FROM Files WHERE ID = $id";

            var cmd = File_Storage.CreateCommand(SQL);
            cmd.Parameters.AddWithValue("$id", FileID);
            var result = await cmd.SQLCastAsync<BooruFileData>();
            return result.FirstOrDefault();
        }

        public async Task<string> DownloadB64File(string FileID)
        {
            var file = await DownloadFile(FileID);
            return $"data:{file.Content_Type};base64,{Convert.ToBase64String(file.Data)}";
        }
    }
}