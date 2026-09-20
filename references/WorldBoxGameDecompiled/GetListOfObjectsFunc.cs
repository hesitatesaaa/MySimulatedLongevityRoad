using System.Collections.Generic;

public delegate IEnumerable<TMetaObject> GetListOfObjectsFunc<TListElement, TMetaObject, TData, TComponent>(TComponent pComponent) where TListElement : WindowListElementBase<TMetaObject, TData> where TMetaObject : CoreSystemObject<TData> where TData : BaseSystemData where TComponent : ComponentListBase<TListElement, TMetaObject, TData, TComponent>;
