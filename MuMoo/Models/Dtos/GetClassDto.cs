using System.ComponentModel;

namespace MuMoo.Models.Dtos
{
    public class GetClassDto : MuMooDto
    {

        [DefaultValue("MyClass")]
        public string className { get; set; }
        [DefaultValue("")]
        public string sql { get; set; }
    }
}
