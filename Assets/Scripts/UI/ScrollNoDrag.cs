using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ScrollNoDrag : ScrollRect
    {
        public override void OnBeginDrag(PointerEventData eventData) { }
        public override void OnDrag(PointerEventData eventData) { }
        public override void OnEndDrag(PointerEventData eventData) { }
    }
}
