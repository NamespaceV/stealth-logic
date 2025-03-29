using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Common.Scripts
{
    // Unity cant serialize List<List<>> :/
    // https://discussions.unity.com/t/serialize-nested-lists/47472/2
    [Serializable]
    public class ListWrapper<T>
    {
        [SerializeField] private List<T> _innerList = new List<T>();
 
        public T this[int key]
        {
            get
            {
                return _innerList[key];
            }
            set
            {
                _innerList[key] = value;
            }
        }

        public void Add(T element)
        {
            _innerList.Add(element);
        }
    }
}