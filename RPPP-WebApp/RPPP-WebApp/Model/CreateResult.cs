namespace RPPP_WebApp.Model
{
    public class CreateResult : JTableAjaxResult
    {
        public CreateResult(object record) : base()
        {
            Record = record;
        }
        public object Record { get; set; }
    }
}
