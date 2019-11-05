using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
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
        private const string Field_Id = "Id";
        private const string Field_String = "Reference";
        private IndexWriter _writer;

        private bool firstUse = true;

        public Index()
        {
            // Ensures index backwards compatibility
            var AppLuceneVersion = LuceneVersion.LUCENE_48;

            var applicationRoot = this.GetApplicationRoot();
            if (string.IsNullOrWhiteSpace(applicationRoot))
            {
                applicationRoot = AppDomain.CurrentDomain.BaseDirectory;
            }
            var indexLocation = applicationRoot.Contains('\\') ? applicationRoot + @"\Data\Index" : applicationRoot + @"/Data/Index";
            
            //var otherdir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = FSDirectory.Open(indexLocation);

            //create an analyzer to process the text
            var analyzer = new StandardAnalyzer(AppLuceneVersion);
            //_idAnalyser = new SimpleAnalyzer(AppLuceneVersion);

            //create an index writer
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            _writer = new IndexWriter(dir, indexConfig);

        }

        public bool FirstUse
        {
            get => firstUse;
            set => firstUse = value;
        }

        public void AddOrUpdate(Reference reference)
        {
            var IndexString = string.Join(' ',
                new List<string>() {reference.Firstname, reference.Middlename, reference.Lastname, reference.Summary, reference.Author, reference.Bibliography, reference.Journal, reference.Keywords, reference.Pages, reference.Title, reference.Url, reference.Volume, reference.Year}
                    .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());

            if (IndexString.Length > 0)
            {
                Document doc = new Document
                {
                    new StringField(Field_Id, reference.Id.ToString(), Field.Store.YES),
                    // StringField indexes but doesn't tokenize
                    new TextField(Field_String, IndexString, Field.Store.YES),
                    //new TextField("favoritePhrase", source.FavoritePhrase, Field.Store.YES)
                };
                //            var id = new Term("Id", reference.Id.ToString());
                //            _writer.DeleteDocuments(id);
                _writer.UpdateDocument(new Term(Field_Id, reference.Id.ToString()), doc);
                // _writer.AddDocument(doc);
                _writer.Flush(triggerMerge: false, applyAllDeletes: false);
            }
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        public IEnumerable<Guid> SearchReference(string terms)
        {
            var lower = terms.ToLower();
            var items = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var phrase = new PhraseQuery();
            foreach (var item in items)
            {
                phrase.Add(new Term(Field_String, item));
            }
            var searcher = new IndexSearcher(_writer.GetReader(applyAllDeletes: true));
            var hits = searcher.Search(phrase, 20 /* top 20 */).ScoreDocs;
            foreach (var hit in hits)
            {
                var foundDoc = searcher.Doc(hit.Doc);
                yield return Guid.Parse(foundDoc.Get(Field_Id));
            }
        }

        public void Delete(Guid newGuid)
        {
            _writer.DeleteDocuments(new Term(Field_Id, newGuid.ToString()));
            _writer.Flush(triggerMerge: false, applyAllDeletes: false);
        }

        public int IndexCount()
        {
            return _writer.MaxDoc;
        }
        public void ClearIndex()
        {
            _writer.DeleteAll();
            _writer.Commit();
        }
        public string GetApplicationRoot()
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return appRoot;
        }
    }
}
