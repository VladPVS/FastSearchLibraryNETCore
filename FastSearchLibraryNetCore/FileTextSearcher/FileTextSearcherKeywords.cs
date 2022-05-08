using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FastSearchLibrary
{
    class FileTextSearcherKeywords : FileTextSearcherBase
    {
        private IEnumerable<string> keywords;
        private readonly StringComparison comparsion;

        public FileTextSearcherKeywords(IEnumerable<string> keywords,
               StringComparison comparsion,
               FileSearcherMultiple fileSearcher,
               Encoding encoding, CancellationToken cancellationToken)
            : base(fileSearcher, encoding, cancellationToken)
        {
            this.keywords = keywords;
            this.comparsion = comparsion;
        }

        public override bool IsTextMatched(string text)
        {
            return keywords.Any(k => text.Contains(k, comparsion));
        }
    }
}
