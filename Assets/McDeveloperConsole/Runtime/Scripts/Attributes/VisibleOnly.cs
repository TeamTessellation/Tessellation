using UnityEngine;

namespace Machamy.Attributes
{
    /// <summary>
    /// (eng) Attribute that restricts modification in the Inspector.<br/>
    /// (kor) Inspector에서 수정을 제한하는 Attribute.
    /// </summary>
    /// <seealso cref="VisibleOnlyDrawer"/>
    public class VisibleOnly : PropertyAttribute
    {
        public EditableIn EditableIn;
        public bool IgnoreParentEditable = false;
        /// <summary>
        /// (eng) Attribute that restricts modification in the Inspector.
        /// (kor) Inspector에서 수정을 제한하는 Attribute.
        /// </summary>
        /// <param name="editableIn"> (eng) Specifies when the field is editable. <br/>
        /// (kor) 필드가 수정 가능한 시점을 지정합니다. <br/>
        /// None: Never editable. <br/>
        /// EditMode: Editable only in Edit Mode. <br/>
        /// PlayMode: Editable only in Play Mode. <br/>
        /// </param>
        /// <param name="ignoreParentEditable">
        /// (eng) If the parent object is already non-editable, whether to ignore it.<br/>
        /// (kor) 상위 오브젝트에서 이미 수정 불가인경우, 무시할 것인지.
        /// true: Ignore and allow editing based on this attribute.<br/>
        /// false: Follow the parent object's editability.<br/>
        /// </param>
        public VisibleOnly(EditableIn editableIn = EditableIn.None, bool ignoreParentEditable = false)
        {
            this.EditableIn = editableIn;
            this.IgnoreParentEditable = ignoreParentEditable;
        }
    }
    public enum EditableIn
    {
        None,
        EditMode,
        PlayMode
    }
}
