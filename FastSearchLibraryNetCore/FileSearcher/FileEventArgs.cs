using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FastSearchLibrary
{
    /// <summary>
    /// Provides data for FilesFound event.
    /// </summary>
    public class FileEventArgs: EventArgs
    {
         /// <summary>
         /// Gets a list of finding files.
         /// </summary>
         public List<FileInfo> Files { get; private set; }

         private CancellationToken? cancellationToken;

         /// <summary>
         /// Gets a token for operation cancelling.
         /// </summary>
         public CancellationToken CancellationToken 
         {
            get
            {
                return cancellationToken ?? throw new NotSupportedException("It is impossible to cancel operation without CancellationToken");
            }

            private set
            {
                cancellationToken = value;
            }
         }

         /// <summary>
         /// Initialize a new instance of FileEventArgs class that describes a FilesFound event.
         /// </summary>
         /// <param name="files">The list of found files.</param>
         public FileEventArgs(List<FileInfo> files)
         {
             Files = files;
         }

         /// <summary>
         /// Initialize a new instance of FileEventArgs class that describes a FilesFound event.
         /// </summary>
         /// <param name="files">The list of found files.</param>
         /// <param name="token">Instance of CancellationToken</param>
         public FileEventArgs(List<FileInfo> files, CancellationToken token)
         {
            Files = files;
            CancellationToken = token;
         }
    }
}
