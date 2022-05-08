using System;
using System.Text;
using System.Threading;

namespace FastSearchLibrary
{
    class FileTextSearcherDelegate : FileTextSearcherBase
    {
        private readonly CancellationToken cancellationToken;

        private Func<string, bool> TextMatcher { get; }

        public FileTextSearcherDelegate(Func<string, bool> textMatch,
            FileSearcherMultiple fileSearcher, Encoding encoding,
            CancellationToken cancellationToken)
            : base(fileSearcher, encoding, cancellationToken)
        {
            TextMatcher = textMatch;
            this.cancellationToken = cancellationToken;
        }

        public override bool IsTextMatched(string text)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            return TextMatcher(text);
        }
    }
}
