namespace MuMoo.Models.Dtos
{
    public class ParameterGuideDto
    {
        public List<ValueDesc> caseString { get; set; }
        public List<ValueDesc> database { get; set; }
        public List<ValueDesc> dotNet { get; set; }

        public ParameterGuideDto()
        {
            caseString = new List<ValueDesc>();
            database = new List<ValueDesc>();
            dotNet = new List<ValueDesc>();
        }
    }
    public class ValueDesc
    {
        public string value { get; set; }
        public string desc { get; set; }
    }
}
