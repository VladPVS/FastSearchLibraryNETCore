using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
    /// <summary>
    /// Represents a class for fast text file search by different content criteria.
    /// </summary>
    public class FileTextSearcher
    {
        private readonly FileTextSearcherBase searcher;

        private readonly CancellationTokenSource cancellationTokenSource
            = new CancellationTokenSource();


        /// <summary>
        /// Event fires when next portion of files is found. Event handlers are not thread safe. 
        /// </summary>
        public event EventHandler<FileEventArgs> FilesFound
        {
            add
            {
                searcher.FilesFound += value;
            }

            remove
            {
                searcher.FilesFound -= value;
            }
        }


        /// <summary>
        /// Event fires when search process is completed or canceled.
        /// </summary>
        public event EventHandler<SearchCompletedEventArgs> SearchCompleted
        {
            add
            {
                searcher.SearchCompleted += value;
            }

            remove
            {
                searcher.SearchCompleted -= value;
            }
        }


        /// <summary>
        /// Initializes a new instance of FileTextSearcher class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="fileMatch">The delegate that determines algorithm of file selection.</param>
        /// <param name="textMatch">The delegate that determines algorithm of file selection among already found files.</param>
        /// <param name="encoding">Encoding of text files</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileTextSearcher(IEnumerable<string> folders,
            Func<FileInfo, bool> fileMatch,
            Func<string, bool> textMatch,
            Encoding encoding)
        {
            CheckFolders(folders);
            CheckDelegate(fileMatch);

            CheckDelegate(textMatch);
            CheckEncoding(encoding);

            var fileSearcherMultiple =
                new FileSearcherMultiple(folders.ToList(), fileMatch, cancellationTokenSource,
                    ExecuteHandlers.InCurrentTask, suppressOperationCanceledException: false);

            searcher = new FileTextSearcherDelegate(textMatch, fileSearcherMultiple, encoding, cancellationTokenSource.Token);
        }


        /// <summary>
        /// Initializes a new instance of FileTextSearcher class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="fileMatch">The delegate that determines algorithm of file selection.</param>
        /// <param name="textMatch">The delegate that determines algorithm of file selection among already found files.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileTextSearcher(IEnumerable<string> folders,
            Func<FileInfo, bool> fileMatch,
            Func<string, bool> textMatch)
            : this(folders, fileMatch, textMatch, Encoding.Default)
        {
        }


        /// <summary>
        /// Initializes a new instance of FileTextSearcher class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="fileMatch">The delegate that determines algorithm of file selection.</param>
        /// <param name="keywords">The list of searched keywords in text files</param>
        /// <param name="encoding">Encoding of text files</param>
        /// <param name="comparsion">Comparsion options of the searched keywords</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileTextSearcher(IEnumerable<string> folders,
            Func<FileInfo, bool> fileMatch,
            IEnumerable<string> keywords,
            Encoding encoding,
            StringComparison comparsion = StringComparison.Ordinal)
        {
            CheckFolders(folders);
            CheckDelegate(fileMatch);

            CheckKeywords(keywords);
            CheckEncoding(encoding);

            var fileSearcherMultiple =
                new FileSearcherMultiple(folders.ToList(), fileMatch, cancellationTokenSource,
                ExecuteHandlers.InCurrentTask, suppressOperationCanceledException: false);

            searcher = new FileTextSearcherKeywords(keywords, comparsion, fileSearcherMultiple, encoding, cancellationTokenSource.Token);
        }


        /// <summary>
        /// Initializes a new instance of FileTextSearcher class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="fileMatch">The delegate that determines algorithm of file selection.</param>
        /// <param name="keywords">The list of searched keywords in text files</param>
        /// <param name="comparsion">Comparsion options of the searched keywords</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileTextSearcher(IEnumerable<string> folders,
            Func<FileInfo, bool> fileMatch,
            IEnumerable<string> keywords,
            StringComparison comparsion = StringComparison.Ordinal)
            : this(folders, fileMatch, keywords, Encoding.Default, comparsion)
        {
        }


        /// <summary>
        /// Initializes a new instance of FileTextSearcher class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="textMatch">The delegate that determines algorithm of file selection among already found files.</param>
        /// <param name="encoding">Encoding of text files</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileTextSearcher(IEnumerable<string> folders,
            string pattern,
            Func<string, bool> textMatch,
            Encoding encoding)
        {
            CheckFolders(folders);
            CheckPattern(pattern);

            CheckDelegate(textMatch);
            CheckEncoding(encoding);

            var fileSearcherMultiple =
                new FileSearcherMultiple(folders.ToList(), pattern, cancellationTokenSource,
                    ExecuteHandlers.InCurrentTask, suppressOperationCanceledException: false);

            searcher = new FileTextSearcherDelegate(textMatch, fileSearcherMultiple, encoding, cancellationTokenSource.Token);
        }


        /// <summary>
        /// Initializes a new instance of FileTextSearcher class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="textMatch">The delegate that determines algorithm of file selection among already found files.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileTextSearcher(IEnumerable<string> folders,
            string pattern,
            Func<string, bool> textMatch)
            : this(folders.ToList(), pattern, textMatch, Encoding.Default)
        {
        }


        /// <summary>
        /// Initializes a new instance of FileTextSearcher class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="keywords">The list of searched keywords in text files</param>
        /// <param name="encoding">Encoding of text files</param>
        /// <param name="comparsion">Comparsion options of the searched keywords</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileTextSearcher(IEnumerable<string> folders,
            string pattern,
            IEnumerable<string> keywords,
            Encoding encoding,
            StringComparison comparsion = StringComparison.Ordinal)
        {
            CheckFolders(folders);
            CheckPattern(pattern);

            CheckKeywords(keywords);
            CheckEncoding(encoding);

            var fileSearcherMultiple =
                new FileSearcherMultiple(folders.ToList(), pattern, cancellationTokenSource,
                    ExecuteHandlers.InCurrentTask, suppressOperationCanceledException: false);

            searcher = new FileTextSearcherKeywords(keywords, comparsion, fileSearcherMultiple, encoding, cancellationTokenSource.Token);
        }


        /// <summary>
        /// Initializes a new instance of FileTextSearcher class.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="keywords">The list of searched keywords in text files</param>
        /// <param name="comparsion">Comparsion options of the searched keywords</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileTextSearcher(IEnumerable<string> folders,
            string pattern,
            IEnumerable<string> keywords,
            StringComparison comparsion = StringComparison.Ordinal)
            : this(folders.ToList(), pattern, keywords, Encoding.Default, comparsion)
        {
        }


        /// <summary>
        /// Starts a file search operation with realtime reporting using several threads in thread pool as an asynchronous operation.
        /// </summary>
        public Task SearchAsync()
        {
            return searcher.SearchAsync();
        }


        /// <summary>
        /// Stops a file search operation.
        /// </summary>
        public void StopSearch()
        {
            cancellationTokenSource.Cancel();
        }


        #region Static methods

        /// <summary>
        /// Returns a list of files that are contained in specified directories and all their subdirectories and match specified filtering delegates.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="fileMatch">The delegate that determines algorithm of file selection.</param>
        /// <param name="textMatch">The delegate that determines algorithm of file selection among already found files.</param>
        /// <param name="encoding">Encoding of text files</param>
        /// <returns>List of found files.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Task<List<FileInfo>> SearchFilesByTextAsync(IEnumerable<string> folders,
            Func<FileInfo, bool> fileMatch,
            Func<string, bool> textMatch,
            Encoding encoding)
        {
            CheckFolders(folders);
            CheckDelegate(fileMatch);

            CheckDelegate(textMatch);
            CheckEncoding(encoding);

            return Task.Run(async () =>
            {
                var cts = new CancellationTokenSource();
                var files = new ConcurrentBag<FileInfo>();

                var fileSearcherMultiple =
                    new FileSearcherMultiple(folders.ToList(), fileMatch, cts);

                var searcher = new FileTextSearcherDelegate(textMatch, fileSearcherMultiple, encoding, cts.Token);

                searcher.FilesFound += (sender, arg) =>
                {
                    arg.Files.ForEach(f => files.Add(f));
                };

                await searcher.SearchAsync();

                return files.ToList();
            });
        }


        /// <summary>
        /// Returns a list of files that are contained in specified directories and all their subdirectories and have at least one of specified keywords.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="fileMatch">The delegate that determines algorithm of file selection.</param>
        /// <param name="keywords">The list of searched keywords in text files</param>
        /// <param name="encoding">Encoding of text files</param>
        /// <param name="comparsion">Comparsion options of the searched keywords</param>
        /// <returns>List of found files.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Task<List<FileInfo>> SearchFilesByTextAsync(IEnumerable<string> folders,
            Func<FileInfo, bool> fileMatch,
            IEnumerable<string> keywords,
            Encoding encoding,
            StringComparison comparsion = StringComparison.Ordinal)
        {
            CheckFolders(folders);
            CheckDelegate(fileMatch);

            CheckKeywords(keywords);
            CheckEncoding(encoding);

            return Task.Run(async () =>
            {
                var cts = new CancellationTokenSource();
                var files = new ConcurrentBag<FileInfo>();

                var fileSearcherMultiple =
                    new FileSearcherMultiple(folders.ToList(), fileMatch, cts);

                var searcher = new FileTextSearcherKeywords(keywords, comparsion, fileSearcherMultiple, encoding, cts.Token);

                searcher.FilesFound += (sender, arg) =>
                {
                    arg.Files.ForEach(f => files.Add(f));
                };

                await searcher.SearchAsync();

                return files.ToList();
            });
        }


        /// <summary>
        /// Returns a list of files that are contained in specified directories and all their subdirectories and match specified filtering delegate.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="textMatch">The delegate that determines algorithm of file selection among already found files.</param>
        /// <param name="encoding">Encoding of text files</param>
        /// <returns>List of found files.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Task<List<FileInfo>> SearchFilesByTextAsync(IEnumerable<string> folders,
            string pattern,
            Func<string, bool> textMatch,
            Encoding encoding)
        {
            CheckFolders(folders);
            CheckPattern(pattern);

            CheckDelegate(textMatch);
            CheckEncoding(encoding);

            return Task.Run(async () =>
            {
                var cts = new CancellationTokenSource();
                var files = new ConcurrentBag<FileInfo>();

                var fileSearcherMultiple =
                    new FileSearcherMultiple(folders.ToList(), pattern, cts);

                var searcher = new FileTextSearcherDelegate(textMatch, fileSearcherMultiple, encoding, cts.Token);

                searcher.FilesFound += (sender, arg) =>
                {
                    arg.Files.ForEach(f => files.Add(f));
                };

                await searcher.SearchAsync();

                return files.ToList();
            });
        }


        /// <summary>
        /// Returns a list of files that are contained in specified directories and all their subdirectories and have at least one of specified keywords.
        /// </summary>
        /// <param name="folders">Start search directories.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="keywords">The list of searched keywords in text files</param>
        /// <param name="encoding">Encoding of text files</param>
        /// <param name="comparsion">Comparsion options of the searched keywords</param>
        /// <returns>List of found files.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Task<List<FileInfo>> SearchFilesByTextAsync(IEnumerable<string> folders,
            string pattern,
            IEnumerable<string> keywords,
            Encoding encoding,
            StringComparison comparsion = StringComparison.Ordinal)
        {
            CheckFolders(folders);
            CheckPattern(pattern);

            CheckKeywords(keywords);
            CheckEncoding(encoding);

            return Task.Run(async () =>
            {
                var cts = new CancellationTokenSource();
                var files = new ConcurrentBag<FileInfo>();

                var fileSearcherMultiple =
                    new FileSearcherMultiple(folders.ToList(), pattern, cts);

                var searcher = new FileTextSearcherKeywords(keywords, comparsion, fileSearcherMultiple, encoding, cts.Token);

                searcher.FilesFound += (sender, arg) =>
                {
                    arg.Files.ForEach(f => files.Add(f));
                };

                await searcher.SearchAsync();

                return files.ToList();
            });
        }


        #region Checking methods

        private static void CheckFolders(IEnumerable<string> folders)
        {
            if (folders == null)
                throw new ArgumentNullException(nameof(folders), "Argument is null.");

            if (!folders.Any())
                throw new ArgumentException("Argument is an empty list.", nameof(folders));

            foreach (var folder in folders)
                CheckFolder(folder);
        }


        private static void CheckKeywords(IEnumerable<string> keywords)
        {
            if (keywords == null)
                throw new ArgumentNullException(nameof(keywords), "Argument is null.");

            if (!keywords.Any())
                throw new ArgumentException("Argument is an empty list.", nameof(keywords));
        }


        private static void CheckFolder(string folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder), "Argument is null.");

            if (folder == String.Empty)
                throw new ArgumentException("Argument is not valid.", nameof(folder));

            DirectoryInfo dir = new DirectoryInfo(folder);

            if (!dir.Exists)
                throw new ArgumentException("Argument does not represent an existing directory.", nameof(folder));
        }


        private static void CheckPattern(string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern), "Argument is null.");

            if (pattern == String.Empty)
                throw new ArgumentException("Argument is not valid.", nameof(pattern));
        }


        private static void CheckDelegate(Delegate isValid)
        {
            if (isValid == null)
                throw new ArgumentNullException(nameof(isValid), "Argument is null.");
        }


        private static void CheckEncoding(Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding), "Argument is null.");
        }

        #endregion


        #endregion
    }
}
