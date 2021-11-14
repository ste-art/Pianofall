using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class RaycastTarget : Graphic
    {
        public override void SetMaterialDirty(){ }
        public override void SetVerticesDirty(){ }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
