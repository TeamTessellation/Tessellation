using System;
using System.Collections.Generic;
using Machamy.Attributes;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// UI의 기본 클래스입니다.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public abstract class UIBase : MonoBehaviour
    {
        [SerializeField, HideInInspector] private Canvas canvas;
        [SerializeField, VisibleOnly] private int savedSortingLayerID;
        [SerializeField, VisibleOnly] private int savedSortingOrder;
        private readonly Stack<int> _sortingOrderStack = new Stack<int>();
        
        protected virtual void Reset()
        {
            canvas = GetComponent<Canvas>();
            savedSortingLayerID = canvas.sortingLayerID;
            savedSortingOrder = canvas.sortingOrder;
        }

        protected virtual void Awake()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }
            savedSortingLayerID = canvas.sortingLayerID;
            savedSortingOrder = canvas.sortingOrder;
        }

        public Canvas Canvas
        {
            get
            {
                if (canvas == null)
                {
                    canvas = GetComponent<Canvas>();
                }
                return canvas;
            }
        }
        
        public void SetSortingLayerID(int id)
        {
            Canvas.sortingLayerID = id;
        }
        public void RestoreSortingLayer()
        {
            Canvas.sortingLayerID = savedSortingLayerID;
        }
        
        public void SetSortingOrder(int order)
        {
            _sortingOrderStack.Push(canvas.sortingOrder);
            canvas.sortingOrder = order;
        }
        
        public void RestoreSortingOrder()
        {
            if (_sortingOrderStack.Count > 0)
            {
                canvas.sortingOrder = _sortingOrderStack.Pop();
            }
            else
            {
                canvas.sortingOrder = savedSortingOrder;
            }
        }

        
        public virtual void Show()
        {
        }

        public virtual void Hide()
        {
        }

        public void SetVisible(bool visible)
        {
            if (visible)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}