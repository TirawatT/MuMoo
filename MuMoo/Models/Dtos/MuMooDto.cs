using System.ComponentModel;

namespace MuMoo.Models.Dtos
{
    public class MuMooDto
    {
        private const string _conStrDefault = "data source=my_data_source;user id=my_id;password=my_password;Persist Security Info=true;";

        [DefaultValue("oracle")]
        public string database { get; set; }
        [DefaultValue(_conStrDefault)]
        public string connectionString { get; set; }
        [DefaultValue("title")]
        public string caseString { get; set; }

    }
}
