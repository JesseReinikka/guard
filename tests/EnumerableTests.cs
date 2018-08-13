﻿namespace Dawn.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public sealed class EnumerableTests : BaseTests
    {
        [Flags]
        public enum CollectionOptions
        {
            Null = 0,

            Empty = 1,

            NotEmpty = 2,

            HasCount = 4,

            HasContains = 8,

            HasNullElement = 16
        }

        [Theory(DisplayName = T + "Enumerable: Empty/NotEmpty")]
        [InlineData(CollectionOptions.Null, CollectionOptions.Null)]
        [InlineData(CollectionOptions.Empty, CollectionOptions.NotEmpty)]
        [InlineData(CollectionOptions.Empty | CollectionOptions.HasCount, CollectionOptions.NotEmpty | CollectionOptions.HasCount)]
        public void Empty(CollectionOptions emptyOptions, CollectionOptions nonEmptyOptions)
        {
            var empty = GetEnumerable<int>(emptyOptions);
            var emptyArg = Guard.Argument(() => empty).Empty();
            CheckAndReset(empty, countCalled: true, enumerationCount: 0, enumerated: true);

            var nonEmpty = GetEnumerable<int>(nonEmptyOptions);
            var nonEmptyArg = Guard.Argument(() => nonEmpty).NotEmpty();
            CheckAndReset(nonEmpty, countCalled: true, enumerationCount: 1);

            if (empty is null)
            {
                emptyArg.NotEmpty();
                nonEmptyArg.Empty();
                return;
            }

            ThrowsArgumentException(
                nonEmptyArg,
                arg => arg.Empty(),
                (arg, message) => arg.Empty(e =>
                {
                    Assert.Same(nonEmpty, e);
                    return message;
                }));

            CheckAndReset(nonEmpty, countCalled: true, enumerationCount: 2);

            ThrowsArgumentException(
                emptyArg,
                arg => arg.NotEmpty(),
                (arg, message) => arg.NotEmpty(e =>
                {
                    Assert.Same(empty, e);
                    return message;
                }));

            CheckAndReset(empty, countCalled: true, enumerationCount: 0, enumerated: true);
        }

        [Theory(DisplayName = T + "Enumerable: MinCount")]
        [InlineData(CollectionOptions.Null, 3, 3, 4)]
        [InlineData(CollectionOptions.Empty, 0, 0, 1)]
        [InlineData(CollectionOptions.Empty | CollectionOptions.HasCount, 0, 0, 1)]
        [InlineData(CollectionOptions.NotEmpty, 3, 3, 4)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasCount, 3, 3, 4)]
        [InlineData(CollectionOptions.NotEmpty, 3, 2, 5)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasCount, 3, 2, 5)]
        public void MinCount(CollectionOptions options, int count, int countOrLess, int moreThanCount)
        {
            var enumerable = GetEnumerable<int>(options, count);
            var enumerableArg = Guard.Argument(() => enumerable).MinCount(countOrLess);
            CheckAndReset(enumerable, countCalled: true, enumerationCount: countOrLess, enumerated: countOrLess != 0);

            if (enumerable is null)
            {
                enumerableArg.MinCount(moreThanCount);
                return;
            }

            ThrowsArgumentException(
                enumerableArg,
                arg => arg.MinCount(moreThanCount),
                (arg, message) => arg.MinCount(moreThanCount, (e, m) =>
                {
                    Assert.Same(enumerable, e);
                    Assert.Equal(moreThanCount, m);
                    return message;
                }));
        }

        [Theory(DisplayName = T + "Enumerable: MaxCount")]
        [InlineData(CollectionOptions.Null, 3, 3, 2)]
        [InlineData(CollectionOptions.Empty, 0, 0, -1)]
        [InlineData(CollectionOptions.Empty | CollectionOptions.HasCount, 0, 0, -1)]
        [InlineData(CollectionOptions.NotEmpty, 3, 3, 2)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasCount, 3, 3, 2)]
        [InlineData(CollectionOptions.NotEmpty, 3, 4, 1)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasCount, 3, 4, 1)]
        public void MaxCount(CollectionOptions options, int count, int countOrMore, int lessThanCount)
        {
            var enumerable = GetEnumerable<int>(options, count);
            var enumerableArg = Guard.Argument(() => enumerable).MaxCount(countOrMore);
            CheckAndReset(enumerable, countCalled: true, enumerationCount: count, enumerated: countOrMore + 1 != 0);

            if (enumerable is null)
            {
                enumerableArg.MinCount(lessThanCount);
                return;
            }

            ThrowsArgumentException(
                enumerableArg,
                arg => arg.MaxCount(lessThanCount),
                (arg, message) => arg.MaxCount(lessThanCount, (e, m) =>
                {
                    Assert.Same(enumerable, e);
                    Assert.Equal(lessThanCount, m);
                    return message;
                }));
        }

        [Theory(DisplayName = T + "Enumerable: CountInRange")]
        [InlineData(CollectionOptions.Null, 3, 2, 4)]
        [InlineData(CollectionOptions.Empty, 0, -1, 1)]
        [InlineData(CollectionOptions.Empty | CollectionOptions.HasCount, 0, -1, 1)]
        [InlineData(CollectionOptions.NotEmpty, 3, 2, 4)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasCount, 3, 2, 4)]
        [InlineData(CollectionOptions.NotEmpty, 3, 1, 5)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasCount, 3, 1, 5)]
        public void CountInRange(CollectionOptions options, int count, int lessThanCount, int moreThanCount)
        {
            var enumerable = GetEnumerable<int>(options, count);
            var enumerableArg = Guard.Argument(() => enumerable);

            enumerableArg.CountInRange(lessThanCount, count);
            CheckAndReset(enumerable, countCalled: true, enumerationCount: count, enumerated: count + 1 != 0);

            enumerableArg.CountInRange(count, count);
            CheckAndReset(enumerable, countCalled: true, enumerationCount: count, enumerated: count + 1 != 0);

            enumerableArg.CountInRange(count, moreThanCount);
            CheckAndReset(enumerable, countCalled: true, enumerationCount: count, enumerated: moreThanCount + 1 != 0);

            enumerableArg.CountInRange(lessThanCount, moreThanCount);
            CheckAndReset(enumerable, countCalled: true, enumerationCount: count, enumerated: moreThanCount + 1 != 0);

            if (enumerable is null)
            {
                for (var i = 0; i < 2; i++)
                {
                    var limit = i == 0 ? lessThanCount : moreThanCount;
                    enumerableArg.CountInRange(limit, limit);
                }

                return;
            }

            for (var i = 0; i < 2; i++)
            {
                var limit = i == 0 ? lessThanCount : moreThanCount;
                ThrowsArgumentException(
                    enumerableArg,
                    arg => arg.CountInRange(limit, limit),
                    (arg, message) => arg.CountInRange(limit, limit, (e, min, max) =>
                    {
                        Assert.Same(enumerable, e);
                        Assert.Equal(limit, min);
                        Assert.Equal(limit, max);
                        return message;
                    }));

                var enumerationCount = (i == 0 ? limit + 1 : count) * 2;
                CheckAndReset(enumerable, countCalled: true, enumerationCount: enumerationCount, enumerated: limit + 1 != 0);
            }
        }

        [Theory(DisplayName = T + "Enumerable: Contains/DoesNotContain")]
        [InlineData(CollectionOptions.Null, 3, 2, -1)]
        [InlineData(CollectionOptions.Empty, 0, null, 1)]
        [InlineData(CollectionOptions.Empty | CollectionOptions.HasContains, 0, null, 1)]
        [InlineData(CollectionOptions.NotEmpty, 3, 2, -1)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasContains, 3, 1, -1)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement, 3, null, -1)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement | CollectionOptions.HasContains, 3, null, -1)]
        public void Contains(CollectionOptions options, int count, int? contained, int? nonContained)
        {
            var enumerable = GetEnumerable<int?>(options, count);
            var enumerableArg = Guard.Argument(() => enumerable);

            var index = enumerable?.Items.TakeWhile(i => i != contained).Count() ?? RandomNumber;
            var comparer = EqualityComparer<int?>.Default;

            if (contained.HasValue)
            {
                enumerableArg.Contains(contained.Value);
                CheckAndReset(enumerable, containsCalled: true, enumerationCount: index + 1);

                enumerableArg.Contains(contained.Value, null);
                CheckAndReset(enumerable, containsCalled: true, enumerationCount: index + 1);

                enumerableArg.Contains(contained.Value, comparer);
                CheckAndReset(enumerable, containsCalled: false, enumerationCount: index + 1);
            }

            if (nonContained.HasValue)
            {
                enumerableArg.DoesNotContain(nonContained.Value);
                CheckAndReset(enumerable, containsCalled: true, enumerationCount: count, enumerated: true);

                enumerableArg.DoesNotContain(nonContained.Value, null);
                CheckAndReset(enumerable, containsCalled: true, enumerationCount: count, enumerated: true);

                enumerableArg.DoesNotContain(nonContained.Value, comparer);
                CheckAndReset(enumerable, containsCalled: false, enumerationCount: count, enumerated: true);
            }

            if (enumerable is null)
            {
                if (nonContained.HasValue)
                    enumerableArg
                        .Contains(nonContained.Value)
                        .Contains(nonContained.Value, null)
                        .Contains(nonContained.Value, comparer);

                if (contained.HasValue)
                    enumerableArg
                        .DoesNotContain(contained.Value)
                        .DoesNotContain(contained.Value, null)
                        .DoesNotContain(contained.Value, comparer);

                return;
            }

            if (nonContained.HasValue)
            {
                ThrowsArgumentException(
                    enumerableArg,
                    arg => arg.Contains(nonContained.Value),
                    (arg, message) => arg.Contains(nonContained.Value, (e, i) =>
                    {
                        Assert.Same(enumerable, e);
                        Assert.Equal(nonContained, i);
                        return message;
                    }));

                CheckAndReset(enumerable, containsCalled: true, enumerationCount: count * 2, enumerated: true);

                ThrowsArgumentException(
                    enumerableArg,
                    arg => arg.Contains(nonContained.Value, null),
                    (arg, message) => arg.Contains(nonContained.Value, null, (e, i) =>
                    {
                        Assert.Same(enumerable, e);
                        Assert.Equal(nonContained, i);
                        return message;
                    }));

                CheckAndReset(enumerable, containsCalled: true, enumerationCount: count * 2, enumerated: true);

                ThrowsArgumentException(
                    enumerableArg,
                    arg => arg.Contains(nonContained.Value, comparer),
                    (arg, message) => arg.Contains(nonContained.Value, comparer, (e, i) =>
                    {
                        Assert.Same(enumerable, e);
                        Assert.Equal(nonContained, i);
                        return message;
                    }));

                CheckAndReset(enumerable, containsCalled: false, enumerationCount: count * 2, enumerated: true);
            }

            if (contained.HasValue)
            {
                ThrowsArgumentException(
                    enumerableArg,
                    arg => arg.DoesNotContain(contained.Value),
                    (arg, message) => arg.DoesNotContain(contained.Value, (e, i) =>
                    {
                        Assert.Same(enumerable, e);
                        Assert.Equal(contained, i);
                        return message;
                    }));

                CheckAndReset(enumerable, containsCalled: true, enumerationCount: (index + 1) * 2);

                ThrowsArgumentException(
                    enumerableArg,
                    arg => arg.DoesNotContain(contained.Value, null),
                    (arg, message) => arg.DoesNotContain(contained.Value, null, (e, i) =>
                    {
                        Assert.Same(enumerable, e);
                        Assert.Equal(contained, i);
                        return message;
                    }));

                CheckAndReset(enumerable, containsCalled: true, enumerationCount: (index + 1) * 2);

                ThrowsArgumentException(
                    enumerableArg,
                    arg => arg.DoesNotContain(contained.Value, comparer),
                    (arg, message) => arg.DoesNotContain(contained.Value, comparer, (e, i) =>
                    {
                        Assert.Same(enumerable, e);
                        Assert.Equal(contained, i);
                        return message;
                    }));

                CheckAndReset(enumerable, containsCalled: false, enumerationCount: (index + 1) * 2);
            }
        }

        [Theory(DisplayName = T + "Enumerable of class: ContainsNull/DoesNotContainNull")]
        [InlineData(CollectionOptions.Null, CollectionOptions.Null)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement, CollectionOptions.Empty)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement | CollectionOptions.HasContains, CollectionOptions.Empty)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement, CollectionOptions.NotEmpty)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement | CollectionOptions.HasContains, CollectionOptions.NotEmpty)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement, CollectionOptions.NotEmpty | CollectionOptions.HasContains)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement | CollectionOptions.HasContains, CollectionOptions.NotEmpty | CollectionOptions.HasContains)]
        public void ContainsNullReference(
            CollectionOptions optionsWithNull, CollectionOptions optionsWithoutNull)
        {
            var withNullCount = 10;
            var enumerableWithNull = GetEnumerable<string>(optionsWithNull, withNullCount);
            var enumerableWithNullArg = Guard.Argument(() => enumerableWithNull).ContainsNull();
            var nullIndex = enumerableWithNull?.Items.TakeWhile(s => s != null).Count() ?? RandomNumber;
            CheckAndReset(enumerableWithNull, containsCalled: true, enumerationCount: nullIndex + 1);

            var withoutNullCount = optionsWithoutNull.HasFlag(CollectionOptions.Empty) ? 0 : withNullCount;
            var enumerableWithoutNull = GetEnumerable<string>(optionsWithoutNull, withoutNullCount);
            var enumerableWithoutNullArg = Guard.Argument(() => enumerableWithoutNull).DoesNotContainNull();
            CheckAndReset(enumerableWithoutNull, containsCalled: true, enumerationCount: withoutNullCount, enumerated: true);

            if (enumerableWithNull is null)
            {
                enumerableWithNullArg.DoesNotContainNull();
                enumerableWithoutNullArg.ContainsNull();
                return;
            }

            ThrowsArgumentException(
                enumerableWithoutNullArg,
                arg => arg.ContainsNull(),
                (arg, message) => arg.ContainsNull(e =>
                {
                    Assert.Same(enumerableWithoutNull, e);
                    return message;
                }));

            CheckAndReset(enumerableWithoutNull, containsCalled: true, enumerationCount: withoutNullCount * 2, enumerated: true);

            ThrowsArgumentException(
                enumerableWithNullArg,
                arg => arg.DoesNotContainNull(),
                (arg, message) => arg.DoesNotContainNull(e =>
                {
                    Assert.Same(enumerableWithNull, e);
                    return message;
                }));

            CheckAndReset(enumerableWithNull, containsCalled: true, enumerationCount: (nullIndex + 1) * 2);
        }

        [Theory(DisplayName = T + "Enumerable of struct: ContainsNull/DoesNotContainNull")]
        [InlineData(CollectionOptions.Null, CollectionOptions.Null)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement, CollectionOptions.Empty)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement | CollectionOptions.HasContains, CollectionOptions.Empty)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement, CollectionOptions.NotEmpty)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement | CollectionOptions.HasContains, CollectionOptions.NotEmpty)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement, CollectionOptions.NotEmpty | CollectionOptions.HasContains)]
        [InlineData(CollectionOptions.NotEmpty | CollectionOptions.HasNullElement | CollectionOptions.HasContains, CollectionOptions.NotEmpty | CollectionOptions.HasContains)]
        public void ContainsNullValue(
            CollectionOptions optionsWithNull, CollectionOptions optionsWithoutNull)
        {
            var withNullCount = 10;
            var enumerableWithNull = GetEnumerable<int?>(optionsWithNull, withNullCount);
            var enumerableWithNullArg = Guard.Argument(() => enumerableWithNull).ContainsNull();
            var nullIndex = enumerableWithNull?.Items.TakeWhile(s => s.HasValue).Count() ?? RandomNumber;
            CheckAndReset(enumerableWithNull, containsCalled: true, enumerationCount: nullIndex + 1);

            var withoutNullCount = optionsWithoutNull.HasFlag(CollectionOptions.Empty) ? 0 : withNullCount;
            var enumerableWithoutNull = GetEnumerable<int?>(optionsWithoutNull, withoutNullCount);
            var enumerableWithoutNullArg = Guard.Argument(() => enumerableWithoutNull).DoesNotContainNull();
            CheckAndReset(enumerableWithoutNull, containsCalled: true, enumerationCount: withoutNullCount, enumerated: true);

            if (enumerableWithNull is null)
            {
                enumerableWithNullArg.DoesNotContainNull();
                enumerableWithoutNullArg.ContainsNull();
                return;
            }

            ThrowsArgumentException(
                enumerableWithoutNullArg,
                arg => arg.ContainsNull(),
                (arg, message) => arg.ContainsNull(e =>
                {
                    Assert.Same(enumerableWithoutNull, e);
                    return message;
                }));

            CheckAndReset(enumerableWithoutNull, containsCalled: true, enumerationCount: withoutNullCount * 2, enumerated: true);

            ThrowsArgumentException(
                enumerableWithNullArg,
                arg => arg.DoesNotContainNull(),
                (arg, message) => arg.DoesNotContainNull(e =>
                {
                    Assert.Same(enumerableWithNull, e);
                    return message;
                }));

            CheckAndReset(enumerableWithNull, containsCalled: true, enumerationCount: (nullIndex + 1) * 2);
        }

        private static TestEnumerable<T> GetEnumerable<T>(CollectionOptions options, int maxCount = 10)
        {
            if (options == CollectionOptions.Null)
                return null;

            IEnumerable<T> items;
            if (options.HasFlag(CollectionOptions.Empty))
            {
                items = Array.Empty<T>();
            }
            else
            {
                var type = typeof(T);
                var range = Enumerable.Range(1, maxCount);
                if (type == typeof(int))
                {
                    items = range as IEnumerable<T>;
                }
                else if (type == typeof(int?))
                {
                    items = range.Select(i => i as int?) as IEnumerable<T>;
                }
                else if (type == typeof(char))
                {
                    items = range.Select(i => (char)i) as IEnumerable<T>;
                }
                else if (type == typeof(char?))
                {
                    items = range.Select(i => (char)i as char?) as IEnumerable<T>;
                }
                else if (type == typeof(string))
                {
                    items = range.Select(i => i.ToString()) as IEnumerable<T>;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            var list = items.ToList();
            if (options.HasFlag(CollectionOptions.HasNullElement))
            {
                list.RemoveAt(list.Count - 1);
                list.Insert(RandomUtils.Current.Next(list.Count), default);
            }

            var hasCount = options.HasFlag(CollectionOptions.HasCount);
            var hasContains = options.HasFlag(CollectionOptions.HasContains);

            if (hasCount && hasContains)
                return new TestEnumerableWithCountAndContains<T>(list);

            if (hasCount)
                return new TestEnumerableWithCount<T>(list);

            if (hasContains)
                return new TestEnumerableWithContains<T>(list);

            return new TestEnumerable<T>(list);
        }

        private static void CheckAndReset<T>(
            TestEnumerable<T> enumerable,
            bool? countCalled = null,
            bool? containsCalled = null,
            int? enumerationCount = null,
            bool? enumerated = null)
        {
            if (enumerable is null)
                return;

            var withCount = enumerable as IEnumerableWithCount<T>;
            if (withCount != null && countCalled.HasValue)
            {
                Assert.Equal(countCalled, withCount.CountCalled);
                Assert.Equal(!countCalled, enumerable.Enumerated);

                if (countCalled.Value)
                    Assert.Equal(0, enumerable.EnumerationCount);
            }

            var withContains = enumerable as IEnumerableWithContains<T>;
            if (withContains != null && containsCalled.HasValue)
            {
                Assert.Equal(containsCalled, withContains.ContainsCalled);
                Assert.Equal(!containsCalled, enumerable.Enumerated);

                if (containsCalled.Value)
                    Assert.Equal(0, enumerable.EnumerationCount);
            }

            if (withCount is null && withContains is null)
            {
                if (!enumerated.HasValue && enumerationCount.HasValue)
                    enumerated = enumerationCount > 0;

                if (enumerated.HasValue)
                    Assert.Equal(enumerated, enumerable.Enumerated);

                if (enumerationCount.HasValue)
                    Assert.Equal(enumerationCount, enumerable.EnumerationCount);
            }

            enumerable.Reset();
            Assert.False(enumerable.Enumerated);
            Assert.Equal(0, enumerable.EnumerationCount);

            if (withCount != null)
                Assert.False(withCount.CountCalled);

            if (withContains != null)
                Assert.False(withContains.ContainsCalled);
        }

        private interface IEnumerableWithCount<T> : IReadOnlyCollection<T>
        {
            bool CountCalled { get; }
        }

        private interface IEnumerableWithContains<T> : IEnumerable<T>
        {
            bool Contains(T item);

            bool ContainsCalled { get; }
        }

        private class TestEnumerable<T> : IEnumerable<T>
        {
            public TestEnumerable(IEnumerable<T> items) => this.Items = items;

            public IEnumerable<T> Items { get; }

            public bool Enumerated { get; private set; }

            public int EnumerationCount { get; private set; }

            public IEnumerator<T> GetEnumerator()
            {
                this.Enumerated = true;
                foreach (var item in this.Items)
                {
                    this.EnumerationCount++;
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            public virtual void Reset()
            {
                this.Enumerated = false;
                this.EnumerationCount = 0;
            }
        }

        private class TestEnumerableWithCount<T> : TestEnumerable<T>, IEnumerableWithCount<T>
        {
            private readonly int count;

            public TestEnumerableWithCount(IEnumerable<T> items)
                : base(items) => this.count = items.Count();

            public int Count
            {
                get
                {
                    this.CountCalled = true;
                    return this.count;
                }
            }

            public bool CountCalled { get; private set; }

            public override void Reset()
            {
                base.Reset();
                this.CountCalled = false;
            }
        }

        private class TestEnumerableWithContains<T> : TestEnumerable<T>, IEnumerableWithContains<T>
        {
            public TestEnumerableWithContains(IEnumerable<T> items)
                : base(items)
            {
            }

            public bool ContainsCalled { get; private set; }

            public bool Contains(T item)
            {
                this.ContainsCalled = true;
                return this.Items.Contains(item);
            }

            public override void Reset()
            {
                base.Reset();
                this.ContainsCalled = false;
            }
        }

        private class TestEnumerableWithCountAndContains<T>
            : TestEnumerableWithCount<T>, IEnumerableWithCount<T>, IEnumerableWithContains<T>
        {
            public TestEnumerableWithCountAndContains(IEnumerable<T> items)
                : base(items)
            {
            }

            public bool ContainsCalled { get; private set; }

            public bool Contains(T item)
            {
                this.ContainsCalled = true;
                return this.Items.Contains(item);
            }

            public override void Reset()
            {
                base.Reset();
                this.ContainsCalled = false;
            }
        }
    }
}
