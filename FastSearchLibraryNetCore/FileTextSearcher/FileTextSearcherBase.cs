using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
    abstract class FileTextSearcherBase
    {
        protected Encoding encoding;

        protected FileSearcherMultiple fileSearcher;

        protected ConcurrentBag<Task> taskHandlers;

        private readonly CancellationToken cancellationToken;

        public abstract bool IsTextMatched(string text);

        public virtual async Task SearchAsync()
        {
            var files = new List<FileInfo>();

            fileSearcher.FilesFound += (sender, arg) =>
            {
                var task = Task.Run(async () =>
                {
                    try
                    {
                        var foundFiles = new List<FileInfo>();

                        string text;

                        foreach (var f in arg.Files)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return;

                            try
                            {
                                text = await File.ReadAllTextAsync(f.FullName, encoding);
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }

                            if (IsTextMatched(text))
                            {
                                foundFiles.Add(f);
                            }
                        }

                        if (cancellationToken.IsCancellationRequested)
                            return;

                        if (foundFiles.Count > 0)
                        {
                            OnFilesFound(foundFiles, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                });

                taskHandlers.Add(task);
            };

            bool isCanceled = false;

            try
            {
                await fileSearcher.StartSearchAsync();
            }
            catch (OperationCanceledException ex)
            {
                isCanceled = true;
            }
            finally
            {
                await Task.WhenAll(taskHandlers);
                OnSearchCompleted(isCanceled);
            }
        }

        public event EventHandler<FileEventArgs> FilesFound;

        public event EventHandler<SearchCompletedEventArgs> SearchCompleted;

        protected virtual void OnFilesFound(List<FileInfo> files, CancellationToken token)
        {
            EventHandler<FileEventArgs> handler = FilesFound;

            handler?.Invoke(this, new FileEventArgs(files, token));
        }

        protected virtual void OnSearchCompleted(bool isCanceled)
        {
            EventHandler<SearchCompletedEventArgs> handler = SearchCompleted;

            handler?.Invoke(this, new SearchCompletedEventArgs(isCanceled));
        }

        protected FileTextSearcherBase(FileSearcherMultiple fileSearcher,
            Encoding encoding, CancellationToken cancellationToken)
        {
            this.fileSearcher = fileSearcher;
            this.taskHandlers = new ConcurrentBag<Task>();
            this.encoding = encoding;
            this.cancellationToken = cancellationToken;
        }
    }
}
