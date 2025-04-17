/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
namespace Fx.Either
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class EitherUnitTests
    {
        [TestMethod]
        public void CreateLeft()
        {
            var value = "some value";
            var left = new Either<string, int>.Left(value);

            Assert.AreEqual(value, left.Value);
        }

        [TestMethod]
        public void CreateRight()
        {
            var value = 42;
            var right = new Either<string, int>.Right(value);

            Assert.AreEqual(value, right.Value);
        }

        [TestMethod]
        public void NullLeft()
        {
            new Either<string?, string?>.Left(null);
        }

        [TestMethod]
        public void NullRight()
        {
            new Either<string?, string?>.Right(null);
        }

        [TestMethod]
        public void VisitNullNode()
        {
            Assert.ThrowsException<ArgumentNullException>(() => MockVisitor.Instance.Visit(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                , default));
        }

        [TestMethod]
        public void LeftAccept()
        {
            Either<string, int> either = new Either<string, int>.Left("asdf");
            
            var result = MockVisitor.Instance.Visit(either, default);

            Assert.AreEqual('a', result);
        }

        [TestMethod]
        public void RightAccept()
        {
            Either<string, int> either = new Either<string, int>.Right(42);

            var result = MockVisitor.Instance.Visit(either, default);

            Assert.AreEqual('4', result);
        }

        private sealed class MockVisitor : Either<string, int>.Visitor<char, Nothing>
        {
            /// <summary>
            /// 
            /// </summary>
            private MockVisitor()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public static MockVisitor Instance { get; } = new MockVisitor();

            /// <inheritdoc/>
            protected sealed override char Accept(Either<string, int>.Left node, Nothing context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return node.Value[0];
            }
            
            /// <inheritdoc/>
            protected sealed override char Accept(Either<string, int>.Right node, Nothing context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return node.Value.ToString()[0];
            }
        }

        [TestMethod]
        public void VisitNullContext()
        {
            MockVisitNullContextVisitor.Instance.Visit(new Either<string, int>.Left("sadf"),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                );
        }

        private sealed class MockVisitNullContextVisitor : Either<string, int>.Visitor<char, object>
        {
            /// <summary>
            /// 
            /// </summary>
            private MockVisitNullContextVisitor()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public static MockVisitNullContextVisitor Instance { get; } = new MockVisitNullContextVisitor();

            /// <inheritdoc/>
            protected sealed override char Accept(Either<string, int>.Left node, object context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return node.Value[0];
            }

            /// <inheritdoc/>
            protected sealed override char Accept(Either<string, int>.Right node, object context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return node.Value.ToString()[0];
            }
        }

        [TestMethod]
        public void ApplyLeft()
        {
            Either<string, int> either = new Either<string, int>.Left("asdf");

            var result = either.Apply((left, context) => left[0], (right, context) => right.ToString()[0], new Nothing());

            Assert.AreEqual('a', result);
        }

        [TestMethod]
        public void ApplyRight()
        {
            Either<string, int> either = new Either<string, int>.Right(42);

            var result = either.Apply((left, context) => left[0], (right, context) => right.ToString()[0], new Nothing());

            Assert.AreEqual('4', result);
        }

        [TestMethod]
        public void ApplyLeftException()
        {
            Either<string, int> either = new Either<string, int>.Left("asdf");

            var invalidOperationException = new InvalidOperationException();
            var invalidCastException = new InvalidCastException();

            var leftMapException = Assert.ThrowsException<LeftMapException>(
                () => either.Apply<char, Nothing>(
                    (left, context) => throw invalidOperationException, 
                    (right, context) => throw invalidCastException, 
                    default));

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);
        }

        [TestMethod]
        public void ApplyRightException()
        {
            Either<string, int> either = new Either<string, int>.Right(42);

            var invalidOperationException = new InvalidOperationException();
            var invalidCastException = new InvalidCastException();

            var rightMapException = Assert.ThrowsException<RightMapException>(
                () => either.Apply<char, Nothing>(
                    (left, context) => throw invalidOperationException, 
                    (right, context) => throw invalidCastException, 
                    default));

            Assert.AreEqual(invalidCastException, rightMapException.InnerException);
        }

        [TestMethod]
        public void ApplyNullLeftMap()
        {
            Either<string, int> either = new Either<string, int>.Left("asdf");

            Assert.ThrowsException<ArgumentNullException>(() => either.Apply<Nothing, Nothing>(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ,
                (right, context) => default, default));
        }

        [TestMethod]
        public void ApplyNullRightMap()
        {
            Either<string, int> either = new Either<string, int>.Left("asdf");

            Assert.ThrowsException<ArgumentNullException>(() => either.Apply<Nothing, Nothing>((left, context) => default,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                ,
                default));
        }

        [TestMethod]
        public async Task VisitAsyncNullNode()
        {
            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await MockAsyncVisitor
                        .Instance
                        .VisitAsync(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            , default)
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task LeftAcceptAsync()
        {
            Either<string, int> either = new Either<string, int>.Left("asdf");

            var result = await MockAsyncVisitor.Instance.VisitAsync(either, default).ConfigureAwait(false);

            Assert.AreEqual('a', result);
        }

        [TestMethod]
        public async Task RightAcceptAsync()
        {
            Either<string, int> either = new Either<string, int>.Right(42);

            var result = await MockAsyncVisitor.Instance.VisitAsync(either, default).ConfigureAwait(false);

            Assert.AreEqual('4', result);
        }

        private sealed class MockAsyncVisitor : Either<string, int>.AsyncVisitor<char, Nothing>
        {
            /// <summary>
            /// 
            /// </summary>
            private MockAsyncVisitor()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public static MockAsyncVisitor Instance { get; } = new MockAsyncVisitor();

            /// <inheritdoc/>
            protected override async Task<char> AcceptAsync(Either<string, int>.Left node, Nothing context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return await Task.FromResult(node.Value[0]).ConfigureAwait(false);
            }

            /// <inheritdoc/>
            protected override async Task<char> AcceptAsync(Either<string, int>.Right node, Nothing context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return await Task.FromResult(node.Value.ToString()[0]).ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task VisitAsyncNullContext()
        {
            await MockVisitNullContextAsyncVisitor
                .Instance
                .VisitAsync(
                    new Either<string, int>.Left("sadf"),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                )
                .ConfigureAwait(false);
        }

        private sealed class MockVisitNullContextAsyncVisitor : Either<string, int>.AsyncVisitor<char, object>
        {
            /// <summary>
            /// 
            /// </summary>
            private MockVisitNullContextAsyncVisitor()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            public static MockVisitNullContextAsyncVisitor Instance { get; } = new MockVisitNullContextAsyncVisitor();

            /// <inheritdoc/>
            protected override async Task<char> AcceptAsync(Either<string, int>.Left node, object context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return await Task.FromResult(node.Value[0]).ConfigureAwait(false);
            }

            /// <inheritdoc/>
            protected override async Task<char> AcceptAsync(Either<string, int>.Right node, object context)
            {
                ArgumentNullException.ThrowIfNull(node);

                return await Task.FromResult(node.Value.ToString()[0]).ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task ApplyAsyncLeft()
        {
            Either<string, int> either = new Either<string, int>.Left("asdf");

            var result = await either
                .ApplyAsync(
                    async (left, context) => await Task.FromResult(left[0]).ConfigureAwait(false), 
                    async (right, context) => await Task.FromResult(right.ToString()[0]).ConfigureAwait(false),
                    new Nothing())
                .ConfigureAwait(false);

            Assert.AreEqual('a', result);
        }

        [TestMethod]
        public async Task ApplyAsyncRight()
        {
            Either<string, int> either = new Either<string, int>.Right(42);

            var result = await either
                .ApplyAsync(
                    async (left, context) => await Task.FromResult(left[0]).ConfigureAwait(false), 
                    async (right, context) => await Task.FromResult(right.ToString()[0]).ConfigureAwait(false), 
                    new Nothing())
                .ConfigureAwait(false);

            Assert.AreEqual('4', result);
        }

        [TestMethod]
        public async Task ApplyAsyncLeftException()
        {
            Either<string, int> either = new Either<string, int>.Left("asdf");

            var invalidOperationException = new InvalidOperationException();
            var invalidCastException = new InvalidCastException();

            var leftMapException = await Assert
                .ThrowsExceptionAsync<LeftMapException>(
                    async () => await either
                        .ApplyAsync<char, Nothing>(
                            (left, context) => throw invalidOperationException,
                            (right, context) => throw invalidCastException,
                            default)
                        .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidOperationException, leftMapException.InnerException);
        }

        [TestMethod]
        public async Task ApplyAsycRightException()
        {
            Either<string, int> either = new Either<string, int>.Right(42);

            var invalidOperationException = new InvalidOperationException();
            var invalidCastException = new InvalidCastException();

            var rightMapException = await Assert
                .ThrowsExceptionAsync<RightMapException>(
                    async () => await either
                        .ApplyAsync<char, Nothing>(
                            (left, context) => throw invalidOperationException,
                            (right, context) => throw invalidCastException,
                            default)
                        .ConfigureAwait(false))
                .ConfigureAwait(false);

            Assert.AreEqual(invalidCastException, rightMapException.InnerException);
        }

        [TestMethod]
        public async Task ApplyAsyncNullLeftMap()
        {
            Either<string, int> either = new Either<string, int>.Left("asdf");

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await either
                        .ApplyAsync<Nothing, Nothing>(
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            ,
                            async (right, context) => await Task.FromResult(new Nothing()).ConfigureAwait(false), 
                            default)
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ApplyAsyncNullRightMap()
        {
            Either<string, int> either = new Either<string, int>.Left("asdf");

            await Assert
                .ThrowsExceptionAsync<ArgumentNullException>(
                    async () => await either
                        .ApplyAsync<Nothing, Nothing>(
                            async (left, context) => await Task.FromResult(new Nothing()).ConfigureAwait(false),
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            null
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            ,
                            default)
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        [TestMethod]
        public void ImplicitLeft()
        {
            Either<string, int> either =
#pragma warning disable CS0618 // Type or member is obsolete
                "asdf"
#pragma warning restore CS0618 // Type or member is obsolete
                ;

            var result = MockVisitor.Instance.Visit(either, default);

            Assert.AreEqual('a', result);
        }

        [TestMethod]
        public void ImplicitRight()
        {
            Either<string, int> either =
#pragma warning disable CS0618 // Type or member is obsolete
                42
#pragma warning restore CS0618 // Type or member is obsolete
                ;

            var result = MockVisitor.Instance.Visit(either, default);

            Assert.AreEqual('4', result);
        }
    }
}
