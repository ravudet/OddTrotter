namespace OddTrotter
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    using global::OddTrotter.TodoList;

    public sealed class OddTrotter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="todoList"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="todoList"/> is <see langword="null"/></exception>
        public OddTrotter(TodoListService todoList)
        {
            if (todoList == null)
            {
                throw new ArgumentNullException(nameof(todoList));
            }

            this.TodoList = todoList;
        }

        public TodoListService TodoList { get; }

        public static void Decrypt(byte[] key, byte[] iv)
        {
            using (var input = File.OpenRead(@"C:\backup"))
            {
                using (var aes = Aes.Create())
                {
                    using (var output = File.OpenWrite(@"C:\Users\ravud\Downloads\plaintext"))
                    {
                        using (var cryptoStream = new CryptoStream(input, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
                        {
                            cryptoStream.CopyTo(output);
                        }
                    }
                }
            }
        }
    }
}
