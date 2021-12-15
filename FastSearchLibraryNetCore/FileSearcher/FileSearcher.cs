﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{

    /// <summary>
    /// Represents a class for fast file search.
    /// </summary>
    public class FileSearcher
    {
        #region Instance members

        private FileSearcherBase searcher;

        private CancellationTokenSource tokenSource;

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
        /// Event fires when search process is completed or stopped. 
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


        #region FilePatternSearcher constructors 

        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, string pattern, ExecuteHandlers handlerOption)
        {
            CheckFolder(folder);

            CheckPattern(pattern);
            
            searcher = new FilePatternSearcher(folder, pattern, handlerOption);
        }


        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, string pattern) : this(folder, pattern, ExecuteHandlers.InCurrentTask)
        {
        }


        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder) : this(folder, "*", ExecuteHandlers.InCurrentTask)
        {
        }

        #endregion


        #region FileDelegateSearcher constructors


        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, Func<FileInfo, bool> isValid, ExecuteHandlers handlerOption)
        {
            CheckFolder(folder);

            CheckDelegate(isValid);

            searcher = new FileDelegateSearcher(folder, isValid, handlerOption);
        }


        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, Func<FileInfo, bool> isValid)
            : this(folder, isValid, ExecuteHandlers.InCurrentTask)
        {
        }

        #endregion


        #region FileCancellationPatternSearcher constructors

        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
        /// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, string pattern, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption, bool suppressOperationCanceledException)
        {
            CheckFolder(folder);

            CheckPattern(pattern);

            CheckTokenSource(tokenSource);

            searcher = new FileCancellationPatternSearcher(folder, pattern, tokenSource.Token, handlerOption, suppressOperationCanceledException);
            this.tokenSource = tokenSource;
        }


        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, string pattern, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption)
            : this(folder, pattern, tokenSource, handlerOption, true)
        {
        }


        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, string pattern, CancellationTokenSource tokenSource) 
            : this(folder, pattern, tokenSource, ExecuteHandlers.InCurrentTask, true)
        {
        }


        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, CancellationTokenSource tokenSource) 
            : this(folder, "*", tokenSource, ExecuteHandlers.InCurrentTask, true)
        {
        }

        #endregion


        #region FileCancellationDelegateSearcher constructors

        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
        /// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, Func<FileInfo, bool> isValid, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption, bool suppressOperationCanceledException)
        {
            CheckFolder(folder);

            CheckDelegate(isValid);

            CheckTokenSource(tokenSource);

            searcher = new FileCancellationDelegateSearcher(folder, isValid, tokenSource.Token, handlerOption, suppressOperationCanceledException);
            this.tokenSource = tokenSource;
        }


        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, Func<FileInfo, bool> isValid, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption)
            : this(folder, isValid, tokenSource, handlerOption, true)
        { 
        }


        /// <summary>
        /// Initializes a new instance of FileSearcher class.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public FileSearcher(string folder, Func<FileInfo, bool> isValid, CancellationTokenSource tokenSource)
            : this(folder, isValid, tokenSource, ExecuteHandlers.InCurrentTask, true)
        {
        }

        #endregion


        #region Checking methods

        private void CheckFolder(string folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder), "Argument is null.");
              
            if (folder == String.Empty)
                throw new ArgumentException("Argument is not valid.", nameof(folder));

            DirectoryInfo dir = new DirectoryInfo(folder);

            if (!dir.Exists)
                throw new ArgumentException("Argument does not represent an existing directory.", nameof(folder));
        }


        private void CheckPattern(string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern), "Argument is null.");

            if (pattern == String.Empty)
                throw new ArgumentException("Argument is not valid.", nameof(pattern));
        }


        private void CheckDelegate(Func<FileInfo, bool> isValid)
        {
            if (isValid == null)
                throw new ArgumentNullException(nameof(isValid), "Argument is null.");
        }


        private void CheckTokenSource(CancellationTokenSource tokenSource)
        {
            if (tokenSource == null)
                throw new ArgumentNullException(nameof(tokenSource), "Argument is null.");
        }
 

        #endregion


        /// <summary>
        /// Starts a file search operation with realtime reporting using several threads in thread pool.
        /// </summary>
        public void StartSearch()
        {
            searcher.StartSearch();
        }


        /// <summary>
        /// Starts a file search operation with realtime reporting using several threads in thread pool as an asynchronous operation.
        /// </summary>
        public Task StartSearchAsync()
        {
            if (searcher is FileCancellationSearcherBase)
            {
                return Task.Run(() =>
                {
                   StartSearch();
                    
                }, tokenSource.Token);
            }

            return Task.Run(() =>
            {
                StartSearch();
            });
        }


        /// <summary>
        /// Stops a file search operation.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void StopSearch()
        {
            if (tokenSource == null)
                throw new InvalidOperationException("Impossible to stop operation without instance of CancellationTokenSource.");

            tokenSource.Cancel();
        }

        #endregion


        #region Static members

        #region Public members


        /// <summary>
        /// Returns a list of files that are contained in directory and all subdirectories.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <returns>List of finding files</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        static public List<FileInfo> GetFiles(string folder, string pattern = "*")
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;
            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                if (directories.Length == 0)
                    return new List<FileInfo>(dirInfo.GetFiles(pattern));
            }
            catch (UnauthorizedAccessException ex)
            {
                return new List<FileInfo>();
            }
            catch (DirectoryNotFoundException ex)
            {
                return new List<FileInfo>();
            }

            List<FileInfo> result = new List<FileInfo>();

            foreach (var d in directories)
            {
                result.AddRange(GetFiles(d.FullName, pattern));
            }

            try
            {
                result.AddRange(dirInfo.GetFiles(pattern));
            }
            catch (UnauthorizedAccessException ex)
            {
            }
            catch (PathTooLongException ex)
            {
            }
            catch (DirectoryNotFoundException ex)
            {
            }

            return result;
        }



        /// <summary>
        /// Returns a list of files that are contained in directory and all subdirectories.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <returns>List of finding files.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        static public List<FileInfo> GetFiles(string folder, Func<FileInfo, bool> isValid)
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;
            List<FileInfo> resultFiles = new List<FileInfo>();

            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                if (directories.Length == 0)
                {
                    FileInfo[] files = dirInfo.GetFiles();

                    foreach (var file in files)
                        if (isValid(file))
                            resultFiles.Add(file);

                    return resultFiles;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return new List<FileInfo>();
            }
            catch (PathTooLongException ex)
            {
                return new List<FileInfo>();
            }
            catch (DirectoryNotFoundException ex)
            {
                return new List<FileInfo>();
            }

            foreach (var d in directories)
            {
                resultFiles.AddRange(GetFiles(d.FullName, isValid));
            }

            try
            {
                FileInfo[] files = dirInfo.GetFiles();

                foreach (var file in files)
                    if (isValid(file))
                        resultFiles.Add(file);
            }
            catch (UnauthorizedAccessException ex)
            {
            }
            catch (PathTooLongException ex)
            {
            }
            catch (DirectoryNotFoundException ex)
            {
            }

            return resultFiles;
        }



        /// <summary>
        /// Returns a list of files that are contained in directory and all subdirectories as an asynchronous operation.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        static public Task<List<FileInfo>> GetFilesAsync(string folder, string pattern = "*")
        {
            return Task.Run<List<FileInfo>>(() =>
            {
                return GetFiles(folder, pattern);
            });
        }



        /// <summary>
        /// Returns a list of files that are contained in directory and all subdirectories as an asynchronous operation.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        static public Task<List<FileInfo>> GetFilesAsync(string folder, Func<FileInfo, bool> isValid)
        {
            return Task.Run<List<FileInfo>>(() =>
            {
                return GetFiles(folder, isValid);
            });
        }



        /// <summary>
        /// Returns a list of files that are contained in directory and all subdirectories using several threads of thread pool.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <returns>List of finding files.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        static public List<FileInfo> GetFilesFast(string folder, string pattern = "*")
        {
            ConcurrentBag<FileInfo> files = new ConcurrentBag<FileInfo>();

            List<DirectoryInfo> startDirs = GetStartDirectories(folder, files, pattern);

            startDirs.AsParallel().ForAll((d) =>
            {
                GetStartDirectories(d.FullName, files, pattern).AsParallel().ForAll((dir) =>
                {
                    GetFiles(dir.FullName, pattern).ForEach((f) => files.Add(f));
                });
            });

            return files.ToList();
        }



        /// <summary>
        /// Returns a list of files that are contained in directory and all subdirectories using several threads of thread pool.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <returns>List of finding files.</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        static public List<FileInfo> GetFilesFast(string folder, Func<FileInfo, bool> isValid)
        {
            ConcurrentBag<FileInfo> files = new ConcurrentBag<FileInfo>();

            List<DirectoryInfo> startDirs = GetStartDirectories(folder, files, isValid);

            startDirs.AsParallel().ForAll((d) =>
            {
                GetStartDirectories(d.FullName, files, isValid).AsParallel().ForAll((dir) =>
                {
                    GetFiles(dir.FullName, isValid).ForEach((f) => files.Add(f));
                });
            });

            return files.ToList();
        }



        /// <summary>
        /// Returns a list of files that are contained in directory and all subdirectories using several threads of thread pool as an asynchronous operation.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="pattern">The search pattern.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        static public Task<List<FileInfo>> GetFilesFastAsync(string folder, string pattern = "*")
        {
            return Task.Run<List<FileInfo>>(() =>
            {
                return GetFilesFast(folder, pattern);
            });
        }



        /// <summary>
        /// Returns a list of files that are contained in directory and all subdirectories using several threads of thread pool as an asynchronous operation.
        /// </summary>
        /// <param name="folder">The start search directory.</param>
        /// <param name="isValid">The delegate that determines algorithm of file selection.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        static public Task<List<FileInfo>> GetFilesFastAsync(string folder, Func<FileInfo, bool> isValid)
        {
            return Task.Run<List<FileInfo>>(() =>
            {
                return GetFilesFast(folder, isValid);
            });
        }


        #endregion

        #region Private members

        static private List<DirectoryInfo> GetStartDirectories(string folder, ConcurrentBag<FileInfo> files, string pattern)
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;
            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                foreach (var f in dirInfo.GetFiles(pattern))
                {
                    files.Add(f);
                }

                if (directories.Length > 1)
                    return new List<DirectoryInfo>(directories);

                if (directories.Length == 0)
                    return new List<DirectoryInfo>();

            }
            catch (UnauthorizedAccessException ex)
            {
                return new List<DirectoryInfo>();
            }
            catch (PathTooLongException ex)
            {
                return new List<DirectoryInfo>();
            }
            catch (DirectoryNotFoundException ex)
            {
                return new List<DirectoryInfo>();
            }

            return GetStartDirectories(directories[0].FullName, files, pattern);
        }



        static private List<DirectoryInfo> GetStartDirectories(string folder, ConcurrentBag<FileInfo> resultFiles, Func<FileInfo, bool> isValid)
        {
            DirectoryInfo dirInfo = null;
            DirectoryInfo[] directories = null;

            try
            {
                dirInfo = new DirectoryInfo(folder);
                directories = dirInfo.GetDirectories();

                FileInfo[] files = dirInfo.GetFiles();

                foreach (var file in files)
                    if (isValid(file))
                        resultFiles.Add(file);

                if (directories.Length > 1)
                    return new List<DirectoryInfo>(directories);

                if (directories.Length == 0)
                    return new List<DirectoryInfo>();
            }
            catch (UnauthorizedAccessException ex)
            {
                return new List<DirectoryInfo>();
            }
            catch (PathTooLongException ex)
            {
                return new List<DirectoryInfo>();
            }
            catch (DirectoryNotFoundException ex)
            {
                return new List<DirectoryInfo>();
            }

            return GetStartDirectories(directories[0].FullName, resultFiles, isValid);
        }

        #endregion

        #endregion

    }
}
