using System;
using System.Net.Mime;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Nbic.References.Public.Models;

namespace Nbic.Indexer
{
    public class Index: IDisposable
    {
        private IndexWriter _writer;
        private SimpleAnalyzer _idAnalyser;

        public Index()
        {
            // Ensures index backwards compatibility
            var AppLuceneVersion = LuceneVersion.LUCENE_48;

            var indexLocation = @"C:\Index";
            var dir = FSDirectory.Open(indexLocation);

            //create an analyzer to process the text
            var analyzer = new StandardAnalyzer(AppLuceneVersion);
            //_idAnalyser = new SimpleAnalyzer(AppLuceneVersion);

            //create an index writer
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            _writer = new IndexWriter(dir, indexConfig);

        }
        public void AddOrUpdate(Reference reference)
        {
            Document doc = new Document
            {
                new StringField("Id", reference.Id.ToString(),Field.Store.YES),
                // StringField indexes but doesn't tokenize
                new TextField("name", reference.Title, Field.Store.YES),
                //new TextField("favoritePhrase", source.FavoritePhrase, Field.Store.YES)
            };
//            var id = new Term("Id", reference.Id.ToString());
//            _writer.DeleteDocuments(id);
            _writer.UpdateDocument(new Term("Id", reference.Id.ToString()), doc);
           // _writer.AddDocument(doc);
            _writer.Flush(triggerMerge: false, applyAllDeletes: false);
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        public Guid SearchReference(string terms)
        {
            Guid result = Guid.Empty;
            var lower = terms.ToLower();
            var items = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var phrase = new PhraseQuery();
            foreach (var item in items)
            {
                phrase.Add(new Term("name", item));
            }
            var searcher = new IndexSearcher(_writer.GetReader(applyAllDeletes: true));
            var hits = searcher.Search(phrase, 20 /* top 20 */).ScoreDocs;
            foreach (var hit in hits)
            {
                var foundDoc = searcher.Doc(hit.Doc);
                result = Guid.Parse(foundDoc.Get("Id"));
            }

            return result;
        }

        public void Delete(Guid newGuid)
        {
            _writer.DeleteDocuments(new Term("Id", newGuid.ToString()));
            _writer.Flush(triggerMerge: false, applyAllDeletes: false);
        }
    }
}
