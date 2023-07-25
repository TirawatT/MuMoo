using System.ComponentModel;

namespace MuMoo.Models.Dtos
{
    public class GetMappingDto : MuMooDto
    {

        [DefaultValue("core")]
        public string dotNet { get; set; }
        [DefaultValue("")]
        public string tableName { get; set; }
        [DefaultValue(true)]
        public bool mapTpye { get; set; }

    }
}
