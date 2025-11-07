using System.Collections.Generic;

using UnityEngine;

namespace UI.Components
{
    public class TransformHolder : MonoBehaviour
    {
        [SerializeField] private List<Transform> transforms;
        
        
        public List<Transform> Transforms => transforms;
    }
}