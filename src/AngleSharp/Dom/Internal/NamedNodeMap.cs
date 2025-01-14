namespace AngleSharp.Dom
{
    using AngleSharp.Text;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// NamedNodeNap is a key/value pair of nodes that can be accessed by
    /// numeric or string index.
    /// </summary>
    sealed class NamedNodeMap : INamedNodeMap
    {
        #region Fields
        
        private readonly List<Attr> _items;
        private readonly WeakReference<Element> _owner;

        #endregion

        #region ctor

        /// <inheritdoc />
        public NamedNodeMap(Element owner)
        {
            _items = new List<Attr>();
            _owner = new WeakReference<Element>(owner);
        }

        #endregion

        #region Index

        /// <inheritdoc />
        public IAttr? this[String name] => GetNamedItem(name);

        /// <inheritdoc />
        public IAttr? this[Int32 index] => index >= 0 && index < _items.Count ? _items[index] : null;

        #endregion

        #region Properties

        /// <inheritdoc />
        public Int32 Length => _items.Count;

        public Element? Owner => _owner.TryGetTarget(out var element) ? element : null;

        #endregion

        #region Internal Methods

        internal void FastAddItem(Attr attr)
        {
            _items.Add(attr);
        }

        internal void RaiseChangedEvent(Attr attr, String? newValue, String? oldValue)
        {
            if (_owner.TryGetTarget(out var element))
            {
                element.AttributeChanged(attr.LocalName, attr.NamespaceUri, oldValue, newValue);
            }
        }

        internal IAttr? RemoveNamedItemOrDefault(String name, Boolean suppressMutationObservers)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (name.Is(_items[i].Name))
                {
                    var attr = _items[i];
                    _items.RemoveAt(i);
                    attr.Container = null;

                    if (!suppressMutationObservers)
                    {
                        RaiseChangedEvent(attr, null, attr.Value);
                    }

                    return attr;
                }
            }

            return null;
        }

        internal IAttr? RemoveNamedItemOrDefault(String name)
        {
            return RemoveNamedItemOrDefault(name, false);
        }

        internal IAttr? RemoveNamedItemOrDefault(String? namespaceUri, String localName, Boolean suppressMutationObservers)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (localName.Is(_items[i].LocalName) && namespaceUri.Is(_items[i].NamespaceUri))
                {
                    var attr = _items[i];
                    _items.RemoveAt(i);
                    attr.Container = null;

                    if (!suppressMutationObservers)
                    {
                        RaiseChangedEvent(attr, null, attr.Value);
                    }

                    return attr;
                }
            }

            return null;
        }

        internal IAttr? RemoveNamedItemOrDefault(String? namespaceUri, String localName)
        {
            return RemoveNamedItemOrDefault(namespaceUri, localName, false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IAttr? GetNamedItem(String name)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (name.Is(_items[i].Name))
                {
                    return _items[i];
                }
            }

            return null;
        }

        /// <inheritdoc />
        public IAttr? GetNamedItem(String? namespaceUri, String localName)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (localName.Is(_items[i].LocalName) && namespaceUri.Is(_items[i].NamespaceUri))
                {
                    return _items[i];
                }
            }

            return null;
        }

        /// <inheritdoc />
        public IAttr? SetNamedItem(IAttr item)
        {
            var proposed = Prepare(item);

            if (proposed != null)
            {
                var name = item.Name;

                for (var i = 0; i < _items.Count; i++)
                {
                    if (name.Is(_items[i].Name))
                    {
                        var attr = _items[i];
                        _items[i] = proposed;
                        RaiseChangedEvent(proposed, proposed.Value, attr.Value);
                        return attr;
                    }
                }

                _items.Add(proposed);
                RaiseChangedEvent(proposed, proposed.Value, null);
            }

            return null;
        }

        /// <inheritdoc />
        public IAttr? SetNamedItemWithNamespaceUri(IAttr item, Boolean suppressMutationObservers)
        {
            var proposed = Prepare(item);

            if (proposed != null)
            {
                var localName = item.LocalName;
                var namespaceUri = item.NamespaceUri;

                for (var i = 0; i < _items.Count; i++)
                {
                    if (localName.Is(_items[i].LocalName) && namespaceUri.Is(_items[i].NamespaceUri))
                    {
                        var attr = _items[i];
                        _items[i] = proposed;

                        if (!suppressMutationObservers)
                        {
                            RaiseChangedEvent(proposed, proposed.Value, attr.Value);
                        }

                        return attr;
                    }
                }

                _items.Add(proposed);

                if (!suppressMutationObservers)
                {
                    RaiseChangedEvent(proposed, proposed.Value, null);
                }
            }

            return null;
        }

        /// <inheritdoc />
        public IAttr? SetNamedItemWithNamespaceUri(IAttr item)
        {
            return SetNamedItemWithNamespaceUri(item, false);
        }

        /// <inheritdoc />
        public IAttr RemoveNamedItem(String name)
        {
            var result = RemoveNamedItemOrDefault(name);

            if (result is null)
            {
                throw new DomException(DomError.NotFound);
            }

            return result;
        }

        /// <inheritdoc />
        public IAttr RemoveNamedItem(String? namespaceUri, String localName)
        {
            var result = RemoveNamedItemOrDefault(namespaceUri, localName);

            if (result is null)
            {
                throw new DomException(DomError.NotFound);
            }

            return result;
        }

        /// <inheritdoc />
        public IEnumerator<IAttr> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        
        #endregion

        #region Helpers

        private Attr? Prepare(IAttr item)
        {
            var attr = item as Attr;

            if (attr != null)
            {
                if (Object.ReferenceEquals(attr.Container, this))
                {
                    return null;
                }

                if (attr.Container != null)
                {
                    throw new DomException(DomError.InUse);
                }

                attr.Container = this;
            }

            return attr;
        }

        #endregion
    }
}
