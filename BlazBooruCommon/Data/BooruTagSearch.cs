using System.ComponentModel.DataAnnotations;

namespace BlazBooruCommon.Data
{
    public class FormSubmission {
        [Required]
        public string TagsSearch { get; set; }

        public string[] Tags => TagsSearch.Split('+', ' ');

        public string URLString => string.Join('+', Tags);
    }
}