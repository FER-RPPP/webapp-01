namespace RPPP_WebApp.Model
{
    public class OptionsResult : JTableAjaxResult
    {
        public OptionsResult(List<TextValue> options)
        {
            Options = options;
        }
        public List<TextValue> Options { get; set; }
    }
}
