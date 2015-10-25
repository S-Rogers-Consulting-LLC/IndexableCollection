using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Linq {

    public interface ConstrainedIndexableCollection<T> : IndexableCollection<T> { }

    public interface IndexableCollection<T> : IndexableReadOnlyCollection<T>, ICollection<T> { }

    public interface IndexableReadOnlyCollection<T> : IReadOnlyCollection<T> { }

    public static class IndexableCollectionExtension {
        #region To<...>
        public static Collection<T> ToCollection<T>(this IEnumerable<T> Enumerable) {
            var collection = new Collection<T>();
            foreach (var t in Enumerable)
                collection.Add(t);
            return collection;
        }

        public static ConstrainedIndexableCollection<T> ToIndexableCollection<T>(this Collection<T> Collection) { return new IndexableCollectionInternal<T>(Collection); }

        public static IndexableReadOnlyCollection<T> ToReadOnlyIndexableCollection<T>(this Collection<T> Collection) { return new IndexableCollectionInternal<T>(Collection); }

        public static ConstrainedIndexableCollection<T> ToIndexableCollection<T>(this IEnumerable<T> Enumerable) { return Enumerable.ToCollection().ToIndexableCollection(); }

        public static IndexableReadOnlyCollection<T> ToReadOnlyIndexableCollection<T>(this IEnumerable<T> Enumerable) { return Enumerable.ToIndexableCollection(); }
        #endregion

        #region Index(...) - Fluent Interface
        public static IndexableCollection<T> BuildPrimaryKeyIndex<T, TProperty>(this ConstrainedIndexableCollection<T> source, Expression<Func<T, TProperty>> propertyExpressions) {
            (source as IndexableCollectionInternal<T>).BuildPrimaryKeyIndex(propertyExpressions.ToPropertyName());
            return source;
        }

        public static IndexableCollection<T> BuildIndex<T, TProperty>(this IndexableCollection<T> source, Expression<Func<T, TProperty>> propertyExpressions) {
            (source as IndexableCollectionInternal<T>).BuildIndex(propertyExpressions.ToPropertyName());
            return source;
        }

        public static IndexableCollection<T> ReBuildIndex<T, TProperty>(this IndexableCollection<T> source, Expression<Func<T, TProperty>> propertyExpressions) {
            (source as IndexableCollectionInternal<T>).ReBuildIndex(propertyExpressions.ToPropertyName());
            return source;
        }

        public static IndexableCollection<T> ReBuildIndexes<T>(this IndexableCollection<T> source) {
            (source as IndexableCollectionInternal<T>).ReBuildIndexes();
            return source;
        }

        public static IndexableCollection<T> DropIndexes<T>(this IndexableCollection<T> source) {
            (source as IndexableCollectionInternal<T>).DropIndexes();
            return source;
        }

        public static IndexableCollection<T> EnableAutoInexing<T>(this IndexableCollection<T> source) {
            (source as IndexableCollectionInternal<T>).IsAutoIndexing = true;
            return source;
        }

        public static IndexableCollection<T> DisableAutoInexing<T>(this IndexableCollection<T> source) {
            (source as IndexableCollectionInternal<T>).IsAutoIndexing = false;
            return source;
        }
        #endregion

        #region Where<...> Implemented
        public static IEnumerable<TSource> Where<TSource>(this ConstrainedIndexableCollection<TSource> source, Expression<Func<TSource, bool>> Expression) {
            return (source as IndexableCollection<TSource>).Where(Expression);
        }

        public static IEnumerable<TSource> Where<TSource>(this IndexableReadOnlyCollection<TSource> source, Expression<Func<TSource, bool>> Expression) {
            return (source as IndexableCollection<TSource>).Where(Expression);
        }

        public static IEnumerable<TSource> Where<TSource>(this IndexableCollection<TSource> source, Expression<Func<TSource, bool>> predicate) {
            var indexableCollection = source as IndexableCollectionInternal<TSource>;

            int? hashRight = null;
            var binaryExpression = (BinaryExpression)predicate.Body;

            switch (predicate.Body.NodeType) {
                case ExpressionType.Or:
                case ExpressionType.OrElse: {
                        var leftResults = Where(indexableCollection, Expression.Lambda<Func<TSource, bool>>(binaryExpression.Left, predicate.Parameters));
                        var rightResults = Where(indexableCollection, Expression.Lambda<Func<TSource, bool>>(binaryExpression.Right, predicate.Parameters));
                        foreach (TSource t in leftResults.Union(rightResults))
                            yield return t;
                        yield break;
                    }

                case ExpressionType.And:
                case ExpressionType.AndAlso: {
                        var leftResults = Where(indexableCollection, Expression.Lambda<Func<TSource, bool>>(binaryExpression.Left, predicate.Parameters));
                        var rightResults = Where(indexableCollection, Expression.Lambda<Func<TSource, bool>>(binaryExpression.Right, predicate.Parameters));
                        foreach (TSource t in leftResults.Intersect(rightResults))
                            yield return t;
                        yield break;
                    }
                case ExpressionType.Equal: {
                        var leftSideExpression = binaryExpression.Left;
                        if (indexableCollection.IsAutoIndexing)
                            if (!indexableCollection.IsIndexed(leftSideExpression.ToPropertyName()))
                                indexableCollection.BuildIndex(leftSideExpression.ToPropertyName());

                        var rightSideExpression = binaryExpression.Right;

                        hashRight = GetHashRight(leftSideExpression, rightSideExpression);

                        var returnedMemberExpression = (MemberExpression)null;
                        if (hashRight.HasValue && HasIndexableProperty<TSource>(indexableCollection, leftSideExpression, out returnedMemberExpression)) {
                            var index = indexableCollection.Index(((MemberExpression)returnedMemberExpression).Member.Name);
                            if (index.ContainsKey(hashRight.Value))
                                foreach (TSource t in index[hashRight.Value].AsEnumerable<TSource>().Where<TSource>(predicate.Compile()))
                                    yield return t;
                            yield break;
                        }

                        foreach (TSource t in indexableCollection.AsEnumerable<TSource>().Where<TSource>(predicate.Compile()))
                            yield return t;
                        yield break;
                    }
                default: {
                        foreach (TSource t in indexableCollection.AsEnumerable<TSource>().Where<TSource>(predicate.Compile()))
                            yield return t;
                        yield break;
                    }
            }
        }
        #endregion

        #region Join<...> Implemented
        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IndexableReadOnlyCollection<TOuter> OuterIndexableCollection, IndexableReadOnlyCollection<TInner> InnerIndexableCollection, Expression<Func<TOuter, TKey>> OuterKeySelector, Expression<Func<TInner, TKey>> InnerKeySelector, Func<TOuter, TInner, TResult> ResultSelector) {
            return OuterIndexableCollection.Join<TOuter, TInner, TKey, TResult>(InnerIndexableCollection, OuterKeySelector, InnerKeySelector, ResultSelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IndexableReadOnlyCollection<TOuter> OuterIndexableCollection, IndexableReadOnlyCollection<TInner> InnerIndexableCollection, Expression<Func<TOuter, TKey>> OuterKeySelector, Expression<Func<TInner, TKey>> InnerKeySelector, Func<TOuter, TInner, TResult> ResultSelector, IEqualityComparer<TKey> EqualityComparer) {
            if (OuterIndexableCollection == null || InnerIndexableCollection == null || OuterKeySelector == null || InnerKeySelector == null || ResultSelector == null)
                throw new ArgumentNullException();

            var outerIndexableCollection = OuterIndexableCollection as IndexableCollectionInternal<TOuter>;
            var innerIndexableCollection = InnerIndexableCollection as IndexableCollectionInternal<TInner>;

            bool haveIndex = false;
            if (InnerKeySelector.NodeType == ExpressionType.Lambda && InnerKeySelector.Body.NodeType == ExpressionType.MemberAccess && OuterKeySelector.NodeType == ExpressionType.Lambda && OuterKeySelector.Body.NodeType == ExpressionType.MemberAccess) {
                var memberExpressionInner = (MemberExpression)InnerKeySelector.Body;
                var memberExpressionOuter = (MemberExpression)OuterKeySelector.Body;

                if (innerIndexableCollection.IsAutoIndexing)
                    if (!innerIndexableCollection.IsIndexed(memberExpressionInner.Member.Name))
                        innerIndexableCollection.BuildIndex(memberExpressionInner.Member.Name);

                if (outerIndexableCollection.IsAutoIndexing)
                    if (!outerIndexableCollection.IsIndexed(memberExpressionOuter.Member.Name))
                        outerIndexableCollection.BuildIndex(memberExpressionOuter.Member.Name);

                if (innerIndexableCollection.IsIndexed(memberExpressionInner.Member.Name))
                    if (outerIndexableCollection.IsIndexed(memberExpressionOuter.Member.Name)) {
                        haveIndex = true;
                        var innerIndex = innerIndexableCollection.Index(memberExpressionInner.Member.Name);
                        var outerIndex = outerIndexableCollection.Index(memberExpressionOuter.Member.Name);

                        foreach (var outerKey in outerIndex.Keys) {
                            var innerGroup = (ConstrainableSet<TInner>)null;
                            if (innerIndex.TryGetValue(outerKey, out innerGroup))  //do a join on the GROUPS based on key result                           
                                foreach (var tResult in outerIndex[outerKey].AsEnumerable<TOuter>().Join<TOuter, TInner, TKey, TResult>(innerGroup.AsEnumerable<TInner>(), OuterKeySelector.Compile(), InnerKeySelector.Compile(), ResultSelector, EqualityComparer))
                                    yield return tResult;
                        }
                    }
            }

            if (!haveIndex)
                foreach (var tResult in outerIndexableCollection.AsEnumerable<TOuter>().Join<TOuter, TInner, TKey, TResult>(innerIndexableCollection.AsEnumerable<TInner>(), OuterKeySelector.Compile(), InnerKeySelector.Compile(), ResultSelector, EqualityComparer))
                    yield return tResult;
        }
        #endregion

        #region Private Methods
        private static string ToPropertyName<T, TProperty>(this Expression<Func<T, TProperty>> propertyExpression) { return ((MemberExpression)(((LambdaExpression)(propertyExpression)).Body)).Member.Name; }

        private static String ToPropertyName(this Expression Expression) {
            foreach (var memberExpression in SelectSingleMemberExpressionIfExsists(Expression))
                return memberExpression.Member.Name;
            return string.Empty;
        }

        private static IEnumerable<MemberExpression> SelectSingleMemberExpressionIfExsists(Expression Expression) {
            var memberExpression = Expression as MemberExpression;
            if (Expression.NodeType == ExpressionType.Call) {
                var call = Expression as MethodCallExpression;
                if (call.Method.Name == "CompareString")
                    memberExpression = call.Arguments[0] as MemberExpression;
            }

            if (null == memberExpression)
                yield break;
            yield return memberExpression;
        }

        private static bool HasIndexableProperty<T>(IndexableCollectionInternal<T> IndexableCollection, Expression Expression, out MemberExpression MemberExpression) {
            MemberExpression = null;
            foreach (var memberExpression in SelectSingleMemberExpressionIfExsists(Expression)) {
                MemberExpression = memberExpression;
                return IndexableCollection.IsIndexed(((MemberExpression)MemberExpression).Member.Name);
            }
            return false;
        }

        private static int? GetHashRight(Expression LeftSideExpression, Expression RightSideExpression) {
            if (LeftSideExpression.NodeType == ExpressionType.Call) {
                var call = LeftSideExpression as System.Linq.Expressions.MethodCallExpression;
                if (call.Method.Name == "CompareString") {
                    var evalRightLambdaExpression = Expression.Lambda(call.Arguments[1], null);
                    //Compile it, invoke it, and get the resulting hash
                    return (evalRightLambdaExpression.Compile().DynamicInvoke(null).GetHashCode());
                }
            }

            //rightside is where we get our hash...
            switch (RightSideExpression.NodeType) {
                //shortcut constants, dont eval, will be faster
                case ExpressionType.Constant:
                    var constantExpression = (ConstantExpression)RightSideExpression;
                    return (constantExpression.Value.GetHashCode());

                //if not constant (which is provably terminal in a tree), convert back to Lambda and eval to get the hash.
                default:
                    //Lambdas can be created from expressions... yay
                    var evalRightLambdaExpression = Expression.Lambda(RightSideExpression, null);
                    //Compile that mutherf-ker, invoke it, and get the resulting hash
                    return (evalRightLambdaExpression.Compile().DynamicInvoke(null).GetHashCode());
            }
        }
        #endregion

        #region Private Classes
        [Serializable]
        [ComVisible(false)]
        [DebuggerDisplay("Count = {Count}")]
        private class IndexableCollectionInternal<T> : Collection<T>, ConstrainedIndexableCollection<T> {
            #region Members
            private Indexes<T> TheIndexesByPropertyName = new Indexes<T>();
            #endregion

            #region Constructors
            public IndexableCollectionInternal(Collection<T> Collection)
                : base(Collection) {
                this.IsAutoIndexing = false;
            }
            #endregion

            #region Public Properties
            public Boolean IsAutoIndexing { get; set; }
            #endregion

            #region Public Methods
            public bool IsIndexed(string PropertyName) {
                lock (TheIndexesByPropertyName) {
                    return TheIndexesByPropertyName.ContainsKey(PropertyName);
                }
            }

            public Index<T> Index(string PropertyName) {
                lock (TheIndexesByPropertyName) {
                    return TheIndexesByPropertyName[PropertyName];
                }
            }

            public bool BuildPrimaryKeyIndex(string PropertyName) {
                lock (TheIndexesByPropertyName) {
                    if (!TheIndexesByPropertyName.ContainsKey(PropertyName))
                        TheIndexesByPropertyName.Add(PropertyName, new Index<T>(PropertyName, ConstraintTypes.UniqueKey));

                    foreach (T t in this)
                        AddToIndex(PropertyName, t);

                    return TheIndexesByPropertyName.ContainsKey(PropertyName);
                }
            }

            public bool BuildIndex(string PropertyName) {
                lock (TheIndexesByPropertyName) {
                    if (!TheIndexesByPropertyName.ContainsKey(PropertyName))
                        TheIndexesByPropertyName.Add(PropertyName, new Index<T>(PropertyName, ConstraintTypes.None));

                    foreach (T t in this)
                        AddToIndex(PropertyName, t);

                    return TheIndexesByPropertyName.ContainsKey(PropertyName);
                }
            }

            public void ReBuildIndexes() {
                foreach (string propertyName in TheIndexesByPropertyName.Keys)
                    ReBuildIndex(propertyName);
            }

            public bool ReBuildIndex(string PropertyName) {
                lock (TheIndexesByPropertyName) {
                    if (!DropIndex(PropertyName))
                        return false;
                    return BuildIndex(PropertyName);
                }
            }

            public bool DropIndex(string PropertyName) {
                lock (TheIndexesByPropertyName) {
                    if (TheIndexesByPropertyName.ContainsKey(PropertyName))
                        TheIndexesByPropertyName.Remove(PropertyName);
                    return !TheIndexesByPropertyName.ContainsKey(PropertyName);
                }
            }

            public void DropIndexes() {
                lock (TheIndexesByPropertyName) {
                    TheIndexesByPropertyName = new Indexes<T>();
                }
            }


            public new void Clear() {
                lock (TheIndexesByPropertyName) {
                    DropIndexes();
                    base.Clear();
                }
            }

            public new bool Remove(T t) {
                lock (TheIndexesByPropertyName) {
                    foreach (var propertyName in TheIndexesByPropertyName.Keys)
                        RemoveFromIndex(propertyName, t);
                    return base.Remove(t);
                }
            }

            public new void Add(T newItem) {
                lock (TheIndexesByPropertyName) {
                    foreach (string propertyName in TheIndexesByPropertyName.Keys)
                        AddToIndex(propertyName, newItem);
                    base.Add(newItem);
                }
            }
            #endregion

            #region Private Methods
            private void RemoveFromIndex(string PropertyName, T t) {
                var propertyInfo = typeof(T).GetProperty(PropertyName);
                if (null == propertyInfo)
                    return;

                var hashCode = propertyInfo.GetValue(t, null).GetHashCode();
                var index = TheIndexesByPropertyName[PropertyName];
                if (index.ContainsKey(hashCode))
                    index[hashCode].Remove(t);
            }

            private void AddToIndex(string PropertyName, T t) {
                var propertyInfo = typeof(T).GetProperty(PropertyName);
                if (null == propertyInfo)
                    return;

                var index = TheIndexesByPropertyName[PropertyName];
                if (propertyInfo.GetValue(t, null) != null) {
                    int hashCode = propertyInfo.GetValue(t, null).GetHashCode();
                    if (index.ContainsKey(hashCode))
                        index[hashCode].Add(t);
                    else {

                        if (ConstraintTypes.UniqueKey == index.ConstraintType) {
                            var constrainableSet = (ConstrainableSet<T>)new UniqueKeyStorageSet<T>(PropertyName);
                            index.Add(hashCode, constrainableSet);
                        } else {
                            var constrainableSet = (ConstrainableSet<T>)new StorageSet<T>(PropertyName);
                            index.Add(hashCode, constrainableSet);
                        }
                        index[hashCode].Add(t);
                    }
                }
            }
            #endregion
        }

        [Serializable]
        [ComVisible(false)]
        [DebuggerDisplay("Count = {Count}")]
        private class Indexes<T> : Dictionary<String, Index<T>> { }

        [Serializable]
        [ComVisible(false)]
        [DebuggerDisplay("Count = {Count}")]
        private class Index<T> : SortedDictionary<Int32, ConstrainableSet<T>> {
            public String PropertyName { get; private set; }
            public PropertyInfo PropertyInfo { get; private set; }
            public ConstraintTypes ConstraintType { get; private set; }

            public Index(String PropertyName, ConstraintTypes ConstraintType) {
                this.PropertyName = PropertyName;
                this.PropertyInfo = typeof(T).GetProperty(this.PropertyName);
                this.ConstraintType = ConstraintType;
            }
        }

        [Serializable]
        [ComVisible(false)]
        private class UniqueKeyStorageSet<T> : ConstrainableSet<T> {
            private Dictionary<String, T> TheDictionary = new Dictionary<String, T>();

            public UniqueKeyStorageSet(String PropertyName) {
                this.PropertyName = PropertyName;
                this.PropertyInfo = typeof(T).GetProperty(this.PropertyName);
            }

            public string PropertyName { get; private set; }
            public PropertyInfo PropertyInfo { get; private set; }
            public ConstraintTypes ConstraintType { get { return ConstraintTypes.UniqueKey; } }

            public IEnumerator<T> GetEnumerator() { return TheDictionary.Values.GetEnumerator(); }
            IEnumerator IEnumerable.GetEnumerator() { return TheDictionary.Values.GetEnumerator(); }

            public void Add(T t) {
                var tValueString = PropertyInfo.GetValue(t, null).ToString();
                if (TheDictionary.ContainsKey(tValueString))
                    throw new DuplicateKeyException(t, "Primary Key '" + tValueString + "' for Property Name '" + PropertyName + "' already Exists");

                TheDictionary.Add(tValueString.ToString(), t);
            }

            public bool Remove(T t) {
                var tValue = PropertyInfo.GetValue(t, null);
                return TheDictionary.Remove(tValue.ToString());
            }
        }

        [Serializable]
        [ComVisible(false)]
        private class StorageSet<T> : ConstrainableSet<T> {
            private Collection<T> TheCollection = new Collection<T>();

            public StorageSet(String PropertyName) {
                this.PropertyName = PropertyName;
                this.PropertyInfo = typeof(T).GetProperty(this.PropertyName);
            }

            public string PropertyName { get; private set; }
            public PropertyInfo PropertyInfo { get; private set; }
            public ConstraintTypes ConstraintType { get { return ConstraintTypes.None; } }

            public IEnumerator<T> GetEnumerator() { return TheCollection.GetEnumerator(); }
            IEnumerator IEnumerable.GetEnumerator() { return TheCollection.GetEnumerator(); }

            public void Add(T t) { TheCollection.Add(t); }
            public bool Remove(T t) { return TheCollection.Remove(t); }
        }

        public enum ConstraintTypes { None, UniqueKey }

        public interface ConstrainableSet<T> : IEnumerable<T> {
            String PropertyName { get; }
            PropertyInfo PropertyInfo { get; }
            ConstraintTypes ConstraintType { get; }

            void Add(T t);
            bool Remove(T t);
        }
        #endregion
    }
}