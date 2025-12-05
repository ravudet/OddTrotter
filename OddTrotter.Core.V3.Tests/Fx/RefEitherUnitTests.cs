namespace Fx
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Fx.Either;
    using Fx.Realizable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class RefEitherUnitTests
    {
        public required TestContext TestContext { get; set; }

        [TestMethod]
        public void ApplyRightMapException()
        {
            var either = new RefEither<Exception, string>("asdf");
            var rightMapException = Assert.That.ThrowsException(either).Commit<RightMapException>(
                either => either.TypeHolder.Apply(
                    left => left.ToString().Length,
                    right => int.Parse(right)));

            Assert.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void ApplyLeftMapException()
        {
            var either = new RefEither<string, Exception>("asdf");
            var leftMapException = Assert.That.ThrowsException(either).Commit<LeftMapException>(
                either => either.TypeHolder.Apply(
                    left => int.Parse(left),
                    right => right.ToString().Length));

            Assert.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectRightMapException()
        {
            var either = new RefEither<Exception, string>("asdf");
            var rightMapException = Assert.That.ThrowsException(either).Commit<RightMapException>(
                either => either.TypeHolder.Select(
                    left => left,
                    right => int.Parse(right)));

            Assert.IsInstanceOfType(rightMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public void SelectLeftMapException()
        {
            //// TODO use `Assert.That` everywhere

            var either = new RefEither<string, Exception>("asdf");
            var leftMapException = Assert.That.ThrowsException(either).Commit<LeftMapException>(
                either => either.TypeHolder.Select(
                    left => int.Parse(left),
                    right => right));

            Assert.IsInstanceOfType(leftMapException.InnerException, typeof(FormatException));
        }

        [TestMethod]
        public async Task TestMethod1Dot1()
        {
            var either = new RefEither<int, Exception>(42);
            var result = await TestMethod1Impl(either).ConfigureAwait(false);

            Assert.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod1Dot2()
        {
            var exception = new Exception("the message");
            var either = new RefEither<int, Exception>(exception);
            var result = await TestMethod1Impl(either).ConfigureAwait(false);

            Assert.AreEqual(exception.ToString(), result);
        }

        private static Realizable<string> TestMethod1Impl(RefEither<int, Exception> either)
        {
            return either.Apply<string, bool, Realizable<string>>(
                (int value, ref bool context) => ToString(value),
                (Exception exception, ref bool context) => ToString(exception),
                ref Context);
        }

        [TestMethod]
        public async Task TestMethod2Dot1()
        {
            var either = new RefEither<string, Exception>("42");
            var result = await TestMethod2Impl(either);

            Assert.AreEqual("42", result);
        }

        [TestMethod]
        public async Task TestMethod2Dot2()
        {
            var either = new RefEither<string, Exception>("not a number");
            var result = await TestMethod2Impl(either);

            Assert.IsTrue(result.Contains("FormatException"));
        }

        [TestMethod]
        public async Task TestMethod2Dot3()
        {
            var exception = new Exception("the message");
            var either = new RefEither<string, Exception>(exception);
            var result = await TestMethod2Impl(either);

            Assert.AreEqual(exception.ToString(), result);
        }

        private static Realizable<string> TestMethod2Impl(RefEither<string, Exception> either)
        {
            var parsed = either.SelectAsync<RefEither<string, Exception>, string, Exception, TaskWrapper<IEither<int, Exception>>, IEither<int, Exception>, TaskWrapper<Exception>, Exception>(
                value => Parse(value),
                error => new TaskWrapper<Exception>(Task.FromResult(error)));

            //// TODO use `typeholder` here (or some other way to address the type inference issue)
            return parsed.Apply<RefEither<IEither<int, Exception>, Exception>, IEither<int, Exception>, Exception, string>(
                actualParsing => actualParsing.Apply(
                    actuallyParsed => actuallyParsed.ToString(),
                    parseError => parseError.ToString())!,
                readError => readError.ToString()!);
        }


        public static TaskWrapper<IEither<int, Exception>> Parse(string value)
        {
            return new TaskWrapper<IEither<int, Exception>>(ParseImpl(value));
        }

        private static async Task<IEither<int, Exception>> ParseImpl(string value)
        {
            return await Task.FromResult(ParseInner(value)).ConfigureAwait(false);
        }

        private static IEither<int, Exception> ParseInner(string value)
        {
            try
            {
                return new Either<int, Exception>.Left(int.Parse(value));
            }
            catch (Exception exception)
            {
                return new Either<int, Exception>.Right(exception);
            }
        }

        public static Realizable<string> ToString(int value)
        {
            return new Realizable<string>(new TaskWrapper<string>(ToStringImpl(value)));
        }

        private static async Task<string> ToStringImpl(int value)
        {
            return await Task.FromResult(value.ToString()).ConfigureAwait(false);
        }

        public static Realizable<string> ToString(Exception exception)
        {
            return new Realizable<string>(new TaskWrapper<string>(ToStringImpl(exception)));
        }

        private static async Task<string> ToStringImpl(Exception exception)
        {
            return await Task.FromResult(exception.ToString()).ConfigureAwait(false);
        }

        private static bool Context = false; //// TODO any test code that leverages this (or the one in the other test class) should actually leverage some "production" extension instead

        [TestMethod]
        public void RefLeft()
        {
            var value = 42;
            var either = new RefEither<SomeRef, Exception>(new SomeRef(value));

            var result = either.TypeHolder.Apply(
                left => left.Value,
                right => right.ToString().Length);

            Assert.AreEqual(value, result);
        }

        private readonly ref struct SomeRef
        {
            public SomeRef(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [TestMethod]
        public void RefRight()
        {
            var value = 42;
            var either = new RefEither<Exception, SomeRef>(new SomeRef(value));

            var result = either.TypeHolder.Apply(
                left => left.ToString().Length,
                right => right.Value);

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public async Task AsyncRefLeft()
        {
            var value = "42";
            var either = await AsyncRefWork(value);

            Assert.IsTrue(either.TypeHolder.Decompose(out var result, out _));
            Assert.AreEqual(42, result.Value);
        }

        private static ITask<RefEither<SomeRef, Exception>> AsyncRefWork(string value)
        {
            return new RefTask<IEither<int, Exception>, RefEither<SomeRef, Exception>>(
                Parse(value),
                potentiallyParsed => potentiallyParsed
                    .Apply<IEither<int, Exception>, int, Exception, RefEither<SomeRef, Exception>>(
                        value => new RefEither<SomeRef, Exception>(new SomeRef(value)),
                        exception => new RefEither<SomeRef, Exception>(exception)));
        }

        private sealed class RefTask<TContext, TValue> : ITask<TValue>
            where TValue : allows ref struct
        {
            private readonly TaskWrapper<TContext> task;
            private readonly Func<TContext, TValue> operation;

            public RefTask(TaskWrapper<TContext> task, Func<TContext, TValue> operation)
            {
                //// TODO can this be `icontinuable` or `iawaitable` or something?
                this.task = task;
                this.operation = operation;
            }

            public IConfiguredAwaitable<TValue> ConfigureAwait(bool continueOnCapturedContext)
            {
                return new ConfiguredAwaitable(
                    new RefTask<TContext, TValue>.Awaiter(
                        this.task.ConfigureAwait(continueOnCapturedContext).GetAwaiter(),
                        this.operation));
            }

            private sealed class ConfiguredAwaitable : IConfiguredAwaitable<TValue>
            {
                private readonly Awaiter awaiter;

                public ConfiguredAwaitable(RefTask<TContext, TValue>.Awaiter awaiter)
                {
                    this.awaiter = awaiter;
                }

                public IAwaiter<TValue> GetAwaiter()
                {
                    return this.awaiter;
                }
            }

            public Realizable<TResult> ContinueWith<TResult>(Func<TValue, TResult> sourceContinuation, Func<Exception, TResult> exceptionContinuation, Func<OperationCanceledException, TResult> canceledContinuation) where TResult : allows ref struct
            {
                var self = this;
                return this.task.ContinueWith(context => sourceContinuation(self.operation(context)), exceptionContinuation, canceledContinuation);
            }

            public IAwaiter<TValue> GetAwaiter()
            {
                return new Awaiter(this.task.GetAwaiter(), this.operation);
            }

            private sealed class Awaiter : IAwaiter<TValue>
            {
                private readonly IAwaiter<TContext> taskAwaiter;
                private readonly Func<TContext, TValue> operation;

                public Awaiter(IAwaiter<TContext> taskAwaiter, Func<TContext, TValue> operation)
                {
                    this.taskAwaiter = taskAwaiter;
                    this.operation = operation;
                }

                public bool IsCompleted
                {
                    get
                    {
                        return this.taskAwaiter.IsCompleted;
                    }
                }

                public TValue GetResult()
                {
                    return this.operation(this.taskAwaiter.GetResult());
                }

                public void OnCompleted(Action continuation)
                {
                    this.taskAwaiter.OnCompleted(continuation);
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    this.taskAwaiter.UnsafeOnCompleted(continuation);
                }
            }
        }

        [TestMethod]
        public async Task ReadFromFile()
        {
            var workingDirectory = Path.Combine(TestContext.TestRunDirectory, TestContext.TestName);
            var filePath = Path.Combine(workingDirectory, "somedata.txt");
            await WriteToFile(filePath, "42").ConfigureAwait(false);

            var potentiallyParsed = await ParseFromFile(filePath);
            Assert.IsTrue(potentiallyParsed.TypeHolder.Decompose(out var result, out _));
            Assert.AreEqual(42, result);
        }

        private static RefTask<string, RefEither<int, Exception>> ParseFromFile(string filePath)
        {
            return new RefTask<string, RefEither<int, Exception>>(
                new TaskWrapper<string>(File.ReadAllTextAsync(filePath)),
                text => ParseToRef(text));
        }

        private static RefEither<int, Exception> ParseToRef(string text)
        {
            int value;
            try
            {
                value = int.Parse(text);
            }
            catch (Exception exception)
            {
                return new RefEither<int, Exception>(exception);
            }

            return new RefEither<int, Exception>(value);
        }

        private static async Task WriteToFile(string filePath, string contents)
        {
            var parentDirectory = Path.GetDirectoryName(filePath);
            if (parentDirectory == null)
            {
                throw new Exception("TODO");
            }

            while (true)
            {
                try
                {
                    await File.WriteAllTextAsync(filePath, contents).ConfigureAwait(false);
                    return;
                }
                catch (DirectoryNotFoundException)
                {
                    Directory.CreateDirectory(parentDirectory);
                }
            }
        }
    }
}
