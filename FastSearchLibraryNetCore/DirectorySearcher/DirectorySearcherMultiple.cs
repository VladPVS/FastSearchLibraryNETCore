﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
        /// <summary>
        /// Represents a class for fast directory search in multiple directories.
        /// </summary>
        public class DirectorySearcherMultiple
        {
            #region Instance members

            private List<DirectoryCancellationSearcherBase> searchers;

            private CancellationTokenSource tokenSource;

            private bool suppressOperationCanceledException;


            /// <summary>
            /// Event fires when next portion of directories is found. Event handlers are not thread safe. 
            /// </summary>
            public event EventHandler<DirectoryEventArgs> DirectoriesFound
            {
                add
                {
                    searchers.ForEach((s) => s.DirectoriesFound += value);
                }

                remove
                {
                    searchers.ForEach((s) => s.DirectoriesFound -= value);
                }
            }


            /// <summary>
            /// Event fires when search process is completed or stopped.
            /// </summary>
            public event EventHandler<SearchCompletedEventArgs> SearchCompleted;


            /// <summary>
            /// Calls a SearchCompleted event.
            /// </summary>
            /// <param name="isCanceled">Determines whether search process canceled.</param>
            protected virtual void OnSearchCompleted(bool isCanceled)
            {
                EventHandler<SearchCompletedEventArgs> handler = SearchCompleted;

                if (handler != null)
                {
                    var arg = new SearchCompletedEventArgs(isCanceled);

                    handler(this, arg);
                }
            }


            #region DirectoryCancellationDelegateSearcher constructors

            /// <summary>
            /// Initialize a new instance of DirectorySearcherMultiple class.
            /// </summary>
            /// <param name="folders">Start search directories.</param>
            /// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
            /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
            /// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
            /// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            public DirectorySearcherMultiple(List<string> folders, Func<DirectoryInfo, bool> isValid, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption, bool suppressOperationCanceledException)
            {
                 CheckFolders(folders);

                 CheckDelegate(isValid);

                 CheckTokenSource(tokenSource);

                 searchers = new List<DirectoryCancellationSearcherBase>();

                 this.suppressOperationCanceledException = suppressOperationCanceledException;

                 foreach (var folder in folders)
                 {
                     searchers.Add(new DirectoryCancellationDelegateSearcher(folder, isValid, tokenSource.Token, handlerOption, false));
                 }

                 this.tokenSource = tokenSource;
            }


            /// <summary>
            /// Initialize a new instance of DirectorySearcherMultiple class.
            /// </summary>
            /// <param name="folders">Start search directories.</param>
            /// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
            /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
            /// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            public DirectorySearcherMultiple(List<string> folders, Func<DirectoryInfo, bool> isValid, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption)
                : this(folders, isValid, tokenSource, ExecuteHandlers.InCurrentTask, true)
            {
            }


            /// <summary>
            /// Initialize a new instance of DirectorySearcherMultiple class.
            /// </summary>
            /// <param name="folders">Start search directories.</param>
            /// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
            /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            public DirectorySearcherMultiple(List<string> folders, Func<DirectoryInfo, bool> isValid, CancellationTokenSource tokenSource)
                : this(folders, isValid, tokenSource, ExecuteHandlers.InCurrentTask, true)
            {
            }


            #endregion


            #region DirectoryCancellationPatternSearcher constructors

            /// <summary>
            /// Initialize a new instance of DirectorySearchMultiple class. 
            /// </summary>
            /// <param name="folders">Start search directories.</param>
            /// <param name="pattern">The search pattern.</param>
            /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
            /// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
            /// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            public DirectorySearcherMultiple(List<string> folders, string pattern, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption, bool suppressOperationCanceledException)
            {
                CheckFolders(folders);

                CheckPattern(pattern);

                CheckTokenSource(tokenSource);

                searchers = new List<DirectoryCancellationSearcherBase>();

                this.suppressOperationCanceledException = suppressOperationCanceledException;

                foreach (var folder in folders)
                {
                    searchers.Add(new DirectoryCancellationPatternSearcher(folder, pattern, tokenSource.Token, handlerOption, false));
                }

                this.tokenSource = tokenSource;
            }


            /// <summary>
            /// Initialize a new instance of DirectorySearchMultiple class. 
            /// </summary>
            /// <param name="folders">Start search directories.</param>
            /// <param name="pattern">The search pattern.</param>
            /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
            /// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            public DirectorySearcherMultiple(List<string> folders, string pattern, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption)
                : this (folders, pattern, tokenSource, handlerOption, true)
            {
            }


            /// <summary>
            /// Initialize a new instance of DirectorySearchMultiple class. 
            /// </summary>
            /// <param name="folders">Start search directories.</param>
            /// <param name="pattern">The search pattern.</param>
            /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            public DirectorySearcherMultiple(List<string> folders, string pattern, CancellationTokenSource tokenSource)
                : this (folders, pattern, tokenSource, ExecuteHandlers.InCurrentTask, true)
            {
            }


            /// <summary>
            /// Initialize a new instance of DirectorySearchMultiple class. 
            /// </summary>
            /// <param name="folders">Start search directories.</param>
            /// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="ArgumentNullException"></exception>
            public DirectorySearcherMultiple(List<string> folders, CancellationTokenSource tokenSource)
                : this (folders, "*", tokenSource, ExecuteHandlers.InCurrentTask, true)
            {

            }

            #endregion


            #region Checking methods
            
            private void CheckFolders(List<string> folders)
            {
                if (folders == null)
                    throw new ArgumentNullException(nameof(folders), "Argument is null.");

                if (folders.Count == 0)
                    throw new ArgumentException("Argument is an empty list.", nameof(folders));

                foreach (var folder in folders)
                    CheckFolder(folder);
            }


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


            private void CheckDelegate(Func<DirectoryInfo, bool> isValid)
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
            /// Starts a directory search operation with realtime reporting using several threads in thread pool.
            /// </summary>
            public void StartSearch()
            {
                try
                {
                    searchers.ForEach(s =>
                    {
                        s.StartSearch();
                    });
                }
                catch (OperationCanceledException ex)
                {
                    OnSearchCompleted(true);
                    if (!suppressOperationCanceledException)
                        throw;
                    return;
                }

                OnSearchCompleted(false);
            }


            /// <summary>
            /// Starts a directory search operation with realtime reporting using several threads in thread pool as an asynchronous operation.
            /// </summary>
            public Task StartSearchAsync()
            {
                return Task.Run(() =>
                {
                    StartSearch();

                }, tokenSource.Token);
            }


            /// <summary>
            /// Stops a directory search operation.
            /// </summary>
            public void StopSearch()
            {
                tokenSource.Cancel();
            }

            #endregion
        }
}
