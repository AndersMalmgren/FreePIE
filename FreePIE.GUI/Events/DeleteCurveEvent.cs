using FreePIE.GUI.Views.Curves;

namespace FreePIE.GUI.Events
{
    public class DeleteCurveEvent
    {
        public CurveViewModel CurveViewModel { get; set; }
        public DeleteCurveEvent(CurveViewModel curve)
        {
            CurveViewModel = curve;
        }
    }
}
